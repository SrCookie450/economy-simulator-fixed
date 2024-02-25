namespace Roblox.Libraries.Exceptions
{
    public enum FailType
    {
        Unknown = 0,
        BadRequest = 1,
        FloodCheck = 2,
    }
    
    public class LogicException : System.Exception
    {
        public string errorMessage { get; set; }
        public int errorCode { get; set; }
        public FailType failType { get; set; }
        
        public LogicException(FailType type, int errorCode, string errorMessage)
        {
            this.failType = type;
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
        }

        public static LogicException FromEnum<T>(FailType type, T errorCode, string errorMessage) where T : Enum
        {
            return new LogicException(type, (int)(object) errorCode, errorMessage);
        }
    }
}