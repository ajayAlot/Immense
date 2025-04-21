// using System;
// using System.Collections.Concurrent;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;

// public class SmsQueueProcessor : BackgroundService
// {
//     private readonly ILogger<SmsQueueProcessor> _logger;
//     private readonly ConcurrentQueue<string> _urlQueue;
//     private readonly SemaphoreSlim _signal;
//     private readonly HttpClient _httpClient;

//     public SmsQueueProcessor(ILogger<SmsQueueProcessor> logger)
//     {
//         _logger = logger;
//         _urlQueue = new ConcurrentQueue<string>();
//         _signal = new SemaphoreSlim(0);

//         // Optimized HttpClient with increased connection limit
//         var handler = new HttpClientHandler
//         {
//             MaxConnectionsPerServer = 100 // Increase connection limit
//         };
//         _httpClient = new HttpClient(handler);
//     }

//     public void EnqueueUrl(string url)
//     {
//         _urlQueue.Enqueue(url);
//         _signal.Release(); // Notify that a new URL is available
//         _logger.LogInformation("URL enqueued. Queue count: {Count}", _urlQueue.Count); // Reduced logging
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         _logger.LogInformation("SmsQueueProcessor started...");

//         while (!stoppingToken.IsCancellationRequested)
//         {
//             await _signal.WaitAsync(stoppingToken); // Wait until an item is available

//             if (_urlQueue.TryDequeue(out string url))
//             {
//                 try
//                 {
//                     var response = await _httpClient.GetAsync(url, stoppingToken);

//                     if (response.IsSuccessStatusCode)
//                     {
//                         _logger.LogInformation("SMS sent successfully.");
//                     }
//                     else
//                     {
//                         _logger.LogWarning("Failed to send SMS. Status: {StatusCode}", response.StatusCode);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error processing SMS request.");
//                 }
//             }
//         }

//         _logger.LogInformation("SmsQueueProcessor is stopping...");
//     }
// }