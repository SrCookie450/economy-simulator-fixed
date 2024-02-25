using Roblox.Models.Users;

namespace Roblox.Dto.Users;

public class UserSettingsEntry
{
    public ThemeTypes theme { get; set; }
    public Gender gender { get; set; }
    public InventoryPrivacy inventoryPrivacy { get; set; }
    public GeneralPrivacy tradePrivacy { get; set; }
    public TradeQualityFilter tradeFilter { get; set; }
}