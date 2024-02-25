namespace Roblox.Models.Economy
{
    public enum PurchaseType
    {
        Purchase = 1,
        Sale,
        Commission,
        GroupPayouts,
        AdSpend,
        BuildersClubStipend,
        TicketsStipend,
        // ES Extensions
        PlayingGame = 100,
        PlaceVisit,
    }

    public enum CurrencyType
    {
        Robux = 1,
        Tickets = 2
    }
    
    public enum TransactionSubType 
    {
        ItemPurchase = 1,
        ItemSale = 2,
        ItemResalePurchase = 3,
        UsernameChange = 4,
        ItemResale = 5,
        GroupCreation = 6,
        GroupRoleSet,
        AudioUploadLong = 8,
        PositionOpen,
        PositionClose,
        PositionSale,
        PositionPurchase,
        StaffAssetModeration,
        StaffApplicationReview,
        StaffTextModeration,
        StaffReportReview,
        GroupPayoutReceived,
        GroupPayoutSent,
    }

    public enum PurchaseAbuseFailureReason
    {
        Ok = 1,
        Unknown,
        UsersRelatedAndCreatedTooEarly,
        UsersRelatedAndPriceIsEqualToBalance,
        UsersRelatedAndTooMuchTransacted,
        UsersRelatedAndTooMuchTransactedIfCompleted,
        UsersRelatedPurchasedTooMany,
    }

    public enum InternalPurchaseFailReason
    {
        Unknown = 0,
        Ok,
        UserAlreadyOwnsBeforePurchase,
        AssetDoesNotExist,
        AssetNotForSale,
        AssetExpired,
        AssetPriceIsNull,
        AssetPriceLessThanZero,
        BalanceLessThanPrice,
        AssetStockExhausted,
        UserAssetPriceIsZero,
        UserAssetBuyerAndSellerAreSame,
        UserAssetPriceIsLessThanOne,
        BalanceWouldBeLessThanZeroAfterSale,
        UserWouldExceedMaximumCopiesIfPurchased,
    }
    
    public class InternalPurchaseFailureException : Exception
    {
        public InternalPurchaseFailReason reason { get; set; }
        public InternalPurchaseFailureException(InternalPurchaseFailReason reason) : base("Purchase failed: " + reason)
        {
            this.reason = reason;
        }
    }
}