namespace Roblox.Libraries;

public class CombinedAsyncDisposable : IAsyncDisposable
{
    private IEnumerable<IAsyncDisposable> children { get; set; }
    
    public CombinedAsyncDisposable()
    {
        children = new List<IAsyncDisposable>();
    }
    
    public CombinedAsyncDisposable(IEnumerable<IAsyncDisposable> newChildren)
    {
        children = newChildren;
    }

    public CombinedAsyncDisposable AddChild(IAsyncDisposable disposable)
    {
        var newList = children.ToList();
        newList.Add(disposable);
        children = newList;
        return this;
    }
    
    public CombinedAsyncDisposable AddChildren(IEnumerable<IAsyncDisposable> disposables)
    {
        var newList = children.ToList();
        newList.AddRange(disposables);
        children = newList;
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var item in children)
        {
            await item.DisposeAsync();
        }

        children = ArraySegment<IAsyncDisposable>.Empty;
        GC.SuppressFinalize(this);
    }
}