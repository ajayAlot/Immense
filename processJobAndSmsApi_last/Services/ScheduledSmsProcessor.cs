// using processJobAndSmsApi.Data;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using processJobAndSmsApi.Constants;


// public class ScheduledSmsProcessor : BackgroundService
// {
//     private readonly IServiceScopeFactory _scopeFactory;
//     private readonly ILogger<ScheduledSmsProcessor> _logger;

//     public ScheduledSmsProcessor(IServiceScopeFactory scopeFactory, ILogger<ScheduledSmsProcessor> logger)
//     {
//         _scopeFactory = scopeFactory;
//         _logger = logger;
//     }

// protected override async Task ExecuteAsync(CancellationToken stoppingToken)
// {
//     while (!stoppingToken.IsCancellationRequested)
//     {
//         try
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var smsService = scope.ServiceProvider.GetRequiredService<SmsService>();
//             var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//             var now = DateTime.Now;
//             var currentDate = now.ToString("yyyy-MM-dd");
//             var currentTime = now.ToString("HH:mm");

//             const int batchSize = 50000;
//             var totalProcessed = 0;

//             while (true)
//             {
//                 var dueSms = await dbContext.UsersSMS
//                     .Where(s => s.schedule_sms == "1" 
//                             && s.schedule_date == currentDate 
//                             && s.schedule_time == currentTime
//                             && s.Status == "PENDING")
//                     .OrderBy(s => s.Id)  // Add ordering by primary key
//                     .Skip(totalProcessed)
//                     .Take(batchSize)
//                     .ToListAsync(stoppingToken);

//                 if (!dueSms.Any()) break;

//                 foreach (var sms in dueSms)
//                 {
//                     try
//                     {
//                         // Construct smsParam dynamically from available fields
//                         var smsParam = $"user_id={sms.user_id}&job_id={sms.job_id}&msg_id={sms.MsgId}";
                        
//                         var url = smsService.GenerateSmsUrl(
//                             smsParam,
//                             sms.Number,
//                             sms.SMPPSMSCIDName,
//                             sms.MsgId,
//                             sms.sms_message
//                         );
//                         await smsService.CallSmsUrlAsync(url);

//                         sms.Status = "SENT";
//                         sms.SentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                     }
//                     catch (Exception ex)
//                     {
//                         _logger.LogError(ex, "Error processing scheduled SMS {MessageId}", sms.MsgId);
//                         sms.Status = "FAILED";
//                     }
//                 }

//                 await dbContext.SaveChangesAsync(stoppingToken);
//                 totalProcessed += dueSms.Count;
//                 _logger.LogInformation("Processed {count} scheduled SMS", totalProcessed);
//             }
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error in ScheduledSmsProcessor");
//         }
//         await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//     }
// }
// }