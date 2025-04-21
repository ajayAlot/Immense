using Microsoft.AspNetCore.Mvc;
using processJobAndSmsApi.Services;

namespace processJobAndSmsApi.Controllers
{
    [ApiController]
    [Route("")]
    public class SmartLinkController : ControllerBase
    {
        private readonly SmartLinkService _smartLinkService;
        private readonly ILogger<SmartLinkController> _logger;
        private readonly IConfiguration _configuration;

        public SmartLinkController(SmartLinkService smartLinkService, ILogger<SmartLinkController> logger, IConfiguration configuration)
        {
            _smartLinkService = smartLinkService;
            _logger = logger;
            _configuration = configuration;
        }


        [HttpGet("u/{shortCode}")]
        public async Task<IActionResult> ProcessHit(string shortCode)
        {
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var result = await _smartLinkService.ProcessSmartLinkHit(shortCode, userAgent, HttpContext);

                if (!result)
                {
                    return Redirect("/smart-link-expired.php");
                }

                var smartUrlDetails = await _smartLinkService.GetSmartUrlDetailsByShortCode(shortCode);
                if (smartUrlDetails?.LongUrl == null)
                {
                    return Redirect("/smart-link-expired.php");
                }

                // Decode URL like in PHP
                var decodedUrl = Uri.UnescapeDataString(smartUrlDetails.LongUrl);
                return Redirect(decodedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing smart link hit");
                return Redirect("/smart-link-expired.php");
            }
        }
    }
}