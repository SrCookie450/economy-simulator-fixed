namespace Roblox.Website.WebsiteServices;

public class WebsiteService
{
    protected HttpContext httpContext { get; set; }
    
    public WebsiteService(HttpContext ctx)
    {
        this.httpContext = ctx;
    }
}