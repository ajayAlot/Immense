using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using processJobAndSmsApi.Data;
using System.Data;
using System.Threading.Tasks;

public class BalanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BalanceService> _logger;

    public BalanceService(ApplicationDbContext context, ILogger<BalanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> GetUsersSMSBalanceByRouteID(string userId, string routeTypeId)
    {
        return await _context.UsersRouteType
            .Where(x => x.UserId == userId && x.RouteTypeId == routeTypeId)
            .Select(x => (int?)x.SmsBalance) // Cast to Nullable<int>
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateSMSBalanceAsync(string debitBalance, string loggedUserId, string routeTypeId)
    { 
        var sqlQueryr = "UPDATE users_route_type SET sms_balance = sms_balance - @DebitBalance WHERE user_id = @LoggedUserID AND route_type_id = @RouteTypeId";
        var sqlQueryt = "UPDATE users_transaction SET sms_balance_qty = sms_balance_qty - @DebitBalance WHERE user_id = @LoggedUserID AND route_type_id = @RouteTypeId";
 
        var debitBalanceParam = new MySqlParameter("@DebitBalance", debitBalance);
        var loggedUserIdParam = new MySqlParameter("@LoggedUserID", loggedUserId);
        var routeTypeIdParam = new MySqlParameter("@RouteTypeId", routeTypeId);

        
        var affectedRowsRouteType = await _context.Database.ExecuteSqlRawAsync(sqlQueryr, debitBalanceParam, loggedUserIdParam, routeTypeIdParam);
        if (affectedRowsRouteType == 0)
        {
            _logger.LogWarning("No rows updated in users_route_type for userId: {UserId}, routeTypeId: {RouteTypeId}", loggedUserId, routeTypeId);
            return false; // No rows updated, user or route type doesn't exist
        }
 
        var affectedRowsTransaction = await _context.Database.ExecuteSqlRawAsync(sqlQueryt, debitBalanceParam, loggedUserIdParam, routeTypeIdParam);
        if (affectedRowsTransaction == 0)
        {
            _logger.LogWarning("No rows updated in users_transaction for userId: {UserId}, routeTypeId: {RouteTypeId}", loggedUserId, routeTypeId);
            return false; // No rows updated, user or route type doesn't exist
        }

        return true; 
    }
}
