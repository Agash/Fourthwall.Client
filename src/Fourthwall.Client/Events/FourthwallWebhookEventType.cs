namespace Fourthwall.Client.Events;

/// <summary>
/// Defines known Fourthwall webhook event type names.
/// </summary>
public static class FourthwallWebhookEventType
{
    // Orders
    /// <summary>An order was placed.</summary>
    public const string OrderPlaced = "ORDER_PLACED";

    /// <summary>An order was updated.</summary>
    public const string OrderUpdated = "ORDER_UPDATED";

    // Donations
    /// <summary>A donation was received.</summary>
    public const string Donation = "DONATION";

    // Memberships / Subscriptions
    /// <summary>A supporter purchased a membership subscription.</summary>
    public const string SubscriptionPurchased = "SUBSCRIPTION_PURCHASED";

    /// <summary>A supporter's membership subscription expired.</summary>
    public const string SubscriptionExpired = "SUBSCRIPTION_EXPIRED";

    /// <summary>A supporter's membership subscription changed (tier, billing cycle, etc.).</summary>
    public const string SubscriptionChanged = "SUBSCRIPTION_CHANGED";

    // Gifts
    /// <summary>A gift purchase was made.</summary>
    public const string GiftPurchase = "GIFT_PURCHASE";

    /// <summary>A gift draw started.</summary>
    public const string GiftDrawStarted = "GIFT_DRAW_STARTED";

    /// <summary>A gift draw ended.</summary>
    public const string GiftDrawEnded = "GIFT_DRAW_ENDED";

    // Products
    /// <summary>A product was created.</summary>
    public const string ProductCreated = "PRODUCT_CREATED";

    /// <summary>A product was updated.</summary>
    public const string ProductUpdated = "PRODUCT_UPDATED";

    // Collections
    /// <summary>A collection was updated.</summary>
    public const string CollectionUpdated = "COLLECTION_UPDATED";

    // Promotions
    /// <summary>A promotion was created.</summary>
    public const string PromotionCreated = "PROMOTION_CREATED";

    /// <summary>A promotion was updated.</summary>
    public const string PromotionUpdated = "PROMOTION_UPDATED";

    /// <summary>A promotion's status changed.</summary>
    public const string PromotionStatusChanged = "PROMOTION_STATUS_CHANGED";

    // Newsletters
    /// <summary>A supporter subscribed to the newsletter.</summary>
    public const string NewsletterSubscribed = "NEWSLETTER_SUBSCRIBED";

    // Thank You
    /// <summary>A thank-you was sent.</summary>
    public const string ThankYouSent = "THANK_YOU_SENT";

    // Membership content
    /// <summary>A membership post was created or updated.</summary>
    public const string MembershipPostUpserted = "MEMBERSHIP_POST_UPSERTED";

    /// <summary>A membership series was created or updated.</summary>
    public const string MembershipSeriesUpserted = "MEMBERSHIP_SERIES_UPSERTED";

    /// <summary>A membership series was deleted.</summary>
    public const string MembershipSeriesDeleted = "MEMBERSHIP_SERIES_DELETED";

    /// <summary>A membership tag was created.</summary>
    public const string MembershipTagCreated = "MEMBERSHIP_TAG_CREATED";

    /// <summary>A membership tier was created or updated.</summary>
    public const string MembershipTierUpserted = "MEMBERSHIP_TIER_UPSERTED";

    /// <summary>A membership tier was deleted.</summary>
    public const string MembershipTierDeleted = "MEMBERSHIP_TIER_DELETED";

    // Platform
    /// <summary>A platform app was disconnected.</summary>
    public const string PlatformAppDisconnected = "PLATFORM_APP_DISCONNECTED";

    // Abandoned carts
    /// <summary>A cart was abandoned for 1 hour.</summary>
    public const string CartAbandoned1H = "CART_ABANDONED_1H";

    /// <summary>A cart was abandoned for 24 hours.</summary>
    public const string CartAbandoned24H = "CART_ABANDONED_24H";

    /// <summary>A cart was abandoned for 72 hours.</summary>
    public const string CartAbandoned72H = "CART_ABANDONED_72H";
}
