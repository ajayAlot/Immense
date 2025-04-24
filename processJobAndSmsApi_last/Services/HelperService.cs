using System;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using processJobAndSmsApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient; // Use this for MySQL
// using System.Data.SqlClient; ← Remove this if using MySQL
using System.Security.Cryptography;
using System.Text;
using processJobAndSmsApi.Models;


namespace processJobAndSmsApi.Services
{
    public class HelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public HelperService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _context = context;
        }

        public int GetLoggedUserId()
        {

            // var session = _httpContextAccessor.HttpContext?.Session;

            // // Get the logged user ID from session
            // var loggedUserId = session?.GetInt32("LoggedUserID");

            // if (loggedUserId == null || loggedUserId <= 0)
            // {
            //     // If not found in session, check if we have a username and get ID from that
            //     var loggedUsername = session?.GetString("logged_by_username");
            //     if (!string.IsNullOrEmpty(loggedUsername))
            //     {
            //         // Call database to get user ID by username
            //         // This would be similar to the PHP $Users->LoggedUserInfo($logged_by_username) call
            //         using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //         {
            //             connection.Open();
            //             var command = new MySqlCommand(
            //                 "SELECT id FROM users WHERE username = @Username AND status = 'Active'",
            //                 connection);
            //             command.Parameters.AddWithValue("@Username", loggedUsername);
            //             var result = command.ExecuteScalar();

            //             if (result != null && result != DBNull.Value)
            //             {
            //                 loggedUserId = Convert.ToInt32(result);
            //                 // Store in session for future use
            //                 session?.SetInt32("LoggedUserID", loggedUserId.Value);
            //             }
            //         }
            //     }

            //     // If still not found, redirect to login
            //     if (loggedUserId == null || loggedUserId <= 0)
            //     {
            //         // In a real implementation, you would redirect to login
            //         // For now, we'll return a default value or throw an exception
            //         throw new UnauthorizedAccessException("User not logged in or session expired");
            //     }
            // }

            // return loggedUserId.Value;
            return 1;
        }

        public async Task<string> GetDefaultSenderId()
        {
            try
            {
                int userId = GetLoggedUserId();
                // Remove unused variable since the method already handles default values

                using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // First try to get user's default sender ID from sender_id table
                    var command = new MySqlCommand(
                        "SELECT senderid FROM sender_id " +
                        "WHERE user_id = @UserId AND is_default = 'Yes' AND status = 'Active' " +
                        "LIMIT 1",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return result?.ToString() ?? "SMSAPI";
                    }

                    // If no default sender ID is set, get the first active sender ID for this user
                    command = new MySqlCommand(
                        "SELECT senderid FROM sender_id " +
                        "WHERE user_id = @UserId AND status = 'Active' " +
                        "ORDER BY id ASC LIMIT 1",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return result?.ToString() ?? "SMSAPI";
                    }

                    // If still no result, check user settings for default sender ID
                    command = new MySqlCommand(
                        "SELECT default_sender_id FROM users_settings " +
                        "WHERE user_id = @UserId",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        // Get the sender ID text from the ID
                        command = new MySqlCommand(
                            "SELECT senderid FROM sender_id " +
                            "WHERE id = @SenderIdNum AND status = 'Active'",
                            connection);
                        command.Parameters.AddWithValue("@SenderIdNum", result.ToString());
                        var senderResult = await command.ExecuteScalarAsync();

                        if (senderResult != null && senderResult != DBNull.Value)
                        {
                            return senderResult?.ToString() ?? "SMSAPI";
                        }
                    }

                    // If all else fails, return a default value
                    return "SMSAPI";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error getting default sender ID: {ex.Message}");

                // Return a fallback value
                return "SMSAPI";
            }
        }

        public string GetDefaultRouteTypeName()
        {
            try
            {
                int userId = GetLoggedUserId();
                string defaultRouteTypeName = "Transactional"; // Default fallback value

                using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    // First try to get user's default route type from user settings
                    var command = new MySqlCommand(
                        "SELECT rt.route_type_name " +
                        "FROM users_settings us " +
                        "JOIN route_type rt ON us.default_route_type_id = rt.route_type_id " +
                        "WHERE us.user_id = @UserId",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return result?.ToString() ?? defaultRouteTypeName;
                    }

                    // If no user setting, get the first available route type for this user
                    command = new MySqlCommand(
                        "SELECT rt.route_type_name " +
                        "FROM users_route_type urt " +
                        "JOIN route_type rt ON urt.route_type_id = rt.route_type_id " +
                        "WHERE urt.user_id = @UserId AND urt.status = 'Active' " +
                        "ORDER BY rt.route_type_id ASC " +
                        "LIMIT 1",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return result?.ToString() ?? defaultRouteTypeName;
                    }

                    // If still no result, get system default route type
                    command = new MySqlCommand(
                        "SELECT rt.route_type_name " +
                        "FROM route_type rt " +
                        "WHERE rt.is_default = 1 OR rt.is_default = 'Yes' " +
                        "LIMIT 1",
                        connection);
                    result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return result?.ToString() ?? defaultRouteTypeName;
                    }
                }

                return defaultRouteTypeName;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error getting default route type: {ex.Message}");

                // Return a fallback value
                return "Transactional";
            }
        }

        public int GetUserGroupsCount()
        {
            try
            {
                int userId = GetLoggedUserId();
                int groupCount = 0;

                using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    // Get count of active groups for the current user
                    var command = new MySqlCommand(
                        "SELECT COUNT(*) FROM users_group " +
                        "WHERE user_id = @UserId AND status = 'Active'",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        groupCount = Convert.ToInt32(result);
                    }
                }

                return groupCount;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error getting user groups count: {ex.Message}");

                // Return a default value in case of error
                return 0;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetUserGroups()
        {
            var userGroups = new List<Dictionary<string, object>>();
            try
            {
                int userId = GetLoggedUserId();

                using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    var command = new MySqlCommand(
                        "SELECT id, name, description, status FROM users_group " +
                        "WHERE user_id = @UserId AND status = 'Active' " +
                        "ORDER BY name ASC",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var group = new Dictionary<string, object>
                    {
                        { "id", reader["id"] },
                        { "name", reader["name"] },
                        { "description", reader["description"] },
                        { "status", reader["status"] }
                    };
                            userGroups.Add(group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user groups: {ex.Message}");
            }

            return userGroups;
        }

        public int GetContactCountByGroupId()
        {
            try
            {
                int userId = GetLoggedUserId();
                int contactCount = 0;

                using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    // Get count of contacts in groups for the current user
                    var command = new MySqlCommand(
                        "SELECT COUNT(*) FROM users_contact " +
                        "WHERE user_id = @UserId AND group_id IN " +
                        "(SELECT id FROM users_group WHERE user_id = @UserId AND status = 'Active')",
                        connection);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        contactCount = Convert.ToInt32(result);
                    }
                }

                return contactCount;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error getting contact count by group: {ex.Message}");

                // Return a default value in case of error
                return 0;
            }
        }

        public string PasswordGenerate()
        {
            string _allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@#$%";
            Random randNum = new Random();
            char[] chars = new char[8];
            for (int i = 0; i < 8; i++)
            {
                chars[i] = _allowedChars[randNum.Next(0, _allowedChars.Length)];
            }
            return new string(chars);
        }

        public string GetLoggedUsername()
        {
            var username = _httpContextAccessor.HttpContext.Session.GetString("username");
            if (username == null)
            {
                return null;
            }
            return username;
        }

        public string GetUsersCategoryIDByTitle(string title)
        {
            var category = _context.UsersCategory.FirstOrDefaultAsync(c => c.Type == title);
            if (category != null)
            {
                return category.Id.ToString();
            }
            return null;
        }

        public string GenerateApiKey(string username)
        {
            // Combine username, current microtime, and a random number
            string input = username + DateTime.Now.Ticks + new Random().Next(1000, 9999);

            // Create MD5 hash
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                // Convert hash to uppercase hex string
                string hashString = BitConverter.ToString(hash).Replace("-", "").ToUpper();

                // Take first 10 characters
                string shortHash = hashString.Substring(0, 10);

                // Split into chunks of 5 and join with dash
                string result = string.Join("-", SplitInParts(shortHash, 5));
                return result;
            }
        }

        private static string[] SplitInParts(string s, int partLength)
        {
            int partCount = (s.Length + partLength - 1) / partLength;
            string[] parts = new string[partCount];
            for (int i = 0; i < partCount; i++)
            {
                int startIndex = i * partLength;
                parts[i] = s.Substring(startIndex, Math.Min(partLength, s.Length - startIndex));
            }
            return parts;
        }

        public string DateToJulian(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (month <= 2)
            {
                year--;
                month += 12;
            }

            int A = year / 100;
            int B = 2 - A + A / 4;

            int julianDay = (int)(365.25 * (year + 4716))
                          + (int)(30.6001 * (month + 1))
                          + day + B - 1524;

            return julianDay.ToString();
        }


        public string GetRandomManagerIDByUsersID(string username)
        {
            using(var connection =new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var query = "SELECT id from manager where username = @username order by rand() LIMIT 0, 1";

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                var result = command.ExecuteScalar();
                if(result !=null && result != DBNull.Value)
                {
                    return result.ToString();
                }
                

            }
            return null;
        }

        public string ToMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Format as hexadecimal
                }
                return sb.ToString();
            }
        }


        public Users GetLoggedUserData()
        {
            var username = GetLoggedUsername();
            if(username == null)
            {
                return null;
            }
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                return user;
            }
            catch (Exception ex) {
                return null;
            }
            
            
            
        }
    }
}
