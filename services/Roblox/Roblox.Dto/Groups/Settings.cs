namespace Roblox.Dto.Groups;

public class GroupSettingsEntry
{
    public bool isApprovalRequired { get; set; }
    public bool areEnemiesAllowed { get; set; }
    public bool areGroupFundsVisible { get; set; }   
    public bool areGroupGamesVisible { get; set; }
    public bool isBuildersClubRequired => false;
}