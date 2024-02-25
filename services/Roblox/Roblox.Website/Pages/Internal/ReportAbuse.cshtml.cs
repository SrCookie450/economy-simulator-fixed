using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Models.AbuseReport;
using Roblox.Services;
using ServiceProvider = Roblox.Services.ServiceProvider;

namespace Roblox.Website.Pages.Internal;

public class ReportAbuse : RobloxPageModel
{
    public string? failureMessage { get; set; }
    public string? successMessage { get; set; }
    
    [BindProperty]
    public AbuseReportReason reportReason { get; set; }
    [BindProperty]
    public string? reportMessage { get; set; }
    
    public void OnGet()
    {
        
    }

    private readonly Regex _alphaNumericRegex = new("[a-zA-Z]+", RegexOptions.Compiled);

    public async Task OnPost()
    {
        if (userSession == null)
        {
            failureMessage = "Not logged in.";
            return;
        }
        
        if (!Enum.IsDefined(reportReason))
        {
            failureMessage = "Invalid report reason.";
            return;
        }

        if (string.IsNullOrWhiteSpace(reportMessage))
        {
            failureMessage = "Report message be at least 10 characters. Please try again.";
            return;
        }

        // check that it is at least 10 alpha characters
        var reportLen = string.Join("",
            _alphaNumericRegex.Match(reportMessage).Groups.Values.Select(c => c.Value).ToArray());
        if (reportLen.Length > 10)
        {
            failureMessage = "Report message be at least 10 characters. Please try again.";
            return;
        }

        using var ar = ServiceProvider.GetOrCreate<AbuseReportService>();
        await ar.InsertReport(userSession.userId, reportReason, reportMessage);
        successMessage = "Your report has been sent successfully.";
        
        reportReason = AbuseReportReason.None;
        reportMessage = null;
    }
}