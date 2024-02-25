using Roblox.Libraries.Exceptions;

namespace Roblox.Libraries;

public class Logic<T> where T : Enum
{
    public Dictionary<T, string> errorMessages { get; set; }
    public Logic(Dictionary<T, string> errorMessages)
    {
        this.errorMessages = errorMessages;
    }

    public void Requires(T code, bool assertion)
    {
        if (assertion != true)
        {
            throw LogicException.FromEnum(FailType.BadRequest, code, errorMessages[code]);
        }
    }
    
    public void Requires(T code, FailType type, bool assertion)
    {
        if (assertion != true)
        {
            throw LogicException.FromEnum(type, code, errorMessages[code]);
        }
    }
}