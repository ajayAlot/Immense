using processJobAndSmsApi.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;  
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using processJobAndSmsApi.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using processJobAndSmsApi.Models;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Data.Common;
using processJobAndSmsApi.Services;



public class SmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IHttpClientFactory _httpClientFactory; 
    private readonly ConcurrentQueue<string> _urlQueue = new ConcurrentQueue<string>();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(10);



    public SmsService(IConfiguration configuration, ILogger<SmsService> logger, ApplicationDbContext context, UserService userService, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _userService = userService;
        _httpClientFactory = httpClientFactory;
    }


    public string GenerateMessageID(string loggedUserId)
        {
            string messageID = "";
            Random random = new Random();

            for (int i = 0; i < 4; i++)
            {
                string part = GenerateHashPart(loggedUserId, random);
                messageID += (i > 0 ? "-" : "") + part;
            }

            return messageID;
        }

    private static string GenerateHashPart(string loggedUserId, Random random)
    {
        string input = loggedUserId + DateTime.UtcNow.Ticks + random.Next(1000, 9999);
        string hash = ComputeMD5Hash(input).ToLower();
        return FormatHash(hash.Substring(0, 4));
    }

    private static string ComputeMD5Hash(string input)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        return string.Concat(hashBytes.Select(b => b.ToString("x2")));
    }

    private static string FormatHash(string hash)
    {
        return string.Join("", Enumerable.Range(0, hash.Length / 4).Select(i => hash.Substring(i * 4, 4)));
    }

    public int JulianDate(long unixTime)
    {
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        return (int)(dateTime.ToOADate() + 2415018.5 + 0.5); 
    }

    public async Task ExecuteBatchSqlWithLoggingAsync(List<string> batchSql, List<List<MySqlParameter>> batchParams)
    {
        try
        {
            for (int i = 0; i < batchSql.Count; i++)
            {
                var sql = batchSql[i];
                var parameters = batchParams[i].ToArray(); 
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                _logger.LogInformation("Inserted rows: {rowsAffected} ", rowsAffected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the SQL query batch.");
        }
    }

    private void StartUrlProcessingTask()
    {
        Task.Run(async () => await ProcessUrlQueueAsync());
    }

    private async Task ProcessUrlQueueAsync()
    {
        while (true)
        {
            await _signal.WaitAsync();
            if (_urlQueue.TryDequeue(out var url))
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully sent SMS to {url}", url);
                    }
                    else
                    {
                        _logger.LogError("Failed to send SMS to {url}. Status code: {statusCode}", url, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending SMS to {url}", url);
                }
            }
        }
    }

    public async Task<string> BuildSmsSql(Dictionary<string, object> queryData, IServiceScopeFactory serviceScopeFactory, string smsParam, List<string> smppSMSCNames, List<int> smppSMSCIds)
    {
        if (queryData == null)
        {
            _logger.LogError("queryData is null.");
            throw new ArgumentNullException(nameof(queryData));
        }

        if (smppSMSCNames == null || smppSMSCNames.Count == 0)
        {
            _logger.LogWarning("No SMSC names provided.");
            return string.Empty;
        }

        if (smppSMSCIds == null || smppSMSCIds.Count == 0)
        {
            _logger.LogWarning("No SMSC IDs provided.");
            return string.Empty;
        }

        var cleanMessageSmartUrl = queryData.GetValueOrDefault("CleanMessageSmartUrl")?.ToString();
        var smsMessage = queryData.GetValueOrDefault("Data.SmsMessage")?.ToString();
        var shortURLPrefix = queryData.GetValueOrDefault("ShortURLPrefix")?.ToString();
        var numbers = queryData.GetValueOrDefault("to")?.ToString()?.Split(',').Select(n => n.Trim()).ToList();

        if (numbers == null || numbers.Count == 0)
        {
            _logger.LogWarning("No phone numbers provided for bulk SMS insertion.");
            return string.Empty;
        }

        var shortCode = _userService.GenerateShortCode();
        var shortURL = shortURLPrefix + shortCode;
        // var smartSmsMessage = _userService.GetSmartMessage(smsMessage, shortURLPrefix, shortURL);

        var smartSmsMessage = !string.IsNullOrEmpty(cleanMessageSmartUrl) 
        ? _userService.GetSmartMessage(smsMessage, shortURLPrefix, shortURL)
        : smsMessage;

        _logger.LogInformation("Smart SMS Parameters - Message: {Message}, URL: {URL}, IsSmartSMS: {IsSmartSMS}",
        smartSmsMessage,
        cleanMessageSmartUrl,
        !string.IsNullOrEmpty(cleanMessageSmartUrl));


        var urlencodedMessage = HttpUtility.UrlEncode(smartSmsMessage);

        var msgIdMap = numbers.ToDictionary(number => number, number => GenerateMessageID(queryData.GetValueOrDefault("UserID")?.ToString()));

        // Split numbers into smaller batches (e.g., 10,000 records per batch)
        var batchSize = 10000;
        var batches = numbers.Chunk(batchSize).ToList();

        var batchSemaphore = new SemaphoreSlim(10);

        // ==============================  DB_Insert  ==================================
        // Create a list of tasks for parallel batch insertion
        // var tasks = batches
        // .Select(batch => InsertBatchAsync(batch.ToList(), queryData, smppSMSCNames, smppSMSCIds, serviceScopeFactory, smartSmsMessage, urlencodedMessage, msgIdMap, smsParam))
        // .ToList();

        // await Task.WhenAll(tasks);


        _logger.LogInformation("Smart SMS Check - CleanMessageSmartUrl: {url}, Is Smart: {isSmart}", 
            cleanMessageSmartUrl, 
            !string.IsNullOrEmpty(cleanMessageSmartUrl));
        
        var tasks = batches
        .Select(async batch => 
        {
            await batchSemaphore.WaitAsync();
            try
            {
                // Always insert into users_sms table
                await InsertBatchAsync(batch.ToList(), queryData, smppSMSCNames, smppSMSCIds, serviceScopeFactory, smartSmsMessage, urlencodedMessage, msgIdMap, smsParam);
                
                // Additionally insert into smart urls table if cleanMessageSmartUrl exists
                if (!string.IsNullOrEmpty(cleanMessageSmartUrl))
                {
                    await InsertSmartUrls(batch.ToList(), queryData, serviceScopeFactory, shortCode, shortURL, cleanMessageSmartUrl, msgIdMap, smsParam);
                }
            }
            finally
            {
                batchSemaphore.Release();
            }
        })
        .ToList();

        await Task.WhenAll(tasks);

        // =================================  API  =====================================
        
        // foreach (var batch in batches)
        // {
        //     _ = ProcessUrlsAsync(batch.ToList(), msgIdMap, smppSMSCNames, smsParam, smartSmsMessage); 
        //     // **Fire-and-forget API calls (no extra Task.WhenAll)**
        // }


        foreach (var batch in batches)
        {
            await batchSemaphore.WaitAsync();
            try
            {
                _ = ProcessUrlsAsync(batch.ToList(), msgIdMap, smppSMSCNames, smsParam, smartSmsMessage);
            }
            finally
            {
                batchSemaphore.Release();
            }
        }


        // ==============  Both Parallel and Dependant On Each Others  ==================

        // var apiTasks = tasks.Concat(apiTasks).ToList();
        // await Task.WhenAll(allTasks);

        _logger.LogInformation("Bulk SMS insertion completed successfully.");
        return smartSmsMessage;
    }

    private async Task InsertBatchAsync(List<string> batch, Dictionary<string, object> queryData, List<string> smppSMSCNames, List<int> smppSMSCIds, IServiceScopeFactory serviceScopeFactory, string smartSmsMessage, string urlencodedMessage, Dictionary<string, string> msgIdMap, string smsParam)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var sqlParams = new List<MySqlParameter>();
            var valuesList = new List<string>();
            int index = 0, smscIndex = 0;

            foreach (var number in batch)
            {
                var msgId = msgIdMap[number];
                var smscName = smppSMSCNames[smscIndex];
                var smscId = smppSMSCIds[smscIndex];

                valuesList.Add($"(@job_id{index}, @job_status{index}, @user_id{index}, @parent_user_id{index}, @gateway_family_id{index}, @route_type_id{index}, @message_format{index}, @message_method{index}, @flash_sms{index}, @schedule_sms{index}, @schedule_date{index}, @schedule_time{index}, @sender_id{index}, @sender_name{index}, @mobile_number{index}, @message_type{index}, @sms_message{index}, @urlencode_sms_message{index}, @sms_length{index}, @sms_count{index}, @smsc_id{index}, @smsc_name{index}, @mclass{index}, @coding{index}, @charset{index}, @dnd{index}, @smart{index}, @refund{index}, @block{index}, @sent_at{index}, @message_id{index}, @status{index}, @jdate{index}, @meta_data{index})");

                sqlParams.AddRange(new[]
                {
                    new MySqlParameter($"@job_id{index}", queryData.GetValueOrDefault("JobId")?.ToString() ?? "0"),
                    new MySqlParameter($"@job_status{index}", queryData.GetValueOrDefault("job_status")?.ToString() ?? "0"),
                    new MySqlParameter($"@user_id{index}", queryData.GetValueOrDefault("UserID")?.ToString() ?? "0"),
                    new MySqlParameter($"@parent_user_id{index}", queryData.GetValueOrDefault("UserParentID")?.ToString() ?? "0"),
                    new MySqlParameter($"@gateway_family_id{index}", queryData.GetValueOrDefault("GatewayFamilyId")?.ToString() ?? "0"),
                    new MySqlParameter($"@route_type_id{index}", queryData.GetValueOrDefault("RouteTypeId")?.ToString() ?? "0"),
                    new MySqlParameter($"@message_format{index}", queryData.GetValueOrDefault("Data.MessageFormat")?.ToString() ?? "0"),
                    new MySqlParameter($"@message_method{index}", queryData.GetValueOrDefault("MessageMethod")?.ToString() ?? "0"),
                    new MySqlParameter($"@flash_sms{index}", queryData.GetValueOrDefault("Access.FlashSms")?.ToString() ?? "0"),
                    new MySqlParameter($"@schedule_sms{index}", queryData.GetValueOrDefault("ScheduleSms")?.ToString() ?? "0"),
                    new MySqlParameter($"@schedule_date{index}", queryData.GetValueOrDefault("Options.Data.ScheduleDate")?.ToString() ?? "0"),
                    new MySqlParameter($"@schedule_time{index}", queryData.GetValueOrDefault("Options.Data.ScheduleTime")?.ToString() ?? "0"),
                    new MySqlParameter($"@sender_id{index}", queryData.GetValueOrDefault("SenderId")?.ToString() ?? "0"),
                    new MySqlParameter($"@sender_name{index}", queryData.GetValueOrDefault("Sender")?.ToString() ?? "0"),
                    new MySqlParameter($"@mobile_number{index}", number),
                    new MySqlParameter($"@message_type{index}", queryData.GetValueOrDefault("Data.MessageType")?.ToString() ?? "0"),
                    new MySqlParameter($"@sms_message{index}", smartSmsMessage),
                    new MySqlParameter($"@urlencode_sms_message{index}", urlencodedMessage),
                    new MySqlParameter($"@sms_length{index}", queryData.GetValueOrDefault("MsgStat.Chars")?.ToString() ?? "0"),
                    new MySqlParameter($"@sms_count{index}", queryData.GetValueOrDefault("SMSCount")?.ToString() ?? "0"),
                    new MySqlParameter($"@smsc_id{index}", smscId),
                    new MySqlParameter($"@smsc_name{index}", smscName),
                    new MySqlParameter($"@mclass{index}", queryData.GetValueOrDefault("Mclass")?.ToString() ?? "0"),
                    new MySqlParameter($"@coding{index}", queryData.GetValueOrDefault("Coding")?.ToString() ?? "0"),
                    new MySqlParameter($"@charset{index}", queryData.GetValueOrDefault("Charset")?.ToString() ?? "0"),
                    new MySqlParameter($"@dnd{index}", queryData.GetValueOrDefault("dnd")?.ToString() ?? "0"),
                    new MySqlParameter($"@smart{index}", queryData.GetValueOrDefault("is_smart")?.ToString() ?? "0"),
                    new MySqlParameter($"@refund{index}", queryData.GetValueOrDefault("refund")?.ToString() ?? "0"),
                    new MySqlParameter($"@block{index}", queryData.GetValueOrDefault("block")?.ToString() ?? "0"),
                    new MySqlParameter($"@sent_at{index}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new MySqlParameter($"@message_id{index}", msgId),
                    new MySqlParameter($"@status{index}", queryData.GetValueOrDefault("status")?.ToString() ?? "0"),
                    new MySqlParameter($"@jdate{index}", queryData.GetValueOrDefault("jdate")?.ToString() ?? "0"),
                    new MySqlParameter($"@meta_data{index}", queryData.GetValueOrDefault("MetaData")?.ToString() ?? "0")
                });

                smscIndex = (smscIndex + 1) % smppSMSCNames.Count;
                index++;
            }

            var smsSql = $"INSERT INTO users_sms (job_id, job_status, user_id, parent_user_id, gateway_family_id, route_type_id, message_format, message_method, flash_sms, schedule_sms, schedule_date, schedule_time, sender_id, sender_name, mobile_number, message_type, sms_message, urlencode_sms_message, sms_length, sms_count, smsc_id, smsc_name, mclass, coding, charset, dnd, smart, refund, block, sent_at, message_id, status, jdate, meta_data) VALUES {string.Join(", ", valuesList)};";
            using var smsCmd = connection.CreateCommand();
            smsCmd.CommandText = smsSql;
            smsCmd.Transaction = (MySqlTransaction)transaction;
            smsCmd.Parameters.AddRange(sqlParams.ToArray());
            await smsCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();




        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while inserting SMS records for batch.");
            throw;
        }
    }

    private async Task InsertSmartUrls(List<string> batch, Dictionary<string, object> queryData, IServiceScopeFactory serviceScopeFactory, string shortCode, string shortURL, string cleanMessageSmartUrl, Dictionary<string, string> msgIdMap, string smsParam)
    {
        _logger.LogInformation("Inserting into smart urls for batch of size: {batch}", batch);
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // First insert into users_sms_smart_urls
            var sqlParams = new List<MySqlParameter>();
            var valuesList = new List<string>();
            int index = 0;

            foreach (var number in batch)
            {
                _logger.LogInformation("Inserting into users_sms_smart_urls for number: {number}");



                valuesList.Add($"(@user_id{index}, @job_id{index}, @mobile_number{index}, @filename{index}, @long_url{index}, @short_code{index}, @short_url{index}, @add_to_group{index}, @group_id{index}, @send_notification{index}, @sms_message{index}, @hits{index}, @pdate{index}, @jdate{index}, @ptime{index}, @ipaddress{index})");

                sqlParams.AddRange(new[]
                {
                    new MySqlParameter($"@user_id{index}", queryData.GetValueOrDefault("UserID")?.ToString() ?? "0"),
                    new MySqlParameter($"@job_id{index}", queryData.GetValueOrDefault("JobId")?.ToString() ?? "0"),

                    new MySqlParameter($"@mobile_number{index}", number),

                    new MySqlParameter($"@filename{index}", queryData.GetValueOrDefault("Data.SmartFilename")?.ToString() ?? 
                        queryData.GetValueOrDefault("RouteTypeData.SmartFilename")?.ToString() ?? 
                        "default_filename"),
                    new MySqlParameter($"@long_url{index}", cleanMessageSmartUrl),
                    new MySqlParameter($"@short_code{index}", shortCode),
                    new MySqlParameter($"@short_url{index}", shortURL),
                    // new MySqlParameter($"@add_to_group{index}", queryData.GetValueOrDefault("AddToGroup")?.ToString() ?? "0"),
                    // new MySqlParameter($"@group_id{index}", queryData.GetValueOrDefault("GroupId")?.ToString() ?? "0"),
                    // new MySqlParameter($"@send_notification{index}", queryData.GetValueOrDefault("SendNotification")?.ToString() ?? "0"),
                    // new MySqlParameter($"@sms_message{index}", ""),

                    new MySqlParameter($"@add_to_group{index}", queryData.GetValueOrDefault("Data.AddToGroup")?.ToString() ?? 
                        queryData.GetValueOrDefault("RouteTypeData.AddToGroup")?.ToString() ?? "0"),
                    new MySqlParameter($"@group_id{index}", queryData.GetValueOrDefault("Data.GroupId")?.ToString() ?? 
                        queryData.GetValueOrDefault("RouteTypeData.GroupId")?.ToString() ?? "0"),
                    new MySqlParameter($"@send_notification{index}", queryData.GetValueOrDefault("Data.SendNotification")?.ToString() ?? 
                        queryData.GetValueOrDefault("RouteTypeData.SendNotification")?.ToString() ?? "0"),
                    new MySqlParameter($"@sms_message{index}", queryData.GetValueOrDefault("Data.SmsMessage")?.ToString() ?? 
                        queryData.GetValueOrDefault("RouteTypeData.SmsMessage")?.ToString() ?? ""),
                    // new MySqlParameter($"@hits{index}", 0),

                    new MySqlParameter($"@hits{index}", queryData.GetValueOrDefault("hits")?.ToString() ?? "0"),
                    new MySqlParameter($"@pdate{index}", DateTime.Now.ToString("yyyy-MM-dd")),
                    new MySqlParameter($"@jdate{index}", queryData.GetValueOrDefault("jdate") ?? JulianDate(DateTimeOffset.Now.ToUnixTimeSeconds())),
                    // new MySqlParameter($"@ptime{index}", DateTime.Now.ToString("HH:mm:ss")),

                    new MySqlParameter($"@ptime{index}", queryData.GetValueOrDefault("RouteTypeData.PTime")?.ToString() ?? "0"),

                    // new MySqlParameter($"@ipaddress{index}", queryData.GetValueOrDefault("ipaddress")?.ToString() ?? ""),

                    new MySqlParameter($"@ipaddress{index}", queryData.GetValueOrDefault("RouteTypeData.IpAddress")?.ToString() ?? "0")
                });

                index++;
            }

            var columns = "user_id, job_id, mobile_number, filename, long_url, short_code, short_url, add_to_group, group_id, send_notification, sms_message, hits, pdate, jdate, ptime, ipaddress";
            var sql = $"INSERT INTO users_sms_smart_urls ({columns}) VALUES {string.Join(", ", valuesList)};";

            // Add query logging
            var loggedQuery = sql;
            foreach (MySqlParameter param in sqlParams)
            {
                loggedQuery = loggedQuery.Replace(param.ParameterName, $"'{param.Value?.ToString()}'");
            }
            _logger.LogInformation("Generated Smart URLs SQL Query: {Query}", loggedQuery);


            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = (MySqlTransaction)transaction;
            cmd.Parameters.AddRange(sqlParams.ToArray());
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            _logger.LogInformation($"Successfully inserted {batch.Count} records into both tables");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Failed to insert records. Rolled back transaction.");
            throw;
        }
    }

    private static readonly HttpClient _httpClient = new(); // Reuse HttpClient

    private async Task ProcessUrlsAsync(List<string> batch, Dictionary<string, string> msgIdMap, List<string> smppSMSCNames, string smsParam, string smsMessage)
    {
        await Parallel.ForEachAsync(batch, async (number, _) =>
        {
            // var smscName = smppSMSCNames[batch.IndexOf(number) % smppSMSCNames.Count];
            var smscName = smppSMSCNames[Random.Shared.Next(smppSMSCNames.Count)]; // Better distribution
            if (!msgIdMap.TryGetValue(number, out var msgId)) return;

            var url = GenerateSmsUrl(smsParam, number, smscName, msgId, smsMessage);
            await CallSmsUrlAsync(url);
        });
    }

        public async Task ProcessSingleSms(UsersSMS smsJob)
    {
        try
        {
            var smsParam = _configuration["SmsConfig:SmsParam"];
            var smppSMSCNames = new List<string> { smsJob.SMPPSMSCIDName };
            var smppSMSCIds = new List<int> { smsJob.SMPPSMSCID };

            var url = GenerateSmsUrl(smsParam, smsJob.Number, smsJob.SMPPSMSCIDName, smsJob.MsgId, smsJob.sms_message);
            await CallSmsUrlAsync(url);

            _logger.LogInformation("Processed single SMS for mobile: {mobile}", smsJob.Number);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing single SMS for mobile: {mobile}", smsJob.Number);
            throw;
        }
    }


    private string GenerateSmsUrl(string smsParam, string number, string smscName, string msgId, string smsMessage)
    {
        var url = $"{SmsConfig.SMS_CGI}{smsParam}&to={Uri.EscapeDataString(number)}&smsc={Uri.EscapeDataString(smscName)}&text={HttpUtility.UrlEncode(smsMessage)}";
        return url.Replace(Uri.EscapeDataString("${msg_id}"), msgId);
    }

    private async Task CallSmsUrlAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to call SMS URL: {url}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling SMS URL: {ex.Message}");
        }
    }

    public static string MyEsc(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            char[] search = ['\\', '\0', '\n', '\r', '\'', '\"', '\u001A'];
            string[] replace = ["\\\\", "\\0", "\\n", "\\r", "\\'", "\\\"", "\\Z"];

            for (int i = 0; i < search.Length; i++)
            {
                value = value.Replace(search[i].ToString(), replace[i]);
            }

            return value;
        }

        return value; // return the original value if null or empty
    }
    public static string GetClientIp(HttpContext context)
    { 
        var ip = context.Request.Headers["HTTP_CLIENT_IP"].FirstOrDefault() ??
                 context.Request.Headers["HTTP_X_FORWARDED_FOR"].FirstOrDefault() ??
                 context.Request.Headers["HTTP_X_FORWARDED"].FirstOrDefault() ??
                 context.Request.Headers["HTTP_FORWARDED_FOR"].FirstOrDefault() ??
                 context.Request.Headers["HTTP_FORWARDED"].FirstOrDefault() ??
                 context.Connection.RemoteIpAddress?.ToString() ?? "UNKNOWN";

        return ip;
    }
    public async Task ProcessPropertiesOld(object obj, Dictionary<string, object> alldata, string prefix)
    {
        if (obj == null) return;

        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var fullKey = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
 
            if (value is Array array)
            {
                alldata[fullKey] = array?.Cast<object>().ToList() ?? new List<object>();
            } 
            else if (value != null && !value.GetType().IsPrimitive && value.GetType() != typeof(string))
            {
                await ProcessProperties(value, alldata, fullKey);
            }
            else
            { 
                alldata[fullKey] = value ?? string.Empty;
            }
        }
    }
    public async Task ProcessProperties(object obj, Dictionary<string, object> alldata, string prefix)
    {
        if (obj == null) return;

        // Get all properties of the object
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length > 0) 
            {
                // _logger.LogWarning("Skipping property with indexer: {property.Name}", property.Name);
                continue;
            }

            // _logger.LogInformation("Processing property: {property.Name}, Type: {property.PropertyType}", property.Name, property.PropertyType);

            var value = property.GetValue(obj);
            var fullKey = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (value is Array array)
            {
                alldata[fullKey] = array?.Cast<object>().ToList() ?? new List<object>();
            }
            else if (value != null && !value.GetType().IsPrimitive && value.GetType() != typeof(string))
            {
                if (value is IEnumerable<object> || value.GetType().IsClass)
                {
                    await ProcessProperties(value, alldata, fullKey);
                }
                else
                {
                    alldata[fullKey] = value ?? string.Empty;
                }
            }
            else
            {
                alldata[fullKey] = value ?? string.Empty;
            }
        }
    }



    public static class Logger
    {
        public static void Trace(string message) => Console.WriteLine($"TRACE: {message}");
        public static void Fatal(string message) => Console.WriteLine($"FATAL: {message}");
    }
}
