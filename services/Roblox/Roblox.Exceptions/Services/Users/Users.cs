namespace Roblox.Exceptions.Services.Users;

/// <summary>User does not exist, is hidden, or is terminated</summary>
public class UserNotFoundException : Exception { }

public class AccountLastOnlineTooRecentlyException : Exception {}