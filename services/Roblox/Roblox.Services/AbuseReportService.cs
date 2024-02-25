using Dapper;
using Roblox.Dto.AbuseReport;
using Roblox.Models.AbuseReport;
using Roblox.Services.App.FeatureFlags;

namespace Roblox.Services;

public class AbuseReportService : ServiceBase, IService
{
    public async Task<IEnumerable<AbuseReportEntry>> GetReports(AbuseReportStatus status)
    {
        return await db.QueryAsync<AbuseReportEntry>(
            "SELECT id, user_id as userId, report_reason as reportReason, report_status as reportStatus, created_at as createdAt, updated_at as updatedAt, report_message as reportMessage FROM abuse_report WHERE report_status = :s ORDER BY created_at LIMIT :limit OFFSET :offset",
            new
            {
                s = status,
                limit = 100,
                offset = 0,
            });
    }
    
    public async Task<AbuseReportEntry> GetReportById(string reportId)
    {
        return await db.QuerySingleOrDefaultAsync<AbuseReportEntry>(
            "SELECT id, user_id as userId, report_reason as reportReason, report_status as reportStatus, created_at as createdAt, updated_at as updatedAt, report_message as reportMessage FROM abuse_report WHERE id = :id LIMIT 1",
            new
            {
                id = reportId,
            });
    }
    
    public async Task SetReportStatus(string reportId, AbuseReportStatus newStatus, long contextUserId)
    {
        await db.ExecuteAsync(
            "UPDATE abuse_report SET report_status = :new_status, updated_at = now(), author_id = :user_id WHERE id = :id",
            new
            {
                user_id = contextUserId,
                id = reportId,
                new_status = newStatus,
            });
    }
    
    public async Task InsertReport(long contextUserId, AbuseReportReason reason, string message)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.AbuseReportsEnabled);
        await db.ExecuteAsync(
            "INSERT INTO abuse_report (id, user_id, report_reason, report_status, report_message) VALUES (:id, :user_id, :reason, :status, :message)",
            new
            {
                id = Guid.NewGuid().ToString(),
                user_id = contextUserId,
                reason,
                status = AbuseReportStatus.Pending,
                message = message,
            });
    }
    
    public bool IsThreadSafe()
    {
        return true;
    }

    public bool IsReusable()
    {
        return false;
    }
}