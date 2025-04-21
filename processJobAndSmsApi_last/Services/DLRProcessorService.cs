// using System.Text;
// using System.Threading.Channels;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using processJobAndSmsApi.Models;
// using processJobAndSmsApi.Data;
// using System.Threading;

// public class DLRProcessorService
// {
//     private readonly Channel<DlrRequest> _queue = Channel.CreateUnbounded<DlrRequest>();
//     private readonly ILogger<DLRProcessorService> _logger;
//     private readonly IServiceScopeFactory _scopeFactory;
//     private readonly CancellationTokenSource _cts = new();
//     private readonly SemaphoreSlim _semaphore = new(10); // Limit parallel DB updates

//     public DLRProcessorService(ILogger<DLRProcessorService> logger, IServiceScopeFactory scopeFactory)
//     {
//         _logger = logger;
//         _scopeFactory = scopeFactory;
//         _ = ProcessQueueAsync(_cts.Token); // Start processing
//     }

//     public async ValueTask AddToQueueAsync(DlrRequest requestData)
//     {
//         _logger.LogInformation("Adding request to queue: {@RequestData}", requestData);
        
//         await _queue.Writer.WriteAsync(requestData, _cts.Token);

//         _logger.LogInformation("Queue size after adding: {QueueCount}", _queue.Reader.Count);
//     }

//     private async Task ProcessQueueAsync(CancellationToken cancellationToken)
//     {
//         _logger.LogInformation("Processing queue started.");

//         // Process each request individually as it comes from the queue
//         await foreach (var request in _queue.Reader.ReadAllAsync(cancellationToken))
//         {
//             _logger.LogInformation("Processing single record.");
//             _ = ProcessSingleRequestAsync(request); // Fire processing immediately
//         }
//     }

//     private async Task ProcessSingleRequestAsync(DlrRequest request)
//     {
//         _logger.LogInformation("Processing single record started");

//         await _semaphore.WaitAsync(); // Limit concurrent DB updates

//         try
//         {
//             await UpdateDLRAsync(request);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error processing single record.");
//         }
//         finally
//         {
//             _semaphore.Release(); // Allow the next record
//         }
//         _logger.LogInformation("Processing single record completed");
//     }

//     private async Task UpdateDLRAsync(DlrRequest request)
//     {
//         _logger.LogInformation("UpdateDLRAsync Started ========================================>");
//         using var scope = _scopeFactory.CreateScope();
//         var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//         var queryBuilder = new StringBuilder();
//         queryBuilder.AppendLine("START TRANSACTION;");

//         if (request.QueryString.TryGetValue("msg_id", out var msgId) &&
//             request.QueryString.TryGetValue("status", out var status) &&
//             request.QueryString.TryGetValue("success", out var success) &&
//             request.QueryString.TryGetValue("ack", out var msgData))
//         {
//             _logger.LogInformation("Extracted Query Parameters - msg_id: {MsgId}, status: {Status}, success: {Success}, msgdata: {MsgData}",
//                 msgId, status, success, msgData);

//             if (!string.IsNullOrEmpty(msgId))
//             {
//                 // queryBuilder.AppendLine($@"
//                 //     UPDATE users_sms 
//                 //     SET status = '{status}', success = '{success}', msgdata = '{msgData}' 
//                 //     WHERE message_id = '{msgId}' AND status = 'Submitted';");

//                 // _logger.LogInformation("Executing SQL Query: {queryBuilder}", queryBuilder);
//             }
//         }

//         queryBuilder.AppendLine("COMMIT;");

//         try
//         {
//             await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString()); // Execute query
//             _logger.LogInformation("Executed DLR update query for single record.");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error updating DLR for single record.");
//             throw;
//         }
//     }
// }