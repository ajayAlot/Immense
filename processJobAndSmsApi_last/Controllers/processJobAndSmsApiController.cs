using Microsoft.AspNetCore.Mvc;
using processJobAndSmsApi.Data;
using processJobAndSmsApi.Models;
using Serilog;
using processJobAndSmsApi.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MySqlConnector;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using CsvHelper;
using System.Globalization;
using System.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography;



namespace processJobAndSmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessJobAndSmsApiController(ApplicationDbContext context, ILogger<ProcessJobAndSmsApiController> logger, NumberService numberService, BalanceService balanceService, UserStatusService userStatusService, UserNotificationService userNotificationService, SmsService smsService, IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory
) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ProcessJobAndSmsApiController> _logger = logger;
        private readonly NumberService _numberService = numberService;
        private readonly BalanceService _balanceService = balanceService;
        private readonly UserStatusService _userStatusService = userStatusService;
        private readonly SmsService _SmsService = smsService;
        private readonly UserNotificationService _userNotificationService = userNotificationService;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        // ========================== For useing same httpClientFactory For DLR ==========================
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory; 


        [HttpPost("process")]
        public async Task<IActionResult> ProcessJobAndSmsApi([FromBody] ProcessJobRequest request)
        { 
            // Log.Information("THE REQUEST DATA IS : {@request}", request);
            
            var alldata = new Dictionary<string, object>();
 
            await _SmsService.ProcessProperties(request, alldata, "");
 
            // Log.Information("Processed Data: {@alldata}");
            var keyValuePairs = alldata.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)).ToList();

            // _logger.LogInformation("keyValuePairs----{keyValuePairs}",keyValuePairs); 
            // _logger.LogInformation("Processing job with JobId: {JobId}", request.JobId);
            // _logger.LogInformation("Processing job with SmsMessage: {SmsMessage}", request.Data?.SmsMessage);
            // _logger.LogInformation("Processing SmsLimit: {SmsLimit}", request.SmsLimit);
            // _logger.LogInformation("Processing RouteTypeId: {RouteTypeId}", request.RouteTypeId);
            // _logger.LogInformation("Processing TotalBalanceSMS: {TotalBalanceSMS}", request.TotalBalanceSMS);
            var RouteTypeId = request.RouteTypeId ?? string.Empty;
            string UserId = request.UserID ?? string.Empty;
            try
            {
                if (string.IsNullOrEmpty(request.JobId))
                {
                    return BadRequest(new { status = "error", message = "JobId is required." });
                }

                var updateStatus = await _userStatusService.UpdateJobStatus(request.JobId, JobStatus.PROCESSING);
                if (!updateStatus)
                {
                    return NotFound(new { status = "error", message = "Job not found or update failed." });
                }

                var numbers = await _numberService.FetchNumbers(request.FilePath ?? string.Empty, request.Data?.UsersGroupId);
                var cleanedNumbers = numbers.Where(number => !string.IsNullOrEmpty(number)) 
                    .Select(number => number.Trim('"')) 
                    .ToList();
                var numberCount = numbers.Count;
                int totalSmsCount = request.SMSCount ?? 0;

                var PostSMSCount = numberCount * totalSmsCount;
                var PostMobileCount = PostSMSCount;

                // _logger.LogInformation("Fetched {numberCount} unique numbers and calculated total SMS count as {PostSMSCount}", numberCount, PostSMSCount);

                var updateJobSmsSent = await _userStatusService.UpdateJobSmsSent(request.JobId, PostSMSCount);
                if (!updateJobSmsSent)
                {
                    return NotFound(new { status = "error", message = "Failed to update SMS sent count for job." });
                }
                var smsLimit = request.Access?.SmsLimit;
                if (PostSMSCount > smsLimit)
                {
                    // _logger.LogWarning("PostSMSCount exceeds SMS Limit. PostSMSCount: {PostSMSCount}, UserID: {UserID}, SMS Limit: {smsLimit}", PostSMSCount, UserId, smsLimit);
                    var jobStatusUpdated = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.OVER_LIMIT, request.JobId);

                    if (!jobStatusUpdated)
                    {
                        return StatusCode(500, new { status = "error", message = "Failed to update job status to OVER_LIMIT." });
                    }
                    return BadRequest(new { status = "error", message = ErrorMessages.OVER_LIMIT});
                }

                // Initialize lists for categorized numbers
var blocked = new List<string>(numberCount);
var spoofed = new List<string>(numberCount);
var yesSmart = new List<string>(numberCount);
var noSmart = new List<string>(numberCount);

// Step 1: Fetch statuses for all numbers in a single query
var groupedNumbers = await _numberService.GetNumberManagementStatusByNumbersAsync(UserId, numbers);

// Step 2: Extract numbers by status
if (groupedNumbers.TryGetValue(UserStatus.BLOCKED, out var blockedNumbers))
{
    blocked.AddRange(blockedNumbers);
}

if (groupedNumbers.TryGetValue(UserStatus.SPOOFED, out var spoofedNumbers))
{
    spoofed.AddRange(spoofedNumbers);
}

// Step 3: Handle numbers with other statuses (e.g., ACTIVE, PENDING, etc.)
var otherStatusNumbers = groupedNumbers
    .Where(kvp => kvp.Key != UserStatus.BLOCKED && kvp.Key != UserStatus.SPOOFED)
    .SelectMany(kvp => kvp.Value)
    .ToList();

yesSmart.AddRange(otherStatusNumbers);

// Step 4: Handle numbers with no status (if needed)
var allFetchedNumbers = groupedNumbers.SelectMany(kvp => kvp.Value).ToList();
var missingNumbers = numbers.Except(allFetchedNumbers).ToList();

if (missingNumbers.Any())
{
    _logger.LogWarning("No status found for numbers: {MissingNumbers}", string.Join(", ", missingNumbers));
    // Optionally, add missing numbers to a specific list (e.g., yesSmart or a separate list)
    yesSmart.AddRange(missingNumbers);
}

/*
                var blocked = new List<string>(numberCount);
                var spoofed = new List<string>(numberCount);
                var yesSmart = new List<string>(numberCount);
                // var yesSmart = new List<string>(cleanedNumbers); // commented out for direct sending without check number status
                var noSmart = new List<string>(numberCount);
                foreach (var number in numbers)
                {
                    var numberStatus = await _numberService.GetNumberManagementStatusByNumberAsync(UserId, number);
                    switch (numberStatus)
                    {
                        case UserStatus.BLOCKED:
                            blocked.Add(number);
                            break;
                        case UserStatus.SPOOFED:
                            spoofed.Add(number);
                            break;
                        default:
                            yesSmart.Add(number);
                            break;
                    }
                }
                */

     async Task<IActionResult> ProcessNumbers(List<string> numbers)
    {
        // Step 1: Fetch statuses for all numbers in a single query
        var groupedNumbers = await _numberService.GetNumberManagementStatusByNumbersAsync(UserId, numbers);

        // Step 2: Extract numbers by status
        var blocked = groupedNumbers.TryGetValue(UserStatus.BLOCKED, out var blockedNumbers) 
            ? blockedNumbers 
            : new List<string>();

        var spoofed = groupedNumbers.TryGetValue(UserStatus.SPOOFED, out var spoofedNumbers) 
            ? spoofedNumbers 
            : new List<string>();

        var yesSmart = groupedNumbers
            .Where(kvp => kvp.Key != UserStatus.BLOCKED && kvp.Key != UserStatus.SPOOFED)
            .SelectMany(kvp => kvp.Value)
            .ToList();

        // Step 3: Handle numbers with no status (if needed)
        var allFetchedNumbers = groupedNumbers.SelectMany(kvp => kvp.Value).ToList();
        var missingNumbers = numbers.Except(allFetchedNumbers).ToList();

        if (missingNumbers.Any())
        {
            _logger.LogWarning("No status found for numbers: {MissingNumbers}", string.Join(", ", missingNumbers));
            // Optionally, add missing numbers to a specific list (e.g., yesSmart or a separate list)
            yesSmart.AddRange(missingNumbers);
        }

        // Step 4: Return or process the categorized lists
        return Ok(new { Blocked = blocked, Spoofed = spoofed, YesSmart = yesSmart });
    }

