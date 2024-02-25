using Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/notifications/v2")]
public class NotificationsControllerV2 : ControllerBase
{
    [HttpGet("stream-notifications/unread-count")]
    public dynamic GetUnreadCount()
    {
        return new
        {
            unreadNotifications = 0,
            statusMessage = (string?) null,
        };
    }

    [HttpPost("stream-notifications/clear-unread")]
    [HttpPost("stream-notifications/mark-interacted")]
    public dynamic ClearUnread()
    {
        return new
        {
            statusMessage = "Success",
        };
    }

    [HttpGet("stream-notifications/get-recent")]
    public dynamic GetRecent()
    {
        return new List<int>();
    }

    [HttpGet("notifications/get-settings")]
    public dynamic GetSettings()
    {
        return new
        {
            notificationBandSettings = new List<dynamic>() 
            {
				new {
					notificationSourceType = "Test",
					receiverDestinationType = "DesktopPush",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "Test",
					receiverDestinationType = "MobilePush",
					isEnabled = true,
					isOverridable = false,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "Test",
					receiverDestinationType = "NotificationStream",
					isEnabled = true,
					isOverridable = false,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "FriendRequestReceived",
					receiverDestinationType = "DesktopPush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "FriendRequestReceived",
					receiverDestinationType = "NotificationStream",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "FriendRequestReceived",
					receiverDestinationType = "MobilePush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "FriendRequestAccepted",
					receiverDestinationType = "DesktopPush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "FriendRequestAccepted",
					receiverDestinationType = "NotificationStream",
					isEnabled = true,
					isOverridable = false,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "FriendRequestAccepted",
					receiverDestinationType = "MobilePush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "ChatNewMessage",
					receiverDestinationType = "DesktopPush",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "ChatNewMessage",
					receiverDestinationType = "MobilePush",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "PrivateMessageReceived",
					receiverDestinationType = "DesktopPush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "PrivateMessageReceived",
					receiverDestinationType = "MobilePush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "PrivateMessageReceived",
					receiverDestinationType = "NotificationStream",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "UserAddedToPrivateServerWhiteList",
					receiverDestinationType = "DesktopPush",
					isEnabled = false,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "TeamCreateInvite",
					receiverDestinationType = "DesktopPush",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "GameUpdate",
					receiverDestinationType = "NotificationStream",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
				new {
					notificationSourceType = "DeveloperMetricsAvailable",
					receiverDestinationType = "NotificationStream",
					isEnabled = true,
					isOverridable = true,
					isSetByReceiver = false,
					pushNotificationDestinationPreferences = Array.Empty<int>(),
				},
			},
			optedOutNotificationSourceTypes = Array.Empty<int>(),
			optedOutReceiverDestinationTypes = Array.Empty<int>(),
        };
    }
    
    
}