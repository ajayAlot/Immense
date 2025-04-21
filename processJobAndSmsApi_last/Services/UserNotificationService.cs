using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using processJobAndSmsApi.Data;
using processJobAndSmsApi.Utils;
using Serilog; // Add this namespace for Serilog

public class UserNotificationService
{
    private readonly ApplicationDbContext _context;

    public UserNotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task JobNotificationAsync(string userId, string userFullName, string jobId, int postSMSCount, string routeTypeId, int totalBalanceSMS)
    {
        Log.Information("Starting JobNotificationAsync for UserId: {UserId}, JobId: {JobId}, PostSMSCount: {PostSMSCount}", userId, jobId, postSMSCount);

        var createdDate = DateTime.Now;
        var pDate = DateTime.Now.ToString("dd/MM/yyyy");

        int day = DateTime.Now.Day;
        int month = DateTime.Now.Month;
        int year = DateTime.Now.Year;
        var jDate = DateUtils.DateToJulian(day, month, year);
        var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var ipAddress = GetIpAddress(); 

        if (postSMSCount > 0)
        {
            Log.Information("PostSMSCount is greater than 0. Current Count: {PostSMSCount}", postSMSCount);
            if (postSMSCount > 5)
            {
                Log.Information("PostSMSCount is greater than 5. Inserting notice into the database.");

                var noticeQuery = @"
                    INSERT INTO `notice` (`user_id`, `subject`, `notice`, `created_by_id`, `modified_by_id`, 
                    `created_date`, `modified_date`, `pdate`, `jdate`, `ptime`, `ipaddress`, `status`) 
                    VALUES (@UserId, @Subject, @Message, @CreatedById, @ModifiedById, 
                    @CreatedDate, @ModifiedDate, @PDate, @JDate, @PTime, @IpAddress, @Status)";

                await _context.Database.ExecuteSqlRawAsync(noticeQuery,
                    new MySqlParameter("@UserId", userId),
                    new MySqlParameter("@Subject", $"{postSMSCount} - SMS Balance Debited"),
                    new MySqlParameter("@Message", $"Dear {userFullName}, {postSMSCount} SMS Balance has been debited for Bulk SMS Job ID: {jobId}. Remaining SMS Balance: {totalBalanceSMS}"),
                    new MySqlParameter("@CreatedById", userId),
                    new MySqlParameter("@ModifiedById", userId),
                    new MySqlParameter("@CreatedDate", createdDate),
                    new MySqlParameter("@ModifiedDate", createdDate),
                    new MySqlParameter("@PDate", pDate),
                    new MySqlParameter("@JDate", jDate),
                    new MySqlParameter("@PTime", timeStamp),
                    new MySqlParameter("@IpAddress", ipAddress),
                    new MySqlParameter("@Status", "Active")
                );

                Log.Information("Notice inserted successfully.");
            }

            string remark = $"{postSMSCount} SMS Balance has been debited for Bulk SMS Job ID: {jobId}";
            await UsersJobDebitCreditAsync(userId, jobId, routeTypeId, "Debit", "Job", postSMSCount, 0, totalBalanceSMS, remark, "Job");
            Log.Information("UsersJobDebitCreditAsync called for userId: {UserId}, jobId: {JobId}, postSMSCount: {PostSMSCount}", userId, jobId, postSMSCount);
        }

        Log.Information("Exiting JobNotificationAsync for UserId: {UserId}", userId);
    }

    public async Task AdminJobNotificationAsync(string adminUserId, string userId, string userFullName, string jobId, int postSMSCount, string routeTypeId, int adminTotalBalanceSMS)
    {
        Log.Information("Starting AdminJobNotificationAsync for AdminUserId: {AdminUserId}, UserId: {UserId}, JobId: {JobId}, PostSMSCount: {PostSMSCount}", adminUserId, userId, jobId, postSMSCount);
        
        var now = DateTime.Now;
        var createdDate = now;
        var pDate = now.ToString("dd/MM/yyyy");
        var jDate = DateUtils.DateToJulian(now.Day, now.Month, now.Year);
        var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var ipAddress = GetIpAddress();

        if (postSMSCount > 0)
        {
            Log.Information("PostSMSCount is greater than 0 for Admin Notification. Current Count: {PostSMSCount}", postSMSCount);

            // Fetch admin user full name
            var adminUserFullName = await GetUserFullNameAsync(adminUserId);
            Log.Information("Fetched Admin User Full Name: {AdminUserFullName}", adminUserFullName);

            if (postSMSCount > 5)
            {
                Log.Information("PostSMSCount is greater than 5 for Admin. Inserting notice into the database.");

                var noticeQuery = @"
                    INSERT INTO `notice` (`user_id`, `subject`, `notice`, `created_by_id`, `modified_by_id`, 
                    `created_date`, `modified_date`, `pdate`, `jdate`, `ptime`, `ipaddress`, `status`) 
                    VALUES (@AdminUserId, @Subject, @Notice, @CreatedById, @ModifiedById, 
                    @CreatedDate, @ModifiedDate, @PDate, @JDate, @PTime, @IpAddress, @Status)";

                var subject = $"{postSMSCount} - SMS Balance Debited (Overselling)";
                var notice = $"Dear {adminUserFullName}, Your customer ({userFullName}) has performed a bulk SMS job. {postSMSCount} SMS balance has been debited for Bulk SMS Job ID: {jobId}. Remaining SMS balance: {adminTotalBalanceSMS}";

                await _context.Database.ExecuteSqlRawAsync(noticeQuery,
                    new MySqlParameter("@AdminUserId", adminUserId),
                    new MySqlParameter("@Subject", subject),
                    new MySqlParameter("@Notice", notice),
                    new MySqlParameter("@CreatedById", adminUserId),
                    new MySqlParameter("@ModifiedById", adminUserId),
                    new MySqlParameter("@CreatedDate", createdDate),
                    new MySqlParameter("@ModifiedDate", createdDate),
                    new MySqlParameter("@PDate", pDate),
                    new MySqlParameter("@JDate", jDate),
                    new MySqlParameter("@PTime", timeStamp),
                    new MySqlParameter("@IpAddress", ipAddress),
                    new MySqlParameter("@Status", "Active")
                );

                Log.Information("Admin notice inserted successfully.");
            }

            // Log debit/credit transaction
            string remark = $"{postSMSCount} SMS Balance has been debited (Overselling) for Bulk SMS Job ID: {jobId}";
            await UsersJobDebitCreditAsync(adminUserId, jobId, routeTypeId, "Debit", "Job", postSMSCount, 0, adminTotalBalanceSMS, remark, "Overselling Job");
            Log.Information("UsersJobDebitCreditAsync called for AdminUserId: {AdminUserId}, jobId: {JobId}, postSMSCount: {PostSMSCount}", adminUserId, jobId, postSMSCount);
        }

        Log.Information("Exiting AdminJobNotificationAsync for AdminUserId: {AdminUserId}", adminUserId);
    }

    private async Task UsersJobDebitCreditAsync(string userId, string jobId, string routeTypeId, string transactionType, string type, int smsQty, int prevSmsQty, int smsBalanceQty, string remark, string status)
    {
        Log.Information("Starting UsersJobDebitCreditAsync for UserId: {UserId}, JobId: {JobId}, TransactionType: {TransactionType}, SmsQty: {SmsQty}", userId, jobId, transactionType, smsQty);
        
        var now = DateTime.Now;
        var createdDate = now;
        var pDate = now.ToString("dd/MM/yyyy");
        var jDate = DateUtils.DateToJulian(now.Day, now.Month, now.Year);
        var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (prevSmsQty == 0)
        {
            prevSmsQty = transactionType == "Debit" ? smsBalanceQty + smsQty : smsBalanceQty - smsQty;
        }

        var debitCreditQuery = @"
            INSERT INTO `users_job_debit_credit` (`user_id`, `job_id`, `route_type_id`, `transaction_type`, `type`, 
            `sms_qty`, `prev_sms_qty`, `sms_balance_qty`, `remark`, `status`, 
            `created_date`, `pdate`, `jdate`, `ptime`) 
            VALUES (@UserId, @JobId, @RouteTypeId, @TransactionType, @Type, 
            @SmsQty, @PrevSmsQty, @SmsBalanceQty, @Remark, @Status, 
            @CreatedDate, @PDate, @JDate, @PTime)";

        await _context.Database.ExecuteSqlRawAsync(debitCreditQuery,
            new MySqlParameter("@UserId", userId),
            new MySqlParameter("@JobId", jobId),
            new MySqlParameter("@RouteTypeId", routeTypeId),
            new MySqlParameter("@TransactionType", transactionType),
            new MySqlParameter("@Type", type),
            new MySqlParameter("@SmsQty", smsQty),
            new MySqlParameter("@PrevSmsQty", prevSmsQty),
            new MySqlParameter("@SmsBalanceQty", smsBalanceQty),
            new MySqlParameter("@Remark", remark),
            new MySqlParameter("@Status", status),
            new MySqlParameter("@CreatedDate", createdDate),
            new MySqlParameter("@PDate", pDate),
            new MySqlParameter("@JDate", jDate),
            new MySqlParameter("@PTime", timeStamp)
        );

        Log.Information("UsersJobDebitCreditAsync completed for UserId: {UserId}, JobId: {JobId}, SmsQty: {SmsQty}", userId, jobId, smsQty);
    }

    // private static string DateToJulian(DateTime date)
    // {
    //     return date.ToString("yyyyMMdd"); 
    // }
 
    private string GetIpAddress()
    { 
        return "127.0.0.1"; 
    }
    public async Task<string> GetUserFullNameAsync(string userId)
    {
        var fullName = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Fullname)
            .FirstOrDefaultAsync();

        return fullName ?? string.Empty;  
    }

    public async Task AdminCuttingRefundAsync(string adminUsersId, string usersId, string userFullName, string routeTypeId, string jobId, int jobSmartSmsNo, int? smsCount, int postSmsCount)
    {
        var creditBalance = jobSmartSmsNo;
        if (creditBalance > 0)
        { 
            var totalBalanceSms = await GetUsersSmsBalanceByRouteIdAsync(adminUsersId, routeTypeId);
            totalBalanceSms += creditBalance; 

            var updateUsersRouteTypeQuery = "UPDATE users_route_type SET sms_balance = sms_balance + @CreditBalance WHERE user_id = @AdminUsersId AND route_type_id = @RouteTypeId";
            await _context.Database.ExecuteSqlRawAsync(updateUsersRouteTypeQuery, new {creditBalance,adminUsersId,routeTypeId });
 
            var updateUsersTransactionQuery = "UPDATE users_transaction SET sms_balance_qty = sms_balance_qty + @CreditBalance WHERE user_id = @AdminUsersId AND route_type_id = @RouteTypeId";
            await _context.Database.ExecuteSqlRawAsync(updateUsersTransactionQuery, new {creditBalance,adminUsersId,routeTypeId });
 
            var createdDate = DateTime.Now.ToString("dd MMMM yyyy");
            var pdate = DateTime.Now.ToString("dd/MM/yyyy"); 
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year; 
            var jdate = DateUtils.DateToJulian(day, month, year);

            var ptime = DateTimeOffset.Now.ToUnixTimeSeconds();
            // var ipAddress = Ipaddress;
 
            var adminUserFullName = await GetUserFullNameAsync(adminUsersId);

            var insertNoticeQuery = @"INSERT INTO notice (user_id, subject, notice, created_by_id, modified_by_id, created_date, modified_date, pdate, jdate, ptime, ipaddress, status) 
                                    VALUES (@AdminUsersId, @Subject, @Notice, @AdminUsersId, @AdminUsersId, @CreatedDate, @CreatedDate, @PDate, @JDate, @PTime, @IpAddress, 'Active')";
            var subject = $"{creditBalance} - SMS Balance Refunded";
            var notice = $"Dear {adminUserFullName}, Your customer ({userFullName}) performed a bulk SMS job. {creditBalance} SMS Balance has been credited (Smart Cutting - Overselling) for Bulk SMS Job ID: {jobId}. Remaining SMS Balance: {totalBalanceSms}";
            
            await _context.Database.ExecuteSqlRawAsync(insertNoticeQuery, new 
            { 
                AdminUsersId = adminUsersId, 
                Subject = subject, 
                Notice = notice, 
                CreatedDate = createdDate, 
                PDate = pdate, 
                JDate = jdate, 
                PTime = ptime,  
            });
            var remark = $"{creditBalance} SMS Balance has been credited (Smart Cutting - Overselling) for Bulk SMS Job ID: {jobId}";
            await UsersJobDebitCreditAsync(adminUsersId, jobId, routeTypeId, "Credit", "Refund", creditBalance, 0, totalBalanceSms, remark, "Smart Cutting - Overselling");
        }
    }
    public async Task<int> GetUsersSmsBalanceByRouteIdAsync(string usersId, string routeTypeId)
    {
        var query = "SELECT sms_balance FROM users_route_type WHERE user_id = @UsersId AND route_type_id = @RouteTypeId";
        var smsBalance = await _context.Database.ExecuteSqlRawAsync(query, new { UsersId = usersId, RouteTypeId = routeTypeId });
        return smsBalance;
    }
 

}