/*
    public async Task<IActionResult> ProcessNumbers(List<string> numbers)
    {
        var blocked = new List<string>();
        var spoofed = new List<string>();
        var yesSmart = new List<string>();

        // Step 1: Fetch statuses for all numbers in a single query
        var numberStatusMap = await _numberService.GetNumberManagementStatusByNumbersAsync(UserId, numbers);

        // Step 2: Categorize numbers based on the fetched statuses
        foreach (var number in numbers)
        {
            if (numberStatusMap.TryGetValue(number, out var status))
            {
                switch (status)
                {
                    case UserStatus.BLOCKED:
                        blocked.Add(number);
                        break;
                    case UserStatus.SPOOFED:
                        spoofed.Add(number);
                        break;
                    default:
                        yesSmart.Add(number);
                        break;
                }
            }
            else
            {
                // Handle numbers with no status (e.g., log or add to a separate list)
            }
        }

        // Step 3: Return or process the categorized lists
        return Ok(new { Blocked = blocked, Spoofed = spoofed, YesSmart = yesSmart });
    }
*/

                // var numberStatuses = await A.GetNumbersManagementStatusAsync(UserId, numbers);

                // foreach (var number in numbers)
                // {
                //     if (numberStatuses.TryGetValue(number, out var status))
                //     {
                //         switch (status)
                //         {
                //             case UserStatus.BLOCKED:
                //                 blocked.Add(number);
                //                 break;
                //             case UserStatus.SPOOFED:
                //                 spoofed.Add(number);
                //                 break;
                //             default:
                //                 yesSmart.Add(number);
                //                 break;
                //         }
                //     }
                //     else
                //     {
                //         yesSmart.Add(number); // Handle numbers not found in DB
                //     }
                // }


                // var smsLimit = request.Access?.SmsLimit;
                // if (PostSMSCount > smsLimit)
                // {
                //     _logger.LogWarning("PostSMSCount exceeds SMS Limit. PostSMSCount: {PostSMSCount}, UserID: {UserID}, SMS Limit: {smsLimit}", PostSMSCount, UserId, smsLimit);
                //     var jobStatusUpdated = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.OVER_LIMIT, request.JobId);

                //     if (!jobStatusUpdated)
                //     {
                //         return StatusCode(500, new { status = "error", message = "Failed to update job status to OVER_LIMIT." });
                //     }
                //     return BadRequest(new { status = "error", message = ErrorMessages.OVER_LIMIT});
                // }

                if (PostSMSCount <= 0)
                {
                    var jobStatusUpdated = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.INVALID_NUMBERS, request.JobId);

                    if (!jobStatusUpdated)
                    {
                        return StatusCode(500, new { status = "error", message = "Failed to update job status to INVALID NUMBERS." });
                    }
                    return BadRequest(new { status = "error", message = ErrorMessages.INVALID_NUMBERS});
                }

                var TotalBalanceSMS = await _balanceService.GetUsersSMSBalanceByRouteID(UserId, RouteTypeId);
                request.TotalBalanceSMS = TotalBalanceSMS ?? 0;

                if (request.OverSellingUsersCategoryName == Roles.ROLE_ADMIN)
                {
                    var OverSellingUsersID = request.OverSellingUsersID ?? string.Empty; 

                    var totalBalanceSmsAdminOverselling = await _balanceService.GetUsersSMSBalanceByRouteID(OverSellingUsersID, RouteTypeId);
                    request.TotalBalanceSMSAdminOverselling = totalBalanceSmsAdminOverselling ?? 0;
                }


                if (request.OverSellingUsersCategoryName == Roles.ROLE_ADMIN && request.TotalBalanceSMSAdminOverselling < PostSMSCount && (request.OverSellingActivated == "Yes" || string.IsNullOrEmpty(request.OverSellingActivated))) 
                {
                    // _logger.LogWarning("Overselling detected. UserID: {UserID}, Route: {Route}, Category: {Category}, Balance: {Balance}, SMSCount: {SMSCount}, Activated: {Activated}", UserId, RouteTypeId, request.OverSellingUsersCategoryName, request.TotalBalanceSMSAdminOverselling, PostSMSCount, request.OverSellingActivated);
                    
                    var jobStatusUpdated = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.OVERSELLING_LOW_BALANCE, request.JobId);
                    
                    if (!jobStatusUpdated)
                    {
                        return StatusCode(500, new { status = "error", message = "Failed to update job status to OVERSELLING LOW BALANCE." });
                    }
                    
                    return BadRequest(new { status = "error", message = ErrorMessages.OVERSELLING_LOW_BALANCE });
                }

                var values = new Dictionary<string, object>
                {
                    { "TotalBalanceSMS", TotalBalanceSMS ?? 0 },
                    { "PostSMSCount", PostSMSCount },
                    { "UserID", UserId },
                    { "Route", request.Route ?? string.Empty}
                };

                var options = new Dictionary<string, object>();  
                
                options = options.Concat(values).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (TotalBalanceSMS < PostSMSCount)
                {
                    // _logger.LogWarning("TotalBalanceSMS: {TotalBalanceSMS}, PostSMSCount: {PostSMSCount}, UserID: {UserID}, Route: {Route}", TotalBalanceSMS, PostSMSCount, UserId, request.Route);
                    
                    var updateSMSBalance = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.LOW_BALANCE, request.JobId);
                    return BadRequest(new { message = ErrorMessages.LOW_BALANCE });
                }

                if (request.Cutting == UserStatus.YES)
                {
                    if (!request.CutOffPercentage.HasValue)
                    {
                        return BadRequest(new { status = "error", message = "CutOffPercentage is required." });
                    }
                    int SendCount = (int)Math.Round(request.CutOffPercentage.Value * PostMobileCount / 100.0);
                    int BalanceCount = PostMobileCount - SendCount; 

                    values = new Dictionary<string, object>
                    {
                        { "SendCount", SendCount },
                        { "CutOffPercentage", request.CutOffPercentage },
                        { "PostMobileCount", PostMobileCount },
                        { "BalanceCount", BalanceCount }
                    };

                    options = options.Concat(values).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    if (PostSMSCount >= request.CuttingMinValue)
                    { 
                        while (yesSmart != null && yesSmart.Count > 0 && BalanceCount < yesSmart.Count)
                        {
                            var to = yesSmart.Last();  
                            yesSmart.RemoveAt(yesSmart.Count - 1);  
                            noSmart.Add(to);  
                        }
                    }
                    else
                    {
                        noSmart.AddRange(yesSmart);
                        yesSmart.Clear(); 
                    }
                    // _logger.LogTrace("SendCount: {SendCount}, CutOffPercentage: {CutOffPercentage}, PostMobileCount: {PostMobileCount}, BalanceCount: {BalanceCount}, : {PostSMSCount}", SendCount, request.CutOffPercentage, PostMobileCount, BalanceCount, PostSMSCount);
                }
                else
                {
                    if (yesSmart != null && yesSmart.Count > 0)
                    {
                        noSmart.AddRange(yesSmart);
                        yesSmart.Clear(); 
                    }
                }
                var blockCount = blocked.Count;
                var spoofedCount = spoofed.Count;
                var yesSmartCount = yesSmart?.Count ?? 0;
                var noSmartCount = noSmart.Count;
                var DebitBalance = PostSMSCount;
                var TotalBalanceSMSS = TotalBalanceSMS - PostSMSCount;

                // _logger.LogWarning("Blocked Count: {blockCount}, Spoofed Count: {spoofedCount}, Yes Smart Count: {yesSmartCount}, No Smart Count: {noSmartCount}", blockCount, spoofedCount, yesSmartCount, noSmartCount);

                int totalBalanceSMSAdmin = 0; // Initialize outside the if block

                if (request.OverSellingUsersCategoryName == Roles.ROLE_ADMIN && request.OverSellingActivated == "true" && request.UserCategory != Roles.ROLE_ADMIN)
                {
                    totalBalanceSMSAdmin = request.TotalBalanceSMSAdminOverselling - PostSMSCount;
                    options["TotalBalanceSMS"] = totalBalanceSMSAdmin;

                    var balancearr = new Dictionary<string, object>
                    {
                        { "DebitBalance", DebitBalance },
                        { "LoggedUserID", UserId },
                        { "route_type_id", RouteTypeId }
                    };

                    if (balancearr.TryGetValue("DebitBalance", out var debitBalanceObj) &&
                        balancearr.TryGetValue("LoggedUserID", out var loggedUserIdObj) &&
                        balancearr.TryGetValue("route_type_id", out var routeTypeIdObj))
                    { 
                        string? debitBalance = debitBalanceObj?.ToString();  
                        string? loggedUserId = loggedUserIdObj?.ToString();  
                        string? routeTypeId = routeTypeIdObj?.ToString();  
 
                        if (string.IsNullOrEmpty(debitBalance) || string.IsNullOrEmpty(loggedUserId) || string.IsNullOrEmpty(routeTypeId))
                        {
                            // _logger.LogWarning("One or more required parameters are null or empty.");
                            return BadRequest(new { message = "Required parameters are missing or invalid." });
                        }
 
                        var updateSMSBalance = await _balanceService.UpdateSMSBalanceAsync(debitBalance, loggedUserId, routeTypeId);

                        if (!updateSMSBalance)
                        {
                            // _logger.LogWarning("Failed to update SMS balance for user: {UserID}", UserId);
                            var updateJobStatusRemark = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.OVERSELLING_LOW_BALANCE, request.JobId);
                            return BadRequest(new { message = ErrorMessages.OVERSELLING_LOW_BALANCE });
                        }
                    }
                    else
                    {
                        // _logger.LogWarning("Required parameters are missing from balancearr.");
                        return BadRequest(new { message = "Required parameters are missing." });
                    }
                }

                var NewDebitBalance = DebitBalance.ToString();
                Log.Information("NewDebitBalance = "+ NewDebitBalance);

                var updateSMSBalanceMain = await _balanceService.UpdateSMSBalanceAsync(NewDebitBalance, UserId, RouteTypeId);

                if (!updateSMSBalanceMain)
                {
                    // _logger.LogWarning("Failed to update SMS balance for user: {UserID}", UserId);
                    var updateJobStatusRemark = await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, ErrorMessages.LOW_BALANCE, request.JobId);
                    return BadRequest(new { message = ErrorMessages.OVERSELLING_LOW_BALANCE });
                }
                
                await _userNotificationService.JobNotificationAsync(UserId, request.UserFullName ?? string.Empty, request.JobId, PostSMSCount, RouteTypeId, TotalBalanceSMSS ?? 0);


                if (request.OverSellingUsersCategoryName == Roles.ROLE_ADMIN && (request.OverSellingActivated == "Yes" || string.IsNullOrEmpty(request.OverSellingActivated)) &&
                    request.UserCategory != Roles.ROLE_ADMIN)
                    {
                        var JobSmartSMSNo = yesSmartCount;
                        string OverSellingUsersID = request.OverSellingUsersID ?? string.Empty;
                        await _userNotificationService.AdminJobNotificationAsync(OverSellingUsersID, UserId, request.UserFullName ?? string.Empty, request.JobId, PostSMSCount, RouteTypeId, totalBalanceSMSAdmin);
                        
                        await _userNotificationService.AdminCuttingRefundAsync(OverSellingUsersID, UserId, request.UserFullName ?? string.Empty, RouteTypeId, request.JobId, JobSmartSMSNo, request.SMSCount, PostSMSCount);
                    }

                var updateStatus2 = await _userStatusService.UpdateJobStatus(request.JobId, JobStatus.COMPLETED);
                // _logger.LogInformation("Update status2------------> {updateStatus2}",updateStatus2);
                if (!updateStatus2)
                {
                    return NotFound(new { status = "error", message = "Job not found or update failed." });
                } 
                
                var coding = request.Coding;
                var dlrUrl = SmsConfig.DLR_URL
                    .Replace(SmsConfig.JOB_PH, request.JobId.ToString())
                    .Replace(SmsConfig.USER_PH, UserId.ToString()); 
                
                var peid = request.Peid;
                var tid = request.Tid;
                var HashId = request.HashId;
                // _logger.LogInformation("peid = {peid}, tid = {tid}, HashId = {HashId}, jobId = {jobId}, userId = {userId}", peid, tid, HashId, request.JobId, UserId);
                // if (!long.TryParse(peid, out long parsedPeid) || !long.TryParse(tid, out long parsedTid))
                // {
                //     _logger.LogError("Invalid Peid or Tid: Peid={peid}, Tid={tid}",peid,tid);
                //     return BadRequest(new { status = "error", message = "Invalid Peid or Tid" });
                // }
                var dltInfo = string.Format(SmsConfig.TLV_INFO, peid, tid, HashId, request.JobId, UserId);
                // var smsMessage = request.MsgStat?.Msg;
                // var smsLength = request.MsgStat?.Chars;

                // _logger.LogInformation("chalu krte hai ab smsdata ka kaam ------------>{dltInfo}",dltInfo);
                var smsData = new Dictionary<string, string>
                {
                    { "username", SmsConfig.SMS_USER },
                    { "password", SmsConfig.SMS_PSWD },
                    { "from", request.Sender ?? string.Empty }, 
                    { "text", request.Data?.SmsMessage ?? string.Empty },  
                    // { "smsc", request.Smsc ?? string.Empty },   
                    { "account", UserId.ToString()}, 
                    { "dlr-mask", SmsConfig.DLR_MASK.ToString()},    // Allows DLR
                    // { "dlr-url", dlrUrl},                    // CAUTION     // If Uncommented then DLR will be sent to the URL & will notbe run through PHP service
                    { "coding", coding.ToString() ?? string.Empty}, 
                    // { "mclass", request.Mclass ?? "0" }, 
                    { "meta-data", SmsConfig.TLV_SMPP + dltInfo }, 
                };
                // _logger.LogInformation("The SMS DATA------{smsData}",smsData);
                
                var smsParam = "?" + await new FormUrlEncodedContent(smsData).ReadAsStringAsync();
                var data = new Dictionary<string, object>();
                var SendingOptions = data.Concat(keyValuePairs).ToList(); 
                // Log.Information("Sendingoption data = "  + SendingOptions);
                var sendingData = new Dictionary<string, object>
                {
                    { "status", UserStatus.SUBMITTED },
                    { "is_smart", UserStatus.NO },
                    { "dnd", UserStatus.NO },
                    { "block", UserStatus.NO },
                    { "refund", UserStatus.NO }
                };
                // _logger.LogInformation("sendingData INFO {sendingData}",sendingData);
                
                if (spoofed != null)
                {
                    var spoofedData = new SendSmsRequest
                    {
                        NumberArray = spoofed,
                        AdditionalData = sendingData,
                        Options2 = SendingOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        MsgStat = request.MsgStat,
                        SmsSend = true,
                        SmsParam = smsParam
                    };
                    await SendAndInsertSMS(spoofedData);
                }
                // _logger.LogInformation("spoofed INFO {spoofed}",spoofed);

                // Handle non-smart numbers
                if (noSmart != null)
                {
                    var nonSmartData = new SendSmsRequest
                    {
                        NumberArray = noSmart,
                        AdditionalData = sendingData,
                        Options2 = SendingOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        MsgStat = request.MsgStat,
                        SmsSend = true,
                        SmsParam = smsParam
                    };
                    await SendAndInsertSMS(nonSmartData);
                // _logger.LogInformation("nonSmartData INFO {nonSmartData}",nonSmartData);
                }

                // Handle smart numbers
                if (yesSmart != null)
                {
                    var smartData = new Dictionary<string, object>
                    {
                        { "status", request.CutoffStatusName ?? string.Empty },
                        { "is_smart", UserStatus.YES },
                        { "dnd", UserStatus.NO },
                        { "block", UserStatus.NO },
                        { "refund", UserStatus.NO }
                    };

                    var smartRequest = new SendSmsRequest
                    {
                        NumberArray = yesSmart,
                        AdditionalData = smartData,
                        Options2 = SendingOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        MsgStat = request.MsgStat,
                        SmsSend = false
                    };
                    await SendAndInsertSMS(smartRequest);
                // _logger.LogInformation("smartRequest INFO {smartRequest}",smartRequest);
                }

                // Handle blocked numbers
                if (blocked != null)
                {
                    var blockData = new Dictionary<string, object>
                    {
                        { "status", UserStatus.NUMBER_BLOCKED },
                        { "is_smart", UserStatus.NO },
                        { "dnd", UserStatus.NO },
                        { "block", UserStatus.YES },
                        { "refund", UserStatus.YES }
                    };

                    var blockedRequest = new SendSmsRequest
                    {
                        NumberArray = blocked,
                        AdditionalData = blockData,
                        Options2 = SendingOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        MsgStat = request.MsgStat,
                        SmsSend = false
                    };
                    await SendAndInsertSMS(blockedRequest);
                }

               
                return Ok(new { status = "success", smsLimit, numberCount, PostSMSCount, TotalBalanceSMS, TotalBalanceSMSS });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the job. Exception message: {Message}. ", ex.Message);
                return StatusCode(500);
            }
        }
        public class SendSmsRequest
        {
            public List<string> NumberArray { get; set; } = [];
            public Dictionary<string, object> AdditionalData { get; set; } = [];
            public Dictionary<string, object> Options2 { get; set; } = [];
            public object? MsgStat { get; set; }
            public bool SmsSend { get; set; } = false; // Default value
            public string? SmsParam { get; set; }
        }

        
        // [HttpPost("sendandsendsms")]
    // private async Task<IActionResult> SendAndInsertSMS([FromBody] SendSmsRequest request)
    // { 
    //     // _logger.LogInformation("SendAndInsertSMS ------------>{request}",request);
    //     var numberArray = request.NumberArray;
    //     var additionalData = request.AdditionalData;
    //     var SendingOptions = request.Options2;
    //     // _logger.LogInformation("SendingOptions ------------>{SendingOptions}",SendingOptions);
        
    //     string loggedUserId = SendingOptions["UserID"]?.ToString() ?? string.Empty;
    //     var msgStat = request.MsgStat ?? string.Empty;
    //     var smsSend = request.SmsSend;
    //     var smsParam = request.SmsParam;

    //     var batchSql = new List<string>();
    //     var batchParams = new List<List<MySqlParameter>>();  // To hold the parameters for each SQL query
    //     var curlHandlers = new List<HttpClient>();
    //     var requests = new List<HttpRequestMessage>();
    //     // int batch = 0;
    //     // const int batchSize = 5000;
    //     var jobId = SendingOptions.TryGetValue("job_id", out object? value) ? value.ToString() : null;

    //     // Retrieve and validate both SMPP SMSC IDs and Names
    //     if (!SendingOptions.TryGetValue("SMPPSMSCID", out var smppSMSCIDsValue) || !SendingOptions.TryGetValue("SMPPSMSCIDName", out var smppSMSCNamesValue))
    //     {
    //         // _logger.LogError("SMPPSMSCIDs or SMPPSMSCNames key not found in SendingOptions.");
    //         return StatusCode(400, "Missing SMPPSMSCIDs or SMPPSMSCNames in SendingOptions.");
    //     }

    //     var smppSMSCIDs = smppSMSCIDsValue?.ToString()?.Split(',').Select(id => int.TryParse(id, out var result) ? result : (int?)null).Where(id => id.HasValue).Select(id => id.Value).ToList() ?? new List<int>(); 
    //     var smppSMSCNames = smppSMSCNamesValue.ToString().Split(',').Where(name => !string.IsNullOrEmpty(name)).ToList();
        

    //     if (smppSMSCIDs.Count != smppSMSCNames.Count)
    //     {
    //         // _logger.LogError("SMPPSMSCIDs and SMPPSMSCNames have different lengths.");
    //         return StatusCode(400, "SMPPSMSCIDs and SMPPSMSCNames have different lengths.");
    //     }
                
    //     try
    //     {
    //         int smscIndex = 0;
    //         using var httpClient = new HttpClient();
    //         foreach (var to in numberArray)
    //         {
    //             var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    //             var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    //             var jdate = _SmsService.JulianDate(time);
    //             var status = additionalData["status"];
    //             var isSmart = additionalData["is_smart"];
    //             var dnd = additionalData["dnd"];
    //             var block = additionalData["block"];
    //             var refund = additionalData["refund"];
    //             var msgId = _SmsService.GenerateMessageID(loggedUserId);

    //             var smscId = smppSMSCIDs[smscIndex];
    //             var smscName = smppSMSCNames[smscIndex];
    //             smscIndex = (smscIndex + 1) % smppSMSCIDs.Count;
    //             // _logger.LogInformation($"Using SMPP_SMSC: {smscName} ({smscId}) for number: {to} smscindex: ({smscIndex})");

    //             var info = new Dictionary<string, object>
    //             {
    //                 { "dnd", dnd },
    //                 { "block", block },
    //                 { "refund", refund },
    //                 { "is_smart", isSmart },
    //                 { "status", status },
    //                 { "msg_id", msgId },
    //                 { "to", to },
    //                 { "jdate", jdate },
    //                 { "sent_at", sentAt },
    //                 { "msg_stat", msgStat },
    //             };

    //             foreach (var option in SendingOptions)
    //             {
    //                 info[option.Key] = option.Value;
    //             }

    //             info["SMPPSMSCID"] = smscId;
    //             info["SMPPSMSCIDName"] = smscName;

    //             // Retrieve the SQL and parameters from BuildSmsSqlAsync
    //             var smsMessage = await _SmsService.BuildSmsSql(info);

    //             // _logger.LogInformation("info DATA " + info);
    //             // _logger.LogInformation("SMSMESSAGE DATA " + smsMessage);
    //             // batchSql.Add(sql);  
    //             // batchParams.Add(parameters);   

    //             // Handle SMS sending
    //             if (smsSend)
    //             {
    //                 var url = SmsConfig.SMS_CGI;
    //                 // url += smsParam + "&to=" + Uri.EscapeDataString(to);
    //                 url += smsParam + "&to=" + Uri.EscapeDataString(to) + "&smsc=" + Uri.EscapeDataString(smscName);
    //                 url = url.Replace(Uri.EscapeDataString("${msg_id}"), msgId);
    //                 url = Regex.Replace(url, @"text=[^&]*", "text=" + HttpUtility.UrlEncode(smsMessage));
    //                 //    _logger.LogInformation("Kannel URL" + url);
    //                 var ch = new HttpClient();
    //                 curlHandlers.Add(ch);

    //                 var response = await ch.GetAsync(url);
    //                 if (response.IsSuccessStatusCode)
    //                 {
    //                     _logger.LogTrace("Response hai ye------>{response}", response);
    //                 }
    //                 else
    //                 {
    //                     _logger.LogInformation("Failed to send SMS to: {to}, Status: {response.StatusCode}", to, response);
    //                 }
    //             } 
    //         } 
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogInformation($"Error in SendAndInsertSMS: {ex.Message}");
    //         return StatusCode(500, "Internal server error");
    //     }
    //     // finally
    //     // {
    //     //     _ = _SmsService.FailedJobsAsync();
    //     // }

    //     return Ok();
    // }





// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================

// private async Task<IActionResult> SendAndInsertSMS([FromBody] SendSmsRequest request)
// { 
//     var numberArray = request.NumberArray;
//     var additionalData = request.AdditionalData;
//     var SendingOptions = request.Options2;
    
//     string loggedUserId = SendingOptions["UserID"]?.ToString() ?? string.Empty;
//     var msgStat = request.MsgStat ?? string.Empty;
//     var smsSend = request.SmsSend;  // No longer needed since we won't send SMS
//     var smsParam = request.SmsParam;

//     var batchSql = new List<string>();
//     var batchParams = new List<List<MySqlParameter>>();  

//     var jobId = SendingOptions.TryGetValue("job_id", out object? value) ? value.ToString() : null;

//     // Validate SMPP SMSC IDs and Names
//     if (!SendingOptions.TryGetValue("SMPPSMSCID", out var smppSMSCIDsValue) || !SendingOptions.TryGetValue("SMPPSMSCIDName", out var smppSMSCNamesValue))
//     {
//         return StatusCode(400, "Missing SMPPSMSCIDs or SMPPSMSCNames in SendingOptions.");
//     }

//     var smppSMSCIDs = smppSMSCIDsValue?.ToString()?.Split(',')
//         .Select(id => int.TryParse(id, out var result) ? result : (int?)null)
//         .Where(id => id.HasValue)
//         .Select(id => id.Value)
//         .ToList() ?? new List<int>(); 
    
//     var smppSMSCNames = smppSMSCNamesValue.ToString().Split(',')
//         .Where(name => !string.IsNullOrEmpty(name))
//         .ToList();

//     if (smppSMSCIDs.Count != smppSMSCNames.Count)
//     {
//         return StatusCode(400, "SMPPSMSCIDs and SMPPSMSCNames have different lengths.");
//     }

//         var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//         var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//         var jdate = _SmsService.JulianDate(time);
//         var status = additionalData["status"];
//         var isSmart = additionalData["is_smart"];
//         var dnd = additionalData["dnd"];
//         var block = additionalData["block"];
//         var refund = additionalData["refund"];
//         var msgId = _SmsService.GenerateMessageID(loggedUserId);

//         int smscIndex = 0;

//         var smscId = smppSMSCIDs[smscIndex];
//         var smscName = smppSMSCNames[smscIndex];
//         smscIndex = (smscIndex + 1) % smppSMSCIDs.Count;

//         var info = new Dictionary<string, object>
//         {
//             { "dnd", dnd },
//             { "block", block },
//             { "refund", refund },
//             { "is_smart", isSmart },
//             { "status", status },
//             // { "msg_id", msgId },
//             // { "to", to },
//             { "jdate", jdate },
//             // { "sent_at", sentAt },
//             { "msg_stat", msgStat },
//         };

//         foreach (var option in SendingOptions)
//         {
//             info[option.Key] = option.Value;
//         }

//         // info["SMPPSMSCID"] = smscId;
//         // info["SMPPSMSCIDName"] = smscName;

//         // List<Dictionary<string, object>> message = new List<Dictionary<string, object>>();


//         // send numarray and everything else to func 2
    
//     // try
//     // {
//     //     foreach (var to in numberArray)
//     //     {

//     //         // Build SQL for inserting the SMS data
//     //         var smsMessage = await _SmsService.BuildSmsSql(info);
            
//     //         // Here, you should execute the SQL insert operation
//     //         // Example:
//     //         await _SmsService.InsertSmsDataAsync(smsMessage);
//     //     } 
//     // }
//     // catch (Exception ex)
//     // {
//     //     _logger.LogInformation($"Error in SendAndInsertSMS: {ex.Message}");
//     //     return StatusCode(500, "Internal server error");
//     // }

//     // message.Add(info);

//     await ProcessBatches(numberArray, info);



//     return Ok();
// }





// //  async Task ProcessBatches(List<string> numbers, Dictionary<string, object> message)
// // {
// //     int batchSize = 1000;
// //     List<Task> tasks = new List<Task>();

// //     for (int i = 0; i < numbers.Count; i += batchSize)
// //     {
// //         var batch = numbers.GetRange(i, Math.Min(batchSize, numbers.Count - i));
// //         tasks.Add(SendMessagesAsync(batch, message));
// //     }

// //     await Task.WhenAll(tasks); // Run all batches in parallel
// // }


// //  async Task SendMessagesAsync(List<string> numbers, Dictionary<string, object> message)
// // {
// //     using HttpClient client = new HttpClient();
// //     List<Task> tasks = new List<Task>();


// //     foreach (var number in numbers)
// //     {
// //         // assign  to = number
// //         message["to"] = number;

// //         tasks.Add(_SmsService.BuildSmsSql(message));
// //     }

// //     await Task.WhenAll(tasks); // Send all messages concurrently



// // }



// private async Task ProcessBatches(List<string> numbers, Dictionary<string, object> message)
// {
//     int batchSize = 1000;
//     List<Task> tasks = new List<Task>();

//     for (int i = 0; i < numbers.Count; i += batchSize)
//     {
//         var batch = numbers.GetRange(i, Math.Min(batchSize, numbers.Count - i));
//         tasks.Add(SendMessagesAsync(batch, message, _serviceScopeFactory)); // ✅ Pass service scope factory
//     }

//     await Task.WhenAll(tasks);
// }


// private async Task SendMessagesAsync(List<string> numbers, Dictionary<string, object> message, IServiceScopeFactory serviceScopeFactory)
// {
//     List<Task> tasks = new List<Task>();


//     foreach (var number in numbers)
//     {
//         var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

//         var SendingOptions = request.Options2;
//         string loggedUserId = SendingOptions["UserID"]?.ToString() ?? string.Empty;
//         var msgId = _SmsService.GenerateMessageID(loggedUserId);
//         var clonedMessage = new Dictionary<string, object>(message) // ✅ Clone message to avoid race conditions
//         {
//         // { "sent_at", sentAt },
//         // info["SMPPSMSCID"] = smscId;
//         // info["SMPPSMSCIDName"] = smscName;

//             ["sent_at"] = sentAt,
//             ["SMPPSMSCID"] = smscId,
//             ["SMPPSMSCIDName"] = smscName,
//             ["msg_id"] = msgId,
//             ["to"] = number
//         };

//         tasks.Add(Task.Run(async () =>
//         {
//             using var scope = serviceScopeFactory.CreateScope(); // ✅ Create a new scope
//             var smsService = scope.ServiceProvider.GetRequiredService<SmsService>(); // ✅ Get a new instance of SmsService

//             await smsService.BuildSmsSql(clonedMessage); // ✅ Each task now has its own DbContext
//         }));
//     }

//     await Task.WhenAll(tasks);
// }



// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// =========================================== Only for SMS Table Entries====================================================
// =============================================== No DLR Sending ===========================================================
// ===================================================    ||   ================================================================
// ===================================================    ||    ===============================================================
// ==================================================   \ || /   =================================================================
// ===================================================   \  /   ==============================================================
// =======================================================\/===============================================================


// private async Task<IActionResult> SendAndInsertSMS([FromBody] SendSmsRequest request)
// { 
//     var numberArray = request.NumberArray;
//     var additionalData = request.AdditionalData;
//     var SendingOptions = request.Options2;
    
//     string loggedUserId = SendingOptions["UserID"]?.ToString() ?? string.Empty;
//     var msgStat = request.MsgStat ?? string.Empty;
//     var smsSend = request.SmsSend;
//     var smsParam = request.SmsParam;

//     var jobId = SendingOptions.TryGetValue("job_id", out object? value) ? value.ToString() : null;

//     // ✅ Validate SMPP SMSC IDs and Names
//     if (!SendingOptions.TryGetValue("SMPPSMSCID", out var smppSMSCIDsValue) || 
//         !SendingOptions.TryGetValue("SMPPSMSCIDName", out var smppSMSCNamesValue))
//     {
//         throw new ArgumentException("Missing SMPPSMSCIDs or SMPPSMSCNames in SendingOptions.");
//     }

//     var smppSMSCIDs = smppSMSCIDsValue?.ToString()?.Split(',')
//         .Select(id => int.TryParse(id, out var result) ? result : (int?)null)
//         .Where(id => id.HasValue)
//         .Select(id => id.Value)
//         .ToList() ?? new List<int>(); 
    
//     var smppSMSCNames = smppSMSCNamesValue.ToString().Split(',')
//         .Where(name => !string.IsNullOrEmpty(name))
//         .ToList();

//     if (smppSMSCIDs.Count != smppSMSCNames.Count)
//     {
//         throw new ArgumentException("SMPPSMSCIDs and SMPPSMSCNames have different lengths.");
//     }

//     var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//     var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//     var jdate = _SmsService.JulianDate(time);
//     var status = additionalData["status"];
//     var isSmart = additionalData["is_smart"];
//     var dnd = additionalData["dnd"];
//     var block = additionalData["block"];
//     var refund = additionalData["refund"];
    
//     int smscIndex = 0;
//     var smscId = smppSMSCIDs[smscIndex];
//     var smscName = smppSMSCNames[smscIndex];
    
//     smscIndex = (smscIndex + 1) % smppSMSCIDs.Count;

//     var info = new Dictionary<string, object>
//     {
//         { "dnd", dnd },
//         { "block", block },
//         { "refund", refund },
//         { "is_smart", isSmart },
//         { "status", status },
//         { "jdate", jdate },
//         { "msg_stat", msgStat }
//     };

//     foreach (var option in SendingOptions)
//     {
//         info[option.Key] = option.Value;
//     }

//     // ✅ Send in batches for performance optimization
//     await ProcessBatches(numberArray, info);

//     return Ok();
// }

// private async Task ProcessBatches(List<string> numbers, Dictionary<string, object> message)
// {
//     int batchSize = 1000;
//     List<Task> tasks = new List<Task>();

//     for (int i = 0; i < numbers.Count; i += batchSize)
//     {
//         var batch = numbers.GetRange(i, Math.Min(batchSize, numbers.Count - i));
//         tasks.Add(SendMessagesAsync(batch, message, _serviceScopeFactory));
//     }

//     await Task.WhenAll(tasks);
// }

// private async Task SendMessagesAsync(List<string> numbers, Dictionary<string, object> message, IServiceScopeFactory serviceScopeFactory)
// {
//     List<Task> tasks = new List<Task>();

//     foreach (var number in numbers)
//     {
//         var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//         var msgId = _SmsService.GenerateMessageID(message["UserID"].ToString());

//         var clonedMessage = new Dictionary<string, object>(message)
//         {
//             ["sent_at"] = sentAt,
//             ["msg_id"] = msgId,
//             ["to"] = number
//         };

//         tasks.Add(Task.Run(async () =>
//         {
//             using var scope = serviceScopeFactory.CreateScope();
//             var smsService = scope.ServiceProvider.GetRequiredService<SmsService>();

//             await smsService.BuildSmsSql(clonedMessage);
//         }));
//     }

//     await Task.WhenAll(tasks);
// }




// ==========================================================================================================================
// ==========================================================================================================================
// ==========================================================================================================================
// =========================================== for Table Entries & DLR ====================================================
// ===================================================    ||   ================================================================
// ===================================================    ||    ===============================================================
// ==================================================   \ || /   =================================================================
// ===================================================   \  /   ==============================================================
// =======================================================\/===============================================================



    private async Task<IActionResult> SendAndInsertSMS([FromBody] SendSmsRequest request)
    { 
        var numberArray = request.NumberArray;
        var additionalData = request.AdditionalData;
        var SendingOptions = request.Options2;
        
        string loggedUserId = SendingOptions["UserID"]?.ToString() ?? string.Empty;
        var msgStat = request.MsgStat ?? string.Empty;
        var smsSend = request.SmsSend;
        var smsParam = request.SmsParam;

        var jobId = SendingOptions.TryGetValue("job_id", out object? value) ? value.ToString() : null;

        // ✅ Validate SMPP SMSC IDs and Names
        if (!SendingOptions.TryGetValue("SMPPSMSCID", out var smppSMSCIDsValue) || 
            !SendingOptions.TryGetValue("SMPPSMSCIDName", out var smppSMSCNamesValue))
        {
            throw new ArgumentException("Missing SMPPSMSCIDs or SMPPSMSCNames in SendingOptions.");
        }

        var smppSMSCIDs = smppSMSCIDsValue?.ToString()?.Split(',')
            .Select(id => int.TryParse(id, out var result) ? result : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .ToList() ?? new List<int>();

        // var smppSMSCIDs = ConsmppSMSCIDs.ConvertAll(id => id.ToString());

        
        var smppSMSCNames = smppSMSCNamesValue.ToString().Split(',')
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        if (smppSMSCIDs.Count != smppSMSCNames.Count)
        {
            throw new ArgumentException("SMPPSMSCIDs and SMPPSMSCNames have different lengths.");
        }

        var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var jdate = _SmsService.JulianDate(time);
        var status = additionalData["status"];
        var isSmart = additionalData["is_smart"];
        var dnd = additionalData["dnd"];
        var block = additionalData["block"];
        var refund = additionalData["refund"];
        
        int smscIndex = 0;
        var smscId = smppSMSCIDs[smscIndex];
        var smscName = smppSMSCNames[smscIndex];
        
        smscIndex = (smscIndex + 1) % smppSMSCIDs.Count;

        var info = new Dictionary<string, object>
        {
            { "dnd", dnd },
            { "block", block },
            { "refund", refund },
            { "is_smart", isSmart },
            { "status", status },
            { "jdate", jdate },
            { "msg_stat", msgStat },
            { "SmsSend", smsSend }
        };

        foreach (var option in SendingOptions)
        {
            info[option.Key] = option.Value;
        }

        // ✅ Send in batches for performance optimization
        await SendMessagesAsync(numberArray, info, smsParam, smppSMSCNames, smppSMSCIDs, _serviceScopeFactory);

        return Ok();
    }


// --------------------
// --------------------

    // private async Task ProcessBatches(List<string> numbers, Dictionary<string, object> message, string smsParam, List<string> smppSMSCNames, List<int> smppSMSCIDs)
    // {
    //     int batchSize = 10000;
    //     List<Task> tasks = new List<Task>();

    // // bulk insert records to users_sms table using bulk insert query or sql bulk copy in mariadb
    //     for (int i = 0; i < numbers.Count; i += batchSize)
    //     {
    //         var batch = numbers.GetRange(i, Math.Min(batchSize, numbers.Count - i));
    //         tasks.Add(SendMessagesAsync(batch, message, _serviceScopeFactory, smsParam, smppSMSCNames, smppSMSCIDs));
    //     }

    //     await Task.WhenAll(tasks);
    // }

// --------------------
// --------------------

    // private async Task ProcessBatches(List<string> numbers, Dictionary<string, object> message, string smsParam, List<string> smppSMSCNames)
    // {
    //     int batchSize = 1000;
    //     int maxConcurrentBatches = 10; // Only 10 batches at a time
    //     var semaphore = new SemaphoreSlim(maxConcurrentBatches);
    //     List<Task> tasks = new List<Task>();

    //     for (int i = 0; i < numbers.Count; i += batchSize)
    //     {
    //         await semaphore.WaitAsync(); // Wait for an available slot

    //         var batch = numbers.GetRange(i, Math.Min(batchSize, numbers.Count - i));
            
    //         var task = SendMessagesAsync(batch, message, _serviceScopeFactory, smsParam, smppSMSCNames)
    //             .ContinueWith(t => semaphore.Release()); // Release slot when done

    //         tasks.Add(task);
    //     }

    //     await Task.WhenAll(tasks);
    // }


private async Task SendMessagesAsync(List<string> numbers, Dictionary<string, object> message, string smsParam, List<string> smppSMSCNames, List<int> smppSMSCIDs, IServiceScopeFactory serviceScopeFactory)
    {

        List<Task> tasks = new List<Task>();
        int smscIndex = 0;

        // foreach (var number in numbers)
        // {
            var sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var msgId = _SmsService.GenerateMessageID(message["UserID"].ToString());
            var smscName = smppSMSCNames[smscIndex];
            smscIndex = (smscIndex + 1) % smppSMSCNames.Count;

        
            var clonedMessage = new Dictionary<string, object>(message)
            {
                ["sent_at"] = sentAt,
                // ["msg_id"] = msgId,
                ["to"] = string.Join(",", numbers),  // Combine all numbers into a single string (if needed)
                ["smsParam"] = smsParam,
                ["smscName"] = smscName,
                ["CleanMessageSmartUrl"] = message.GetValueOrDefault("CleanMessageSmartUrl")?.ToString() ?? string.Empty // Add this line

            };

            tasks.Add(Task.Run(async () =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                var smsService = scope.ServiceProvider.GetRequiredService<SmsService>();
                // var smsMessage = await smsService.BuildSmsSql(clonedMessage);


                var smsMessage = await smsService.BuildSmsSql(clonedMessage, serviceScopeFactory, smsParam, smppSMSCNames, smppSMSCIDs);
                _logger.LogInformation("smppSMSCNames AJAY ------------>{smscName}",smscName);

            /*
                // Handle SMS sending
                if ((bool)message["SmsSend"])
                {
                    var numbersString = string.Join(",", numbers);  // Convert List<string> to CSV format

                    var url = SmsConfig.SMS_CGI;
                    url += smsParam + "&to=" + Uri.EscapeDataString(numbersString) + "&smsc=" + Uri.EscapeDataString(smscName);
                    url = url.Replace(Uri.EscapeDataString("${msg_id}"), msgId);
                    url = Regex.Replace(url, @"text=[^&]*", "text=" + HttpUtility.UrlEncode(smsMessage));

                    // using var httpClient = new HttpClient();
                    // OR
                    var httpClient = _httpClientFactory.CreateClient();

                    var response = await httpClient.GetAsync(url);

                    // if (response.IsSuccessStatusCode)
                    // {
                    //     _logger.LogTrace("SMS Sent Successfully to {to}", number);
                    // }
                    // else
                    // {
                    //     _logger.LogInformation("Failed to send SMS to: {to}, Status: {status}", number, response.StatusCode);
                    // }
                }
            */
            }));
        // }

        await Task.WhenAll(tasks);
    }

// ==========================================================================================================================
// ==========================================================================================================================
// ---------------------------------------------- Dynamic ---------------------------------------------------
// ---------------------------------------------- Dynamic ---------------------------------------------------
// ---------------------------------------------- Dynamic ---------------------------------------------------

// private async Task BatchInsertSmsDataAsync(List<Dictionary<string, string>> mobileNumbersWithData, ProcessJobRequest request, string mobileColumnHeader, string smsMessage, int SmsMessageLength, string currentGateway, string currentGatewayID, string messageID)
// {
//     var batchSize = 2000;
//     var batches = mobileNumbersWithData.Select((x, i) => new { Index = i, Value = x })
//                                        .GroupBy(x => x.Index / batchSize)
//                                        .Select(x => x.Select(v => v.Value).ToList())
//                                        .ToList();

//     var insertTasks = new List<Task>();

//     foreach (var batch in batches)
//     {
//         // Create a task for each batch insertion
//         var task = InsertBatchAsync(batch, request, mobileColumnHeader, smsMessage, SmsMessageLength, currentGateway, currentGatewayID, messageID);
//         insertTasks.Add(task);
//     }

//     // Execute all batch insertions in parallel
//     await Task.WhenAll(insertTasks);
// }

// private async Task InsertBatchAsync(List<Dictionary<string, string>> batch, ProcessJobRequest request, string mobileColumnHeader, string smsMessage, int SmsMessageLength, string currentGateway, string currentGatewayID, string messageID)
// {
//     var insertQuery = new StringBuilder("INSERT INTO users_sms (job_id, job_status, user_id, parent_user_id, gateway_family_id, route_type_id, message_format, message_method, flash_sms, schedule_sms, schedule_date, schedule_time, sender_id, sender_name, mobile_number, message_type, sms_message, urlencode_sms_message, sms_length, sms_count, smsc_id, smsc_name, mclass, coding, charset, dnd, smart, refund, block, sent_at, message_id, status, jdate, meta_data) VALUES ");

//     var parameters = new List<MySqlParameter>();
//     var valueStrings = new List<string>();

//     for (int i = 0; i < batch.Count; i++)
//     {
//         var data = batch[i];
//         var mobileNumber = data[mobileColumnHeader];

//         valueStrings.Add($"( @JobId{i}, @JobStatus{i}, @LoggedUserId{i}, @LoggedUserParentID{i}, @GatewayFamilyId{i}, @RouteTypeId{i}, @MessageFormat{i}, @MessageMethod{i}, @FlashSms{i}, @ScheduleSms{i}, @ScheduleDate{i}, @ScheduleTime{i}, @SenderId{i}, @SenderName{i}, @ToNumber{i}, @MessageType{i}, @SmsMessage{i}, @UrlencodeSmsMessage{i}, @SmsMessageLength{i}, @SmsCount{i}, @SmppSmscId{i}, @SmppSmscIdName{i}, @Mclass{i}, @Coding{i}, @Charset{i}, @DndCount{i}, @SmartSmsNo{i}, @RefundCount{i}, @BlockCount{i}, @Pdate{i}, @messageID{i}, @MessageStatus{i}, @Jdate{i}, @MetaData{i} )");

//         parameters.AddRange(new[]
//         {
//             new MySqlParameter($"@JobId{i}", request.JobId),
//             new MySqlParameter($"@JobStatus{i}", request.JobStatus),
//             new MySqlParameter($"@LoggedUserId{i}", request.LoggedUserId),
//             new MySqlParameter($"@LoggedUserParentID{i}", request.LoggedUserParentID),
//             new MySqlParameter($"@GatewayFamilyId{i}", request.GatewayFamilyId),
//             new MySqlParameter($"@RouteTypeId{i}", request.RouteTypeId),
//             new MySqlParameter($"@MessageFormat{i}", request.MessageFormat),
//             new MySqlParameter($"@MessageMethod{i}", request.MessageMethod),
//             new MySqlParameter($"@FlashSms{i}", request.FlashSms),
//             new MySqlParameter($"@ScheduleSms{i}", request.ScheduleSms),
//             new MySqlParameter($"@ScheduleDate{i}", request.ScheduleDate),
//             new MySqlParameter($"@ScheduleTime{i}", request.ScheduleTime),
//             new MySqlParameter($"@SenderId{i}", request.SenderId),
//             new MySqlParameter($"@SenderName{i}", request.SenderName),
//             new MySqlParameter($"@ToNumber{i}", mobileNumber),
//             new MySqlParameter($"@MessageType{i}", request.MessageType),
//             new MySqlParameter($"@SmsMessage{i}", smsMessage),
//             new MySqlParameter($"@UrlencodeSmsMessage{i}", request.UrlencodeSmsMessage),
//             new MySqlParameter($"@SmsMessageLength{i}", SmsMessageLength),
//             new MySqlParameter($"@SmsCount{i}", request.SMSCount),
//             new MySqlParameter($"@SmppSmscId{i}", currentGatewayID),
//             new MySqlParameter($"@SmppSmscIdName{i}", currentGateway),
//             new MySqlParameter($"@Mclass{i}", request.Mclass),
//             new MySqlParameter($"@Coding{i}", request.Coding),
//             new MySqlParameter($"@Charset{i}", request.Charset),
//             new MySqlParameter($"@DndCount{i}", request.DNDCount),
//             new MySqlParameter($"@SmartSmsNo{i}", request.SmartSMSNo),
//             new MySqlParameter($"@RefundCount{i}", request.RefundCount),
//             new MySqlParameter($"@BlockCount{i}", request.BlockCount),
//             new MySqlParameter($"@Pdate{i}", request.PDate),
//             new MySqlParameter($"@messageID{i}", messageID),
//             new MySqlParameter($"@MessageStatus{i}", request.MessageStatus),
//             new MySqlParameter($"@Jdate{i}", request.JDate),
//             new MySqlParameter($"@MetaData{i}", request.MetaData)
//         });
//     }

//     insertQuery.Append(string.Join(",", valueStrings));
//     insertQuery.Append(";");

//     var result = await _context.Database.ExecuteSqlRawAsync(insertQuery.ToString(), parameters.ToArray());

//     if (result <= 0)
//     {
//         _logger.LogError($"Error inserting SMS data for batch.");
//         throw new Exception("Error inserting SMS data for batch.");
//     }
// }


// private async Task BatchSendSmsKannelAsync(List<Dictionary<string, string>> mobileNumbersWithData, ProcessJobRequest request, string smsMessage, string currentGateway, string currentGatewayID, string mobileColumnHeader)
// {
//     var batchSize = 1000;
//     var batches = mobileNumbersWithData.Select((x, i) => new { Index = i, Value = x })
//                                       .GroupBy(x => x.Index / batchSize)
//                                       .Select(x => x.Select(v => v.Value).ToList())
//                                       .ToList();

//     var tasks = new List<Task>();

//     foreach (var batch in batches)
//     {
//         var batchTasks = batch.Select(data => SendSmsKannel(request, data[mobileColumnHeader], smsMessage, currentGateway, currentGatewayID));
//         tasks.AddRange(batchTasks);
//     }

//     await Task.WhenAll(tasks);
// }

// private async Task SendSmsKannel(ProcessJobRequest request, string mobileNumber, string smsMessage, string currentGateway, string currentGatewayID)
// {
//     // Prepare the SMS payload
//     var smsPayload = new
//     {
//         mobile = mobileNumber,
//         sender = request.SenderName,
//         smsmsg = smsMessage,
//         smsc = currentGateway,
//         message_type = request.MessageType?.ToUpper(),
//         coding = request.Coding,
//         meta_data = request.MetaData,
//         msg_id = _SmsService.GenerateMessageID(request.LoggedUserId),
//         job_id = request.JobId,
//         user_id = request.LoggedUserId
//     };

//     string jsonPayload = JsonConvert.SerializeObject(smsPayload);
//     StringContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

//     var dlrUrl = SmsConfig.DLR_URL
//         .Replace(SmsConfig.JOB_PH, request.JobId.ToString())
//         .Replace(SmsConfig.USER_PH, request.LoggedUserId.ToString());

//     var dltInfo = string.Format(SmsConfig.TLV_INFO, request.Peid, request.Tid, request.HashId, request.JobId, request.LoggedUserId);

//     var smsData = new Dictionary<string, string>
//     {
//         { "username", SmsConfig.SMS_USER },
//         { "password", SmsConfig.SMS_PSWD },
//         { "from", request.SenderName?.ToString() ?? string.Empty },
//         { "text", request.Data?.SmsMessage ?? string.Empty },
//         { "account", request.LoggedUserId.ToString() },
//         { "dlr-mask", SmsConfig.DLR_MASK.ToString()},
//         { "dlr-url", dlrUrl},
//         { "coding", request.Coding?.ToString() ?? string.Empty },
//         { "meta-data", request.MetaData },
//     };

//     var smsParam = "?" + await new FormUrlEncodedContent(smsData).ReadAsStringAsync();
//     var url = SmsConfig.SMS_CGI + smsParam + "&to=" + Uri.EscapeDataString(mobileNumber) + "&smsc=" + Uri.EscapeDataString(currentGateway);
//     url = url.Replace(Uri.EscapeDataString("${msg_id}"), _SmsService.GenerateMessageID(request.LoggedUserId)?.ToString() ?? string.Empty);
//     url = Regex.Replace(url, @"text=[^&]*", "text=" + HttpUtility.UrlEncode(smsMessage));

//     // Add the URL to the queue
//     _smsQueueProcessor.EnqueueUrl(url);

//     HttpClient httpClient = new HttpClient();
//     var response = await httpClient.GetAsync(url);
//     string responseText = await response.Content.ReadAsStringAsync();

//     if (!response.IsSuccessStatusCode)
//     {
//         _logger.LogError($"Failed to send SMS to {mobileNumber}. Response: {responseText}");
//     }
// }

// private List<Dictionary<string, string>> ReadExcelFile(string filePath)
// {
//     var data = new List<Dictionary<string, string>>();

//     using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
//     {
//         IWorkbook workbook;

//         // Determine the file type based on the extension
//         if (Path.GetExtension(filePath).ToLower() == ".xls")
//         {
//             workbook = new HSSFWorkbook(stream); // For .xls files
//         }
//         else
//         {
//             workbook = new XSSFWorkbook(stream); // For .xlsx files
//         }

//         var sheet = workbook.GetSheetAt(0); // Get the first sheet
//         var headerRow = sheet.GetRow(0); // Assume the first row contains headers

//         // Iterate through rows (skip the header row)
//         for (int i = 1; i <= sheet.LastRowNum; i++)
//         {
//             var row = sheet.GetRow(i);
//             if (row == null) continue; // Skip empty rows

//             var rowData = new Dictionary<string, string>();

//             // Iterate through cells in the row
//             for (int j = 0; j < headerRow.LastCellNum; j++)
//             {
//                 var header = headerRow.GetCell(j)?.ToString() ?? $"Column{j + 1}"; // Use column index if header is missing
//                 var cellValue = row.GetCell(j)?.ToString() ?? string.Empty; // Use empty string if cell is empty
//                 rowData[header] = cellValue;
//             }

//             data.Add(rowData);
//         }
//     }

//     return data;
// }

// [HttpPost("addUserSMS")]
// public async Task<IActionResult> AddUserSMS([FromBody] ProcessJobRequest request)
// {
//     _logger.LogInformation("Incoming request: {@Request}", request);
//     _logger.LogInformation("Incoming request JSON: {RequestJson}", JsonConvert.SerializeObject(request));
//     Console.WriteLine("Incoming request: " + JsonConvert.SerializeObject(request));
//     _logger.LogInformation("AddUserSMS method started.");

//     var updateStatus = await _userStatusService.UpdateJobStatus(request.JobId, JobStatus.PROCESSING);

//     if (!updateStatus)
//     {
//         _logger.LogError("Failed to update job status to PROCESSING for JobId: {JobId}", request.JobId);
//         return StatusCode(500, "Failed to update job status to PROCESSING.");
//     }

//     try
//     {
//         string File = request.FilePath + request.ContactFilename;
//         string basePath = "/var/www/html/test-panel/";
//         string filePath = Path.Combine(basePath, File);

//         _logger.LogInformation("Filepath: {FilePath}", filePath);

//         if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
//         {
//             throw new FileNotFoundException($"File path is invalid or file does not exist: {filePath}");
//         }

//         var mobileNumbersWithData = ReadExcelFile(filePath);

//         if (mobileNumbersWithData == null || !mobileNumbersWithData.Any())
//         {
//             _logger.LogError("No data found in the Excel file.");
//             return BadRequest("No data found in the file.");
//         }

//         string mobileColumnHeader = mobileNumbersWithData.First().Keys.First();

//         if (!mobileNumbersWithData.First().ContainsKey(mobileColumnHeader))
//         {
//             _logger.LogError($"Column '{mobileColumnHeader}' not found in the Excel file.");
//             return BadRequest($"Column '{mobileColumnHeader}' not found.");
//         }

//         string[] Gateway = request.SMPPSMSCIDName.Split(',');
//         string[] GatewayIDs = request.SMPPSMSCID.Split(',');

//         var originalSmsMessage = request.SmsMessage;

//         // Run database insertion and SMS sending in parallel
//         var databaseInsertTask = BatchInsertSmsDataAsync(mobileNumbersWithData, request, mobileColumnHeader, originalSmsMessage, originalSmsMessage.Length, Gateway[0], GatewayIDs[0], _SmsService.GenerateMessageID(request.LoggedUserId));
//         var smsSendingTask = BatchSendSmsKannelAsync(mobileNumbersWithData, request, originalSmsMessage, Gateway[0], GatewayIDs[0], mobileColumnHeader);

//         // Wait for both tasks to complete
//         await Task.WhenAll(databaseInsertTask, smsSendingTask);

//         await _userStatusService.UpdateJobStatus(request.JobId, JobStatus.COMPLETED);
//         _logger.LogInformation("SMS sent and added to the database successfully for all mobile numbers.");
//         return Ok("SMS sent and added to the database successfully for all mobile numbers.");
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError("An error occurred: {Message}", ex.Message);
//         await _userStatusService.UpdateJobStatusRemarkAsync(JobStatus.FAILED, "An error occurred during processing.", request.JobId);
//         return StatusCode(500, "An error occurred during processing.");
//     }
// }
}
}