using processJobAndSmsApi.Data;
using processJobAndSmsApi.Models;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace processJobAndSmsApi.Services
{
    public class SmartLinkService
    {
        private readonly ApplicationDbContext _context;
        private readonly SmsService _smsService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmartLinkService> _logger;
        private readonly IConfiguration _configuration;

        public string GenerateSmartLinkUrl(string shortCode)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "imns.in";
            return $"{baseUrl}/u/{shortCode}";
        }


        public SmartLinkService(ApplicationDbContext context, SmsService smsService, 
            IHttpClientFactory httpClientFactory, ILogger<SmartLinkService> logger)
        {
            _context = context;
            _smsService = smsService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<UsersSmsSmartUrl?> GetSmartUrlDetailsByShortCode(string shortCode)
        {
            return await _context.UsersSmsSmartUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }

        public async Task<bool> ProcessSmartLinkHit(string shortCode, string userAgent, HttpContext httpContext)
        {
            try
            {
                var smartUrlDetails = await GetSmartUrlDetailsByShortCode(shortCode);
                if (smartUrlDetails == null) return false;

                // Get device info (you'll need to implement or use a device detection library)
                var deviceInfo = DetectDevice(userAgent);

                var now = DateTime.Now;
                var hit = new SmartLinkHit
                {
                    UserId = smartUrlDetails.UserId,
                    JobId = smartUrlDetails.JobId,
                    MobileNumber = smartUrlDetails.MobileNumber,
                    UserAgent = userAgent,
                    Device = deviceInfo.Device,
                    Browser = deviceInfo.Browser,
                    Version = deviceInfo.Version,
                    LongUrl = smartUrlDetails.LongUrl,
                    ShortCode = shortCode,
                    ShortUrl = smartUrlDetails.ShortUrl,
                    CreatedDate = now.ToString("j F Y, g:i A"),
                    Pdate = now.ToString("dd/MM/yyyy"),
                    Jdate = _smsService.JulianDate(DateTimeOffset.Now.ToUnixTimeSeconds()),
                    Ptime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    IpAddress = SmsService.GetClientIp(httpContext)
                };

                _context.SmartLinkHits.Add(hit);

                // Update hits count
                smartUrlDetails.Hits++;
                _context.UsersSmsSmartUrls.Update(smartUrlDetails);

                await _context.SaveChangesAsync();

                // Handle group addition if needed
                if (smartUrlDetails.AddToGroup == "Yes")
                {
                    // await AddToGroup(smartUrlDetails);
                }

                // Handle notification if needed
                if (smartUrlDetails.SendNotification == "Yes" && !string.IsNullOrEmpty(smartUrlDetails.SmsMessage))
                {
                    await SendNotification(smartUrlDetails, hit);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing smart link hit");
                return false;
            }
        }

    // private async Task AddToGroup(UsersSmsSmartUrl smartUrl)
    // {
    //     var existingContact = await _context.UsersContact
    //         .FirstOrDefaultAsync(c => c.Mobile == smartUrl.MobileNumber && 
    //                                 // c.UserId.ToString() == smartUrl.UserId && 
    //                                 // c.GroupId.ToString() == smartUrl.GroupId);
    //         )

    //     if (existingContact == null)
    //     {
    //         var contact = new UsersContact
    //         {
    //             UserId = smartUrl.UserId,  // Directly use string value
    //             GroupId = int.Parse(smartUrl.GroupId), // Directly use string value
    //             Name = "",  // Empty string as in PHP code
    //             Mobile = smartUrl.MobileNumber,
    //             Status = "Active"
    //         };
    //         _context.UsersContact.Add(contact);
    //         await _context.SaveChangesAsync();
    //     }
    // }


private async Task SendNotification(UsersSmsSmartUrl smartUrl, SmartLinkHit hit)
{
    try
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == smartUrl.UserId);
        
        if (user == null) return;

        var routeType = await _context.UsersRouteType
            .FirstOrDefaultAsync(r => r.UserId == user.Id && r.RouteTypeId != null);            
        if (routeType == null) return;

        var message = smartUrl.SmsMessage;
        message = message.Replace("{long_url}", smartUrl.LongUrl)
                        .Replace("{short_url}", smartUrl.ShortUrl)
                        .Replace("{device}", hit.Device)
                        .Replace("{browser}", hit.Browser)
                        .Replace("{ip}", hit.IpAddress)
                        .Replace("{datetime}", hit.CreatedDate);

        var smsJob = new UsersSMS
        {
            user_id = user.Id,
            Number = smartUrl.MobileNumber,
            sms_message = message,
            Status = "Pending",
            Smart = "Yes",
            job_id = smartUrl.JobId,
            Jdate = _smsService.JulianDate(DateTimeOffset.Now.ToUnixTimeSeconds()),
            SentAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
        };

        _context.UsersSMS.Add(smsJob);
        await _context.SaveChangesAsync();

        await _smsService.ProcessSingleSms(smsJob);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending smart link notification");
    }
}

private (string Device, string Browser, string Version) DetectDevice(string userAgent)
{
    try
    {
        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(userAgent);

        var device = clientInfo.Device.Family;
        if (string.IsNullOrEmpty(device) || device == "Other")
        {
            device = clientInfo.OS.Family;
        }

        return (
            Device: device,
            Browser: clientInfo.UA.Family,
            Version: $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}"
        );
    }
    catch
    {
        return ("Unknown", "Unknown", "Unknown");
    }
}
    }
}