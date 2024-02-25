namespace Roblox.Dto.Users;

public class MessageEntryDb
{
    public long senderId { get; set; }
    public string senderUsername { get; set; }
    public long receiverId { get; set; }
    public string receiverUsername { get; set; }
    public string body { get; set; }
    public string subject { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public bool isRead { get; set; }
    public bool isSystemMessage => senderId == 1;
    public bool isReportAbuseDisplayed => senderId != 1;
}