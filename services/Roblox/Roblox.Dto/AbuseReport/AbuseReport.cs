using Roblox.Models.AbuseReport;

namespace Roblox.Dto.AbuseReport;

public class AbuseReportEntry
{
    public string id { get; set; }
    public long userId { get; set; }
    public long authorId { get; set; }
    public AbuseReportReason reportReason { get; set; }
    public AbuseReportStatus reportStatus { get; set; }
    public string reportMessage { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}