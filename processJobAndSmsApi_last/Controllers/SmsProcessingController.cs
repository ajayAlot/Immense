using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;
using System.IO;
using processJobAndSmsApi.Data;
using processJobAndSmsApi.Models;
using processJobAndSmsApi.Constants;
using processJobAndSmsApi.Extensions;
using System.Collections.Concurrent;

namespace processJobAndSmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsProcessingController : ControllerBase
    {
        private readonly ILogger<SmsProcessingController> _logger;
        private readonly UserStatusService _userStatusService;
        private readonly SmsService _smsService;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly IServiceProvider _serviceProvider;

        public SmsProcessingController(
            ILogger<SmsProcessingController> logger,
            UserStatusService userStatusService,
            SmsService smsService,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _userStatusService = userStatusService;
            _smsService = smsService;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("add-user-sms")]
        public async Task<IActionResult> AddUserSMS([FromBody] ProcessJobRequest request)
        {

            if (request == null)
            {
                _logger.LogError("Request object is null");
                return BadRequest("Request cannot be null");
            }

            // Add detailed validation logging
            foreach (var modelState in ModelState)
            {
                if (modelState.Value.Errors.Count > 0)
                {
                    _logger.LogError("Validation errors for {Property}: {Errors}",
                        modelState.Key,
                        string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }

            // Rest of your existing code...
            _logger.LogInformation("Incoming request: {@Request}", request);
            _logger.LogInformation("Incoming request JSON: {RequestJson}", JsonConvert.SerializeObject(request));
            
            _logger.LogInformation("Incoming request: {@Request}", request);
            _logger.LogInformation("Incoming request JSON: {RequestJson}", JsonConvert.SerializeObject(request));

            var updateStatus = await _userStatusService.UpdateJobStatus(request.JobId, JobStatus.PROCESSING);
            if (!updateStatus)
            {
                _logger.LogError("Failed to update job status to PROCESSING for JobId: {JobId}", request.JobId);
                return StatusCode(500, "Failed to update job status to PROCESSING.");
            }

            try
            {
                string filePath = Path.Combine("/var/www/html/test-panel/", request.FilePath + request.ContactFilename);
                _logger.LogInformation("Filepath: {FilePath}", filePath);

                if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File path is invalid or file does not exist: {filePath}");
                }

                var mobileNumbersWithData = ReadExcelFile(filePath);
                if (mobileNumbersWithData == null || !mobileNumbersWithData.Any())
                {
                    _logger.LogError("No data found in the Excel file.");
                    return BadRequest("No data found in the file.");
                }

                string mobileColumnHeader = mobileNumbersWithData.First().Keys.First();
                if (!mobileNumbersWithData.First().ContainsKey(mobileColumnHeader))
                {
                    _logger.LogError($"Column '{mobileColumnHeader}' not found in the Excel file.");
                    return BadRequest($"Column '{mobileColumnHeader}' not found.");
                }

                // Remove duplicates by keeping first occurrence
                var uniqueRecords = mobileNumbersWithData
                    .GroupBy(x => x[mobileColumnHeader])
                    .Select(g => g.First())
                    .ToList();

                if (mobileNumbersWithData.Count != uniqueRecords.Count)
                {
                    _logger.LogWarning("Removed {DuplicateCount} duplicate records. Processing {UniqueCount} unique records.",
                        mobileNumbersWithData.Count - uniqueRecords.Count, uniqueRecords.Count);
                }

                string[] gateways = request.SMPPSMSCIDName.Split(',');
                string[] gatewayIds = request.SMPPSMSCID.Split(',');
                string smsMessageTemplate = request.SmsMessage;

                // Create a list of records with unique message IDs
                var smsRecords = uniqueRecords
                    .Select(data => new SmsRecord(
                        MobileNumber: data[mobileColumnHeader],
                        Data: data,
                        MessageId: _smsService.GenerateMessageID(request.LoggedUserId)
                    ))
                    .ToList();

                // Split into batches for processing
                var batchSize = 10000;
                var batches = smsRecords.Chunk(batchSize).ToList();

                // Process all batches sequentially to avoid DbContext threading issues
                foreach (var batch in batches)
                {
                    await ProcessBatchAsync(batch.ToList(), request, mobileColumnHeader, 
                        smsMessageTemplate, gateways[0], gatewayIds[0]);
                }

                await _userStatusService.UpdateJobStatus(request.JobId, JobStatus.COMPLETED);
                _logger.LogInformation("SMS processing completed successfully for all records.");
                return Ok(new {
                    Message = "SMS processing completed successfully",
                    TotalRecords = smsRecords.Count,
                    DuplicatesRemoved = mobileNumbersWithData.Count - uniqueRecords.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred: {Message}", ex.Message);
                await _userStatusService.UpdateJobStatusRemarkAsync(
                    JobStatus.FAILED,
                    "An error occurred during processing.",
                    request.JobId);
                return StatusCode(500, new {
                    Error = "An error occurred during processing.",
                    Details = ex.Message
                });
            }
        }

        private async Task ProcessBatchAsync(List<SmsRecord> batch, ProcessJobRequest request,
            string mobileColumnHeader, string smsMessageTemplate, 
            string currentGateway, string currentGatewayID)
        {
            try
            {
                // Filter valid numbers
                var validRecords = batch.Where(r => IsValidMobileNumber(r.MobileNumber)).ToList();
                var invalidRecords = batch.Where(r => !IsValidMobileNumber(r.MobileNumber)).ToList();

                if (invalidRecords.Any())
                {
                    _logger.LogWarning("Batch contains {InvalidCount} invalid numbers", invalidRecords.Count);
                }

                // Prepare base SMS parameters
                var smsParamBase = await PrepareSmsParameters(request);

                // Process records sequentially within the batch
                foreach (var record in validRecords)
                {
                    try
                    {
                        // Create a new DbContext for each record
                        using var scope = _serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Generate dynamic message
                        string dynamicMessage = ReplacePlaceholders(smsMessageTemplate, record.Data);

                        // Insert into database
                        await InsertSmsRecordAsync(dbContext, record, request, dynamicMessage, 
                            dynamicMessage.Length, currentGateway, currentGatewayID);

                        // Send SMS
                        var url = GenerateSmsUrl(smsParamBase, record.MobileNumber, 
                            currentGateway, record.MessageId, dynamicMessage);
                        await CallSmsUrlAsync(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error processing record {MobileNumber}: {Error}", 
                            record.MobileNumber, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing batch: {Error}", ex.Message);
                throw;
            }
        }

        private async Task InsertSmsRecordAsync(ApplicationDbContext dbContext, SmsRecord record, 
            ProcessJobRequest request, string smsMessage, int smsMessageLength, 
            string currentGateway, string currentGatewayID)
        {
            var insertQuery = @"INSERT INTO users_sms 
                (job_id, job_status, user_id, parent_user_id, gateway_family_id, 
                route_type_id, message_format, message_method, flash_sms, schedule_sms, 
                schedule_date, schedule_time, sender_id, sender_name, mobile_number, 
                message_type, sms_message, urlencode_sms_message, sms_length, sms_count, 
                smsc_id, smsc_name, mclass, coding, charset, dnd, smart, refund, block, 
                sent_at, message_id, status, jdate, meta_data) 
                VALUES 
                (@JobId, @JobStatus, @LoggedUserId, @LoggedUserParentID, @GatewayFamilyId, 
                @RouteTypeId, @MessageFormat, @MessageMethod, @FlashSms, @ScheduleSms, 
                @ScheduleDate, @ScheduleTime, @SenderId, @SenderName, @ToNumber, 
                @MessageType, @SmsMessage, @UrlencodeSmsMessage, @SmsMessageLength, @SmsCount, 
                @SmppSmscId, @SmppSmscIdName, @Mclass, @Coding, @Charset, @DndCount, 
                @SmartSmsNo, @RefundCount, @BlockCount, @Pdate, @messageID, @MessageStatus, 
                @Jdate, @MetaData)";

            var parameters = GetSqlParameters(request, record.MobileNumber, smsMessage, 
                smsMessageLength, currentGateway, currentGatewayID, record.MessageId);

            var result = await dbContext.Database.ExecuteSqlRawAsync(insertQuery, parameters);
            if (result <= 0)
            {
                _logger.LogError("Failed to insert record for {MobileNumber}", record.MobileNumber);
            }
        }


        private MySqlParameter[] GetSqlParameters(ProcessJobRequest request, string mobileNumber,
            string smsMessage, int smsMessageLength, string currentGateway, 
            string currentGatewayID, string messageID)
        {
            return new[]
            {
                new MySqlParameter("@JobId", request.JobId),
                new MySqlParameter("@JobStatus", request.JobStatus),
                new MySqlParameter("@LoggedUserId", request.LoggedUserId),
                new MySqlParameter("@LoggedUserParentID", request.LoggedUserParentID),
                new MySqlParameter("@GatewayFamilyId", request.GatewayFamilyId),
                new MySqlParameter("@RouteTypeId", request.RouteTypeId),
                new MySqlParameter("@MessageFormat", request.MessageFormat),
                new MySqlParameter("@MessageMethod", request.MessageMethod),
                new MySqlParameter("@FlashSms", request.FlashSms),
                new MySqlParameter("@ScheduleSms", request.ScheduleSms),
                new MySqlParameter("@ScheduleDate", request.ScheduleDate),
                new MySqlParameter("@ScheduleTime", request.ScheduleTime),
                new MySqlParameter("@SenderId", request.SenderId),
                new MySqlParameter("@SenderName", request.SenderName),
                new MySqlParameter("@ToNumber", mobileNumber),
                new MySqlParameter("@MessageType", request.MessageType),
                new MySqlParameter("@SmsMessage", smsMessage),
                new MySqlParameter("@UrlencodeSmsMessage", request.UrlencodeSmsMessage),
                new MySqlParameter("@SmsMessageLength", smsMessageLength),
                new MySqlParameter("@SmsCount", request.SMSCount),
                new MySqlParameter("@SmppSmscId", currentGatewayID),
                new MySqlParameter("@SmppSmscIdName", currentGateway),
                new MySqlParameter("@Mclass", request.Mclass),
                new MySqlParameter("@Coding", request.Coding),
                new MySqlParameter("@Charset", request.Charset),
                new MySqlParameter("@DndCount", request.DNDCount),
                new MySqlParameter("@SmartSmsNo", "NA"),
                new MySqlParameter("@RefundCount", request.RefundCount),
                new MySqlParameter("@BlockCount", request.BlockCount),
                new MySqlParameter("@Pdate", request.PDate),
                new MySqlParameter("@messageID", messageID),
                new MySqlParameter("@MessageStatus", request.MessageStatus),
                new MySqlParameter("@Jdate", request.JDate),
                new MySqlParameter("@MetaData", request.MetaData)
            };
        }

        private (bool HasDuplicates, int DuplicateCount, string FirstDuplicate) CheckForDuplicates(
            List<Dictionary<string, string>> data, string mobileColumnHeader)
        {
            var duplicates = data
                .GroupBy(x => x[mobileColumnHeader])
                .Where(g => g.Count() > 1)
                .ToList();

            return (
                HasDuplicates: duplicates.Any(),
                DuplicateCount: duplicates.Sum(g => g.Count() - 1),
                FirstDuplicate: duplicates.FirstOrDefault()?.Key
            );
        }

        private async Task<string> PrepareSmsParameters(ProcessJobRequest request)
        {
            var dltInfo = string.Format(SmsConfig.TLV_INFO, 
                request.MetaData.Split("EntityID=")[1].Split("&")[0],
                request.MetaData.Split("ContentID=")[1].Split("&")[0],
                request.MetaData.Split("HASH=")[1],
                request.JobId,
                request.LoggedUserId);

            var smsData = new Dictionary<string, string>
            {
                { "username", SmsConfig.SMS_USER },
                { "password", SmsConfig.SMS_PSWD },
                { "from", request.SenderName?.ToString() ?? string.Empty },
                { "account", request.LoggedUserId.ToString() },
                { "dlr-mask", SmsConfig.DLR_MASK.ToString() },
                { "dlr-url", SmsConfig.DLR_URL },  // Changed from dlrUrl to SmsConfig.DLR_URL
                { "coding", request.Coding?.ToString() ?? string.Empty },
                { "meta-data", SmsConfig.TLV_SMPP + dltInfo }
            };
            return "?" + await new FormUrlEncodedContent(smsData).ReadAsStringAsync();
        }
        
        private bool IsValidMobileNumber(string number)
        {
            return !string.IsNullOrWhiteSpace(number) && 
                   number.Length == 10 && 
                   number.All(char.IsDigit);
        }

        private string GenerateSmsUrl(string smsParamBase, string number, 
            string smscName, string msgId, string smsMessage)
        {
            var url = $"{SmsConfig.SMS_CGI}{smsParamBase}&to={Uri.EscapeDataString(number)}&smsc={Uri.EscapeDataString(smscName)}";
            url = url.Replace(Uri.EscapeDataString("${msg_id}"), msgId);
            url += "&text=" + HttpUtility.UrlEncode(smsMessage);
            return url;
        }

        private async Task CallSmsUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to send SMS. URL: {Url}, Response: {ResponseText}", url, responseText);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error calling SMS URL: {Url}, Exception: {Message}", url, ex.Message);
            }
        }

        private List<Dictionary<string, string>> ReadExcelFile(string filePath)
        {
            var data = new List<Dictionary<string, string>>();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = Path.GetExtension(filePath).ToLower() == ".xls"
                    ? new HSSFWorkbook(stream)
                    : new XSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);
                var headerRow = sheet.GetRow(0);

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var rowData = new Dictionary<string, string>();
                    for (int j = 0; j < headerRow.LastCellNum; j++)
                    {
                        var header = headerRow.GetCell(j)?.ToString() ?? $"Column{j + 1}";
                        var cellValue = row.GetCell(j)?.ToString() ?? string.Empty;
                        rowData[header] = cellValue;
                    }
                    data.Add(rowData);
                }
            }
            return data;
        }

        private string ReplacePlaceholders(string template, Dictionary<string, string> rowData)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            if (rowData == null) return template;

            return Regex.Replace(template, @"\{([^}]+)\}", match =>
            {
                string columnName = match.Groups[1].Value.Trim();
                return rowData.TryGetValue(columnName, out string value) ? value : string.Empty;
            });
        }
    }

    public record SmsRecord(
        string MobileNumber,
        Dictionary<string, string> Data,
        string MessageId
    );
}