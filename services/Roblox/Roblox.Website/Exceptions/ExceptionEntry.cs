namespace Roblox.Exceptions
{
    public class ExceptionEntry
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }

        public ExceptionEntry(int errorCode, string errorMessage)
        {
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
        }
    }
}