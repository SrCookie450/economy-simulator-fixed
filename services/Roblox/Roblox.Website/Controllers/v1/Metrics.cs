using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/metrics/v1")]
public class MetricsControllerV1
{
    [HttpPost("performance/measurements")]
    [HttpPost("performance/send-measurement")]
    public void ReportMeasurements()
    {
        
    }

    [HttpGet("thumbnails/metadata")]
    public dynamic GetThumbnailsMetadata()
    {
        return new
        {
            logRatio = 0,
        };
    }

    [HttpPost("thumbnails/load")]
    public void ReportThumbnailsLoad()
    {
        return;
    }
}