 
using Microsoft.EntityFrameworkCore; 
using MySqlConnector;
using processJobAndSmsApi.Data; 
using processJobAndSmsApi.Constants;

public class NumberService(ApplicationDbContext context, ILogger<NumberService> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<NumberService> _logger = logger;

    // public async Task<bool> UpdateJobStatus(string jobId, string status)
    // {
    //     try
    //     {
    //         var result = await _context.Database.ExecuteSqlRawAsync(
    //             "UPDATE users_job SET job_status = @Status WHERE job_id = @JobId",
    //             new MySqlParameter("@Status", status),
    //             new MySqlParameter("@JobId", jobId)
    //         );

    //         return result > 0;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error updating job status for JobId: {JobId}", jobId);
    //         return false;
    //     }
    // }

    public async Task<List<string>> FetchNumbers(string filePath, int[]? usersGroupIds)
    {
        //  _logger.LogInformation("Retrieved jobId from API: {jobId}", usersGroupIds);
        var numbers = new List<string>();
 
        if (!string.IsNullOrEmpty(filePath))
        {
            numbers.AddRange(ProcessFileNumbers(filePath));
            // _logger.LogInformation("Processed numbers from file: {FilePath}", filePath);
        }
  
        if (usersGroupIds != null && usersGroupIds.Any())
        {
            var groupNumbers = await FetchNumbersAsync(usersGroupIds);
            numbers.AddRange(groupNumbers);
            // _logger.LogInformation("Fetched numbers for usersGroupIds: {UsersGroupIds}", usersGroupIds);
        }

        return numbers.Distinct().ToList();
    }

    private List<string> ProcessFileNumbers(string filePath)
    {
        var numbers = new List<string>();
        var fileType = Path.GetExtension(filePath).ToLower();

        if (fileType == ".csv" || fileType == ".txt")
        { 
            numbers = ReadFileFormat(filePath);
        }
        else if (fileType == ".xls" || fileType == ".xlsx")
        { 
            var csvFilePath = Path.ChangeExtension(filePath, ".csv");

            if (File.Exists(csvFilePath))
            { 
                numbers = ReadFileFormat(csvFilePath);
            }
            else
            {
                _logger.LogWarning("CSV file not found for Excel file: {FilePath}", filePath);
            }
        }
        else
        {
            _logger.LogWarning("Unsupported file type: {FilePath}", filePath);
        }

        var plainTextNumbers = string.Join(", ", numbers);
        // _logger.LogInformation("Processed numbers ajay : {Numbers}", plainTextNumbers);
        return numbers;
    }

    private List<string> ReadFileFormat(string filePath)
    {
        var numbers = new List<string>();

        if (File.Exists(filePath))
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (IsValidMobileNumber(line))
                {
                    numbers.Add(line.Trim('"'));
                }
            }
        }
        else
        {
            _logger.LogWarning("File not found: {FilePath}", filePath);
        }

        return numbers;
    }

    private static bool IsValidMobileNumber(string number)
    {
        return !string.IsNullOrEmpty(number); 
    }

    public async Task<List<string>> FetchNumbersAsync(int[] usersGroupIds)
    {
        var results = new List<string>();

        foreach (var groupId in usersGroupIds)
        {
            string sqlQuery = "SELECT DISTINCT Mobile FROM users_contact WHERE group_id = @GroupId AND Status = 'Active' ORDER BY Id ASC"; 
            var contacts = await _context.UsersContact
                .FromSqlRaw(sqlQuery, new MySqlParameter("@GroupId", groupId))
                .Select(c => c.Mobile)
                .Where(m => m != null)  
                .Cast<string>()
                .ToListAsync();

            results.AddRange(contacts);
        }

        return results.Distinct().ToList();
    }


    // public async Task<bool> GetNumberManagementStatusByNumberAsync(int userId, string number)
    // { 
    //     string sqlQuery = "SELECT * FROM number_management WHERE number = @Number AND user_id = @UserId";

    //     var result = await _context.NumberManagement
    //         .FromSqlRaw(sqlQuery, 
    //             new MySqlParameter("@Number", number), 
    //             new MySqlParameter("@UserId", userId))
    //         .FirstOrDefaultAsync();

    //     return result != null;
    // }



// ================= ORIGINAL CODE =================

    // public async Task<string> GetNumberManagementStatusByNumberAsync(string userId, string number)
    // {
    //     string sqlQuery = "SELECT status FROM number_management WHERE number = @Number AND user_id = @UserId";
        
    //     var result = await _context.NumberManagement
    //         .FromSqlRaw(sqlQuery, 
    //             new MySqlParameter("@Number", number), 
    //             new MySqlParameter("@UserId", userId))
    //         .Select(n => n.Status)
    //         .FirstOrDefaultAsync();  

    //     return result ?? UserStatus.NULLDATA;
    // }
// =================================================

public async Task<Dictionary<string, List<string>>> GetNumberManagementStatusByNumbersAsync(string userId, List<string> numbers)
{
    // Fetch statuses for all numbers in a single query
    var statuses = await _context.NumberManagement
        .Where(n => numbers.Contains(n.Number) && n.UserId == userId)
        .Select(n => new { n.Number, n.Status })
        .ToListAsync();

    // Group numbers by status
    var groupedNumbers = statuses
        .GroupBy(n => n.Status) // Group by status
        .ToDictionary(g => g.Key, g => g.Select(n => n.Number).ToList()); // Convert to dictionary

    return groupedNumbers;
}

// public async Task<Dictionary<string, string>> GetNumbersManagementStatusAsync(string userId, List<string> numbers)
// {
//     string sqlQuery = @"SELECT id, user_id, number, status 
//     FROM number_management 
//     WHERE user_id = @UserId AND number IN ({0})";

//     var parameters = new List<MySqlParameter>
//     {
//         new MySqlParameter("@UserId", userId)
//     };

//     var numberParams = new List<string>();
//     for (int i = 0; i < numbers.Count; i++)
//     {
//         string paramName = "@Number" + i;
//         numberParams.Add(paramName);
//         parameters.Add(new MySqlParameter(paramName, numbers[i]));
//     }

//     sqlQuery = string.Format(sqlQuery, string.Join(",", numberParams));

//     var results = await _context.NumberManagement
//     .FromSqlRaw(sqlQuery, parameters.ToArray())
//     .Select(n => new { n.Number, n.Status })  // Ensure you're selecting only what's needed
//     .ToDictionaryAsync(n => n.Number, n => n.Status);

//     return results;
// }




// ----------------------------------------------------OPTION 1----------------------------------------------------
// public async Task<Dictionary<string, string>> GetNumberManagementStatusByNumberAsync(string userId, List<string> numbers)
// {
//     if (numbers == null || numbers.Count == 0)
//         return new Dictionary<string, string>();

//     // Create a parameterized SQL query
//     string sqlQuery = "SELECT id, number, status, user_id FROM number_management WHERE user_id = @UserId AND number IN ({0})";

//     // Create parameters for numbers dynamically
//     var numberParams = numbers.Select((n, i) => new MySqlParameter($"@Number{i}", n)).ToList();
//     string numberPlaceholders = string.Join(", ", numberParams.Select(p => p.ParameterName));

//     // Final SQL query with dynamic placeholders
//     sqlQuery = string.Format(sqlQuery, numberPlaceholders);

//     var parameters = new List<MySqlParameter> { new MySqlParameter("@UseId", userId) };
//     parameters.AddRange(numberParams);

//     var results = await _context.NumberManagement
//         .FromSqlRaw(sqlQuery, parameters.ToArray())
//         .AsNoTracking()
//         .ToListAsync();

//     return results.ToDictionary(n => n.Number, n => n.Status);
// }





// public async Task<Dictionary<string, string>> GetNumberManagementStatusByNumberAsync(string userId)
// {
//     if (string.IsNullOrEmpty(userId))
//         return new Dictionary<string, string>();

//     // Create a parameterized SQL query to fetch all numbers and statuses for the given userId
//     string sqlQuery = "SELECT id, number, status, user_id FROM number_management WHERE user_id = @UserId";

//     // Create parameters for the userId
//     var parameters = new List<MySqlParameter> { new MySqlParameter("@UserId", userId) };

//     // Execute the SQL query
//     var results = await _context.NumberManagement
//         .FromSqlRaw(sqlQuery, parameters.ToArray())
//         .AsNoTracking()
//         .ToListAsync();

//     // Return a dictionary mapping numbers to their statuses
//     return results.ToDictionary(n => n.Number, n => n.Status);
// }




}
