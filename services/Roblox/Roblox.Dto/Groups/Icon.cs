namespace Roblox.Dto.Groups;

public class GroupIcon : IAsyncDisposable
{
    public Stream? finalIconStream { get; set; }

    public GroupIcon(Stream safeStream)
    {
        this.finalIconStream = safeStream;
    }

    public async ValueTask DisposeAsync()
    {
        if (finalIconStream != null)
            await finalIconStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}