using System.Text;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using processJobAndSmsApi.Data;

public class UserService(IConfiguration configuration, ILogger<UserService> logger, ApplicationDbContext context)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<UserService> _logger = logger;
    private readonly ApplicationDbContext _context = context;
    private static readonly Random _random = new();
    public string GenerateShortCode()
    {
        int length = 4;  
        string alphabet = "abcdefghijklABCDEFGHIJKLM1234567890NOPQRSTUVWXYZmnopqrstuvwxyz1234567890abcdefghijklABCDEFGHIJKLM1234567890NOPQRSTUVWXYZmnopqrstuvwxyz";
        char[] shortCode = new char[length];
        int alphaLength = alphabet.Length;

        for (int i = 0; i < length; i++)
        {
            int randomIndex = _random.Next(0, alphaLength);
            shortCode[i] = alphabet[randomIndex];
        } 
        return new string([.. shortCode.OrderBy(c => _random.Next())]);
    }
    public string GetSmartMessage(string? smsMessage, string? shortURLPrefix, string? shortURL)
    {
        if (smsMessage == null)
        {
            return "NOT found";
        }
        string smartSmsMessage = smsMessage.Replace(shortURLPrefix + "1XXXX", shortURL);

        for (int i = 2; i <= 9; i++)
        {
            smartSmsMessage = smartSmsMessage.Replace(shortURLPrefix + i + "XXXX", shortURL);
        }

        return smartSmsMessage;
    }

    public async Task<int> AddUserSmartSmsAsync(Dictionary<string, object> resArr)
    {
        _logger.LogInformation("The data from the request is: {data}", resArr);
        // Extracting values from the dictionary, with null-safe handling
        var loggedUserId = resArr["LoggedUserID"]?.ToString();
        var jobId = resArr["job_id"]?.ToString();
        var toNumber = resArr["ToNumber"]?.ToString();
        var smartFilename = resArr["smart_filename"]?.ToString();
        var longUrl = resArr["longURL"]?.ToString();
        var shortCode = resArr["shortCode"]?.ToString();
        var shortUrl = resArr["shortURL"]?.ToString();
        var addToGroup = resArr["add_to_group"]?.ToString();
        var addToGroupId = resArr["add_to_group_id"]?.ToString();
        var sendNotification = resArr["send_notification"]?.ToString();
        var notificationMessage = resArr["notification_message"]?.ToString();

        // Date parsing with safe fallback

        if (!DateTime.TryParse(resArr["pdate"]?.ToString(), out DateTime pDate))
        {
            _logger.LogWarning("Invalid pdate format, setting to current date");
            pDate = DateTime.Now;
        }

        if (!DateTime.TryParse(resArr["jdate"]?.ToString(), out DateTime jDate))
        {
            _logger.LogWarning("Invalid jdate format, setting to current date");
            jDate = DateTime.Now;
        }

        // Extracting pTime as-is (consider converting it if necessary)
        var pTime = resArr["ptime"]?.ToString();
        var ipAddress = resArr["ipaddress"]?.ToString();

        // SQL query string for inserting data into users_sms_smart_urls
        var sqlQuery = @"
            INSERT INTO users_sms_smart_urls 
            (user_id, job_id, mobile_number, filename, long_url, short_code, short_url, add_to_group, group_id, 
            send_notification, sms_message, hits, pdate, jdate, ptime, ipaddress) 
            VALUES 
            (@LoggedUserID, @JobId, @ToNumber, @SmartFilename, @LongURL, @ShortCode, @ShortURL, @AddToGroup, 
            @AddToGroupId, @SendNotification, @NotificationMessage, 0, @PDate, @jdate, @PTime, @IpAddress)";

        // SQL parameters list
        var sqlParams = new List<MySqlParameter>
        {
            new MySqlParameter("@LoggedUserID", loggedUserId),
            new MySqlParameter("@JobId", jobId),
            new MySqlParameter("@ToNumber", toNumber),
            new MySqlParameter("@SmartFilename", smartFilename),
            new MySqlParameter("@LongURL", longUrl),
            new MySqlParameter("@ShortCode", shortCode),
            new MySqlParameter("@ShortURL", shortUrl),
            new MySqlParameter("@AddToGroup", addToGroup),
            new MySqlParameter("@AddToGroupId", addToGroupId),
            new MySqlParameter("@SendNotification", sendNotification),
            new MySqlParameter("@NotificationMessage", notificationMessage),
            new MySqlParameter("@PDate", pDate),
            new MySqlParameter("@jdate", jDate),
            new MySqlParameter("@PTime", pTime),
            new MySqlParameter("@IpAddress", ipAddress)
        };

        try
        {
            // Execute the SQL insert command
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sqlQuery, sqlParams.ToArray());
            _logger.LogInformation("Inserted smart SMS with JobId: {jobId}, Filename: {smartFilename}", jobId, smartFilename);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            // Log the error and return 0
            _logger.LogError(ex, "Error while inserting smart SMS for JobId: {jobId}", jobId);
            return 0;
        }
    }

}