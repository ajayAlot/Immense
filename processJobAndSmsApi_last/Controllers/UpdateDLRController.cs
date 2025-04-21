// using Microsoft.AspNetCore.Mvc;
// using processJobAndSmsApi.Data;
// using processJobAndSmsApi.Models;
// using System.Web;

// namespace processJobAndSmsApi.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class UpdateDLRController : Controller
//     {
//         private readonly ILogger<UpdateDLRController> _logger;
//         private readonly DLRProcessorService _dlrProcessor;

//         public UpdateDLRController(ILogger<UpdateDLRController> logger, DLRProcessorService dlrProcessor)
//         {
//             _logger = logger;
//             _dlrProcessor = dlrProcessor;
//         }

//         [HttpGet]
//         [Route("process-dlr")]
//         public async Task<IActionResult> KannelWebhook()
//         {
//             try
//             {
//                 var queryStringCollection = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
//                 var queryString = queryStringCollection.AllKeys.ToDictionary(k => k, k => queryStringCollection[k] ?? "");

//                 var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
//                 var uniqueId = Guid.NewGuid().ToString();

//                 var DlrData = new DlrRequest
//                 {
//                     DateTimeReceived = DateTime.UtcNow,
//                     HexUniqueId = uniqueId,
//                     RequestBody = requestBody,
//                     QueryString = queryString
//                 };

//                 await _dlrProcessor.AddToQueueAsync(DlrData); // Use async method

//                 return Ok(new { Message = "DLR received and queued", UniqueId = uniqueId });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing KannelWebhook request.");
//                 return StatusCode(500, "Internal server error.");
//             }
//         }
//     }
// }