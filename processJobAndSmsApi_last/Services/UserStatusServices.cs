using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using processJobAndSmsApi.Data;
using processJobAndSmsApi.Models;
using Serilog;

public class UserStatusService(ApplicationDbContext context, ILogger<UserStatusService> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<UserStatusService> _logger = logger;

    public async Task<bool> UpdateJobStatus(string jobId, string status)
    {
        try
        {
            var result = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE users_job SET job_status = @Status WHERE job_id = @JobId",
                new MySqlParameter("@Status", status),
                new MySqlParameter("@JobId", jobId)
            );

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job status for JobId: {JobId}", jobId);
            return false;
        }
    }
    public async Task<bool> UpdateJobSmsSent(string jobId, int PostSMSCount)
    {
        try
        {
            var result = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE users_job SET job_sms_sent = @Number WHERE job_id = @JobId",
                new MySqlParameter("@Number", PostSMSCount), 
                new MySqlParameter("@JobId", jobId)
            );

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job status for JobId: {JobId}", jobId);
            return false;
        }
    }
    public async Task<bool> UpdateJobStatusRemarkAsync( string jobStatus, string remarks,string jobId)
    {
        try
        {
            _logger.LogInformation("Updating job status for JobStatus: {JobStatus}, Remarks: {Remarks} ,JobId: {JobId}", jobStatus, remarks, jobId  );

            var sqlQuery = "UPDATE users_job SET job_status = @JobStatus, remarks = @Remarks WHERE job_id = @JobId";
            var result = await _context.Database.ExecuteSqlRawAsync(
                sqlQuery,
                new MySqlParameter("@JobStatus", jobStatus),
                new MySqlParameter("@Remarks", remarks),
                new MySqlParameter("@JobId", jobId)
            );

            if (result > 0)
            {
                _logger.LogInformation("Successfully updated job status for JobId: {JobId}", jobId);
                return true;
            }
            else
            {
                _logger.LogWarning("No rows updated for JobId: {JobId}. It may not exist.", jobId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job status and remarks for JobId: {JobId}", jobId);
            return false;
        }
    }

 
}
