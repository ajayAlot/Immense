using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using processJobAndSmsApi.Models.Configuration;

public class FileWatcherService : BackgroundService
{
    private readonly DlrLogParserService _dlrService;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly DlrSettings _dlrSettings;
    private const int ReadBufferSize = 131072; // 128KB buffer

    public FileWatcherService(
        DlrLogParserService dlrService,
        ILogger<FileWatcherService> logger,
        IOptions<DlrSettings> dlrSettings)
    {
        _dlrService = dlrService;
        _logger = logger;
        _dlrSettings = dlrSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting DLR processor from standard input");

        try
        {
            using var reader = new StreamReader(
                Console.OpenStandardInput(), 
                Encoding.UTF8, 
                detectEncodingFromByteOrderMarks: false, 
                bufferSize: ReadBufferSize, 
                leaveOpen: true);

            var batch = new List<string>(5000); // Pre-allocate batch buffer
            var readTask = reader.ReadLineAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                var line = await readTask;
                readTask = reader.ReadLineAsync(); // Start next read immediately

                if (line == null)
                {
                    if (batch.Count > 0)
                    {
                        await _dlrService.AddBatchAsync(batch, stoppingToken);
                        batch.Clear();
                    }
                    await Task.Delay(100, stoppingToken);
                    continue;
                }

                batch.Add(line);
                if (batch.Count >= 1000) // Send in larger chunks
                {
                    await _dlrService.AddBatchAsync(batch, stoppingToken);
                    batch = new List<string>(1000); // Reset with pre-allocated size
                }
            }

            // Process any remaining lines
            if (batch.Count > 0)
            {
                await _dlrService.AddBatchAsync(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DLR processor stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing input");
            throw;
        }
    }
}