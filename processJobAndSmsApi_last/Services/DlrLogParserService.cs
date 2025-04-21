using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using processJobAndSmsApi.Models;
using processJobAndSmsApi.Models.Configuration;

public class DlrLogParserService : IDisposable
{
    private readonly DlrSettings _dlrSettings;
    private readonly string _connectionString;
    private readonly int _batchSize;
    private readonly ILogger<DlrLogParserService> _logger;
    private readonly SemaphoreSlim _batchLock = new SemaphoreSlim(1, 1);
    private List<string> _currentBatch = new List<string>();
    private Task _processTask = Task.CompletedTask;
    private readonly Timer _flushTimer;
    private bool _disposed;
    private readonly ConcurrentBag<MySqlConnection> _connectionPool = new();

    // Compiled regex patterns for performance
    private static readonly Dictionary<string, Regex> _regexPatterns = new()
    {
        { "MsgData", new Regex(@"\[msg:\d{1,4}:(id.{1,})\] \[udh", RegexOptions.Compiled) },
        { "SmscName", new Regex(@"\[SMSC:(.{3,10})\b\]", RegexOptions.Compiled) },
        { "DlrMask", new Regex(@"dlr_mask=(\d{1,})&", RegexOptions.Compiled) },
        { "SenderName", new Regex(@"\[from:(.{4,15})\b\]", RegexOptions.Compiled) },
        { "MobileNumber", new Regex(@"\[to:(.{4,15})\b\]", RegexOptions.Compiled) },
        { "Success", new Regex(@"done date:(.\d{6,})", RegexOptions.Compiled) },
        { "DlrErr", new Regex(@"err:(\d{1,})", RegexOptions.Compiled) },
        { "Status", new Regex(@"stat:(.{1,}) er", RegexOptions.Compiled) },
        { "SentAtUtc", new Regex(@"submit date:(.\d{6,})", RegexOptions.Compiled) },
        { "UserId", new Regex(@"user_id=([0-9]+)", RegexOptions.Compiled) },
        { "JobId", new Regex(@"job_id=([a-z0-9-]+)", RegexOptions.Compiled) },
        { "MsgId", new Regex(@"msg_id=([a-z0-9-]+)", RegexOptions.Compiled) },
        { "Fid", new Regex(@"\[FID:([a-z0-9-]+)\b\]", RegexOptions.Compiled) }
    };

    public DlrLogParserService(string connectionString, IOptions<DlrSettings> dlrSettings, ILogger<DlrLogParserService> logger)
    {
        _dlrSettings = dlrSettings.Value;
        _connectionString = connectionString;
        _batchSize = Math.Min(_dlrSettings.BatchSize, 20000); // Increased max batch size
        _logger = logger;
        _flushTimer = new Timer(FlushPartialBatch, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
    }

    public async Task AddLogLineAsync(string line, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        await _batchLock.WaitAsync(cancellationToken);
        try
        {
            _currentBatch.Add(line);
            
            // Increased target batch size
            var targetBatchSize = Math.Min(_batchSize, 20000);
            if (_currentBatch.Count >= targetBatchSize || 
                (_currentBatch.Count > 0 && _processTask.IsCompleted))
            {
                await StartBatchProcessing(cancellationToken);
            }
        }
        finally
        {
            _batchLock.Release();
        }
    }

    public async Task AddBatchAsync(IEnumerable<string> lines, CancellationToken cancellationToken)
    {
        await _batchLock.WaitAsync(cancellationToken);
        try
        {
            _currentBatch.AddRange(lines);
            if (_processTask.IsCompleted)
            {
                await StartBatchProcessing(cancellationToken);
            }
        }
        finally
        {
            _batchLock.Release();
        }
    }

    private async Task StartBatchProcessing(CancellationToken cancellationToken)
    {
        var batchToProcess = _currentBatch;
        _currentBatch = new List<string>();
        _processTask = ProcessBatchAsync(batchToProcess, cancellationToken);
        await _processTask;
    }

    private void FlushPartialBatch(object state)
    {
        if (_batchLock.Wait(0))
        {
            try
            {
                if (_currentBatch.Count > 0 && _processTask.IsCompleted)
                {
                    _ = StartBatchProcessing(CancellationToken.None);
                }
            }
            finally
            {
                _batchLock.Release();
            }
        }
    }

    private async Task ProcessBatchAsync(List<string> lines, CancellationToken cancellationToken)
    {
        if (lines.Count == 0) return;

        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting DLR batch processing at {StartTime} UTC with {RecordCount} records", 
            startTime.ToString("yyyy-MM-dd HH:mm:ss"), lines.Count);

        var stopwatch = Stopwatch.StartNew();
        var sqlStatements = new ConcurrentBag<string>();
        
        await Task.WhenAll(lines.Select(async line => 
        {
            var row = ParseLine(line);
            if (row != null)
            {
                ProcessRecord(row);
                var sql = GenerateUpdateSql(row);
                if (!string.IsNullOrEmpty(sql))
                {
                    sqlStatements.Add(sql);
                }
            }
            await Task.CompletedTask;
        }));

        // Increased chunk size for bulk updates
        const int chunkSize = 2000;
        var chunks = sqlStatements.Chunk(chunkSize);        
        foreach (var chunk in chunks)
        {
            await ExecuteBulkSqlAsync(chunk.ToList(), cancellationToken);
        }

        var endTime = DateTime.UtcNow;
        _logger.LogInformation("Completed DLR batch processing at {EndTime} UTC. Processed {RecordCount} records in {ElapsedMilliseconds}ms", 
            endTime.ToString("yyyy-MM-dd HH:mm:ss"), lines.Count, stopwatch.ElapsedMilliseconds);
    }
    
    private async Task<MySqlConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connectionPool.TryTake(out var connection))
        {
            if (connection.State == ConnectionState.Open)
                return connection;
            connection.Dispose();
        }
        
        var newConnection = new MySqlConnection(_connectionString);
        await newConnection.OpenAsync(ct);
        return newConnection;
    }

    private void ReturnConnection(MySqlConnection connection)
    {
        if (connection?.State == ConnectionState.Open)
        {
            _connectionPool.Add(connection);
        }
        else
        {
            connection?.Dispose();
        }
    }

    // private async Task ExecuteBulkSqlAsync(List<string> sqlStatements, CancellationToken cancellationToken)
    // {
    //     if (sqlStatements.Count == 0) return;

    //     // Group updates by WHERE clause for more efficient batch updates
    //     var groupedUpdates = sqlStatements
    //         .GroupBy(sql => 
    //         {
    //             var whereStart = sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
    //             var limitStart = sql.IndexOf("LIMIT", StringComparison.OrdinalIgnoreCase);
    //             return sql.Substring(whereStart, limitStart - whereStart).Trim();
    //         })
    //         .Select(g => 
    //             $"UPDATE users_sms SET {string.Join(", ", g.Select(sql => 
    //             {
    //                 var setStart = sql.IndexOf("SET", StringComparison.OrdinalIgnoreCase) + 3;
    //                 var whereStart = sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
    //                 return sql.Substring(setStart, whereStart - setStart).Trim();
    //             }).Distinct())} {g.Key}");

    //     var connection = await GetConnectionAsync(cancellationToken);
    //     try
    //     {
    //         using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            
    //         foreach (var batch in groupedUpdates)
    //         {
    //             using var command = connection.CreateCommand();
    //             command.Transaction = transaction;
    //             command.CommandText = batch;
    //             await command.ExecuteNonQueryAsync(cancellationToken);
    //         }
            
    //         await transaction.CommitAsync(cancellationToken);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Bulk update failed for {StatementCount} statements", sqlStatements.Count);
    //         throw;
    //     }
    //     finally
    //     {
    //         ReturnConnection(connection);
    //     }
    // }

        private async Task ExecuteBulkSqlAsync(List<string> sqlStatements, CancellationToken cancellationToken)
    {
        if (sqlStatements.Count == 0) return;

        var messageIds = new List<string>();
        var statusUpdates = new List<string>();
        var successUpdates = new List<string>();
        var msgDataUpdates = new List<string>();

        foreach (var sql in sqlStatements)
        {
            var match = Regex.Match(sql, @"SET (.*?) WHERE message_id = '(.*?)'");
            if (match.Success)
            {
                var setClause = match.Groups[1].Value;
                var messageId = match.Groups[2].Value;
                messageIds.Add($"'{messageId}'");

                var status = Regex.Match(setClause, @"status = '(.*?)'");
                statusUpdates.Add(status.Success ? status.Groups[1].Value : "status");
                
                var success = Regex.Match(setClause, @"success = '(.*?)'");
                successUpdates.Add(success.Success ? success.Groups[1].Value : "success");
                
                var msgData = Regex.Match(setClause, @"msgdata = '(.*?)'");
                msgDataUpdates.Add(msgData.Success ? msgData.Groups[1].Value : "msgdata");
            }
        }

        var caseStatements = messageIds.Select((id, i) => 
            $"WHEN {id} THEN '{statusUpdates[i]}'").ToList();
        var successCases = messageIds.Select((id, i) => 
            $"WHEN {id} THEN '{successUpdates[i]}'").ToList();
        var msgDataCases = messageIds.Select((id, i) => 
            $"WHEN {id} THEN '{msgDataUpdates[i]}'").ToList();

        var bulkUpdate = @$"
            UPDATE users_sms 
            SET 
                status = CASE message_id 
                    {string.Join("\n                    ", caseStatements)}
                    ELSE status 
                END,
                success = CASE message_id 
                    {string.Join("\n                    ", successCases)}
                    ELSE success 
                END,
                msgdata = CASE message_id 
                    {string.Join("\n                    ", msgDataCases)}
                    ELSE msgdata 
                END
            WHERE message_id IN ({string.Join(",", messageIds)})
            AND status = 'Submitted'";

        var connection = await GetConnectionAsync(cancellationToken);
        try
        {
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = bulkUpdate;
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk update failed for {StatementCount} statements", sqlStatements.Count);
            throw;
        }
        finally
        {
            ReturnConnection(connection);
        }
    }

    private DlrLogRecord ParseLine(string line)
    {
        var record = new DlrLogRecord();
        
        foreach (var pattern in _regexPatterns)
        {
            var match = pattern.Value.Match(line);
            if (match.Success && match.Groups.Count > 1)
            {
                typeof(DlrLogRecord).GetProperty(pattern.Key)?.SetValue(record, match.Groups[1].Value);
            }
        }

        return record;
    }

    private static readonly ArrayPool<char> _charPool = ArrayPool<char>.Shared;

    private void ProcessRecord(DlrLogRecord record)
    {
        record.SenderName = ProcessPhoneNumber(record.SenderName);
        record.MobileNumber = ProcessPhoneNumber(record.MobileNumber);

        if (!string.IsNullOrEmpty(record.Status))
        {
            record.Status = record.Status.ToUpper() switch
            {
                "DELIVRD" => "Delivered",
                "FAILED" => "Failed",
                "REJECTD" => "Rejected",
                _ => "Failed"
            };
        }

        var dndErrors = new[] { "640", "650", "413", "74", "074", "652" };
        if (!string.IsNullOrEmpty(record.DlrErr) && Array.IndexOf(dndErrors, record.DlrErr) >= 0)
        {
            record.Status = "DND Number";
        }

        if (!string.IsNullOrEmpty(record.Success))
        {
            var matches = Regex.Matches(record.Success, @"\d{2}");
            if (matches.Count >= 5)
            {
                var year = DateTime.Now.ToString("yy").Substring(0, 2);
                var values = new List<string> { year };
                foreach (Match match in matches)
                {
                    values.Add(match.Value);
                }
                
                if (values.Count == 5) values.Add("00");
                record.Success = $"{values[0]}{values[1]}-{values[2]}-{values[3]} {values[4]}:{values[5]}:{values[6]}";
            }
        }

        if (!string.IsNullOrEmpty(record.MsgId))
        {
            record.MessageId = record.MsgId;
        }
    }

    private string ProcessPhoneNumber(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var buffer = _charPool.Rent(input.Length);
        try
        {
            input.AsSpan().CopyTo(buffer);
            var span = new Span<char>(buffer, 0, input.Length);
            
            // Remove '+' and trim
            span = span.Trim();
            if (span.Length > 0 && span[0] == '+')
            {
                span = span.Slice(1);
            }
            
            // Handle dash case
            var dashIndex = span.IndexOf('-');
            if (dashIndex >= 0)
            {
                span = span.Slice(dashIndex + 1).Trim();
            }
            
            return span.ToString();
        }
        finally
        {
            _charPool.Return(buffer);
        }
    }

    private string GenerateUpdateSql(DlrLogRecord record)
    {
        if (string.IsNullOrEmpty(record.MessageId))
            return string.Empty;

        var updates = new List<string>();
        if (!string.IsNullOrEmpty(record.Status))
        {
            updates.Add($"status = '{MySqlHelper.EscapeString(record.Status)}'");
        }
        if (!string.IsNullOrEmpty(record.Success))
        {
            updates.Add($"success = '{MySqlHelper.EscapeString(record.Success)}'");
        }
        if (!string.IsNullOrEmpty(record.MsgData))
        {
            updates.Add($"msgdata = '{MySqlHelper.EscapeString(record.MsgData)}'");
        }

        return updates.Count > 0 
            ? $"UPDATE users_sms SET {string.Join(", ", updates)} WHERE message_id = '{MySqlHelper.EscapeString(record.MessageId)}' AND status = 'Submitted' LIMIT 1;" 
            : string.Empty;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _flushTimer?.Dispose();
            _batchLock?.Dispose();
            
            // Clean up connection pool
            while (_connectionPool.TryTake(out var connection))
            {
                connection.Dispose();
            }
            
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}