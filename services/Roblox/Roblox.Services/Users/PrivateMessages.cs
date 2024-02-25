using System.Text.RegularExpressions;
using Dapper;
using Roblox.Dto;
using Roblox.Dto.Users;
using Roblox.Metrics;
using Roblox.Models.Users;
using Roblox.Services.Exceptions;

namespace Roblox.Services;

public class PrivateMessagesService : ServiceBase, IService
{
    public async Task<int> CountUnreadMessages(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) as total FROM user_message WHERE (NOT is_read OR is_read IS NULL) AND user_id_to = :user_id", new
            {
                user_id = userId,
            });
        return result.total;
    }

    public async Task<MessageEntryDb> GetMessageById(long messageId)
    {
        var result = await db.QuerySingleOrDefaultAsync<MessageEntryDb>(
            "SELECT um.id, user_id_from as senderId, user_id_to as receiverId, um.subject, um.body, um.created_at as created, um.updated_at as updated, is_read as isRead, u2.username as receiverUsername, u3.username as senderUsername FROM user_message AS um INNER JOIN \"user\" AS u2 ON u2.id = um.user_id_to INNER JOIN \"user\" AS u3 ON u3.id = um.user_id_from WHERE um.id = :id", new
            {
                id = messageId,
            });
        if (result == null) throw new RecordNotFoundException();
        return result;
    }

    private async Task<bool> CanSendMessage(long userId, long contextUserId)
    {
        var privacy = await db.QuerySingleOrDefaultAsync("SELECT user_settings.private_message_privacy FROM user_settings WHERE user_id = :uid", new
        {
            uid = userId,
        });
        // TODO: Actual checks
        return ((GeneralPrivacy) privacy.private_message_privacy) == GeneralPrivacy.All;
    }

    public async Task<bool> IsFloodChecked(long senderUserId)
    {
        var sentMessages = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_message WHERE created_at > :dt AND user_id_from = :user_id", new
            {
                user_id = senderUserId,
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
            });
        // local limit
        if (sentMessages.total > 20)
        {
            UserMetrics.ReportMessageFloodCheckReached(senderUserId, sentMessages.total);
            return true;
        }

        var allSiteMessages = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_message WHERE created_at > :dt", new
            {
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
            });
        // temporary global limit
        if (allSiteMessages.total > 100)
        {
            UserMetrics.ReportGlobalMessageFloodCheckReached(senderUserId, sentMessages.total);
            return true;
        }
        
        return false;
    }

    public async Task CreateMessage(long recipientUserId, long senderUserId, string subject, string body,
        long? replyMessageId = null, bool includePreviousMessage = false)
    {
        var realBody = body;
        var realSubject = subject;
        if (replyMessageId != null)
        {
            // First check, message itself must be under 1k
            if (realBody.Length > 1000)
                throw new ArgumentException("Please shorten your message to 1,000 characters or less and try again.");
            var previousMessage = await GetMessageById((long)replyMessageId);
            if (previousMessage.receiverId != senderUserId || previousMessage.senderId != recipientUserId)
                throw new Exception("Sender is not authorized to view this message");
            if (includePreviousMessage)
            {
                var dateFormat = "M/d/yyyy";
                var timeFormat = "h:mm tt";
                var header =
                    $"On {previousMessage.created.ToString(dateFormat)} at {previousMessage.created.ToString(timeFormat)}, {previousMessage.senderUsername} wrote:\n{previousMessage.body}";

                realBody += $"\n\n\n------------------------------\n{header}";
            }

            realSubject = "RE: " + previousMessage.subject;
            recipientUserId = previousMessage.senderId;
        }

        if (senderUserId != 1)
        {
            var canSend = await CanSendMessage(recipientUserId, senderUserId);
            if (!canSend)
                throw new ArgumentException("Recipient privacy settings prevent the creation of this message");

            canSend = await CanSendMessage(senderUserId, recipientUserId);
            if (!canSend)
                throw new ArgumentException("Sender privacy settings prevent the creation of this message");
        }

        // Second check - message with previous must be under 8192
        if (realBody.Length > 8192)
            throw new ArgumentException("Message is too long. Please uncheck Include previous message and try again.");
        if (subject.Length > 63)
            throw new ArgumentException("Please shorten your subject to 64 characters or less and try again.");

        await InsertAsync("user_message", new
        {
            user_id_to = recipientUserId,
            user_id_from = senderUserId,
            subject = realSubject,
            body = realBody,
            is_read = false,
            is_archived = false,
        });
        
    }

    public async Task SetReadStatus(long messageId, bool isRead)
    {
        await db.ExecuteAsync("UPDATE user_message SET is_read = :status WHERE id = :id", new {id = messageId, status = isRead});
    }
    
    public async Task SetArchiveStatus(long messageId, bool isArchived)
    {
        await db.ExecuteAsync("UPDATE user_message SET is_archived = :status WHERE id = :id", new {id = messageId, status = isArchived});
    }

    public async Task<dynamic> GetMessages(long userId, string box, int limit, int offset)
    {
        var sq = new SqlBuilder();
        var selectTemplate = sq.AddTemplate(
            "SELECT user_message.created_at, user_message.updated_at, user_message.is_read, user_message.subject, user_message.body, user_message.id, user_id_to, user_id_from, u2.username as user_from_name, u1.username as user_to_name FROM user_message INNER JOIN \"user\" as u2 ON u2.id = user_id_from INNER JOIN \"user\" as u1 ON u1.id = user_id_to /**where**/ ORDER BY user_message.id DESC LIMIT :limit OFFSET :offset", new
            {
                limit,
                offset,
            });
        var countTemplate = sq.AddTemplate("SELECT COUNT(*) AS total FROM user_message /**where**/");
        
        // TODO: Enum would be nice...
        if (box == "inbox")
        {
            sq.Where("user_id_to = " + userId).Where("is_archived = false");
        }
        else if (box == "archive")
        {
            sq.Where("user_id_to = " + userId).Where("is_archived = true");
        }
        else if (box == "sent")
        {
            sq.Where("user_id_from = " + userId);
        }
        else
        {
            throw new ArgumentException("messageTab does not exist: " + box);
        }

        var total = await db.QuerySingleOrDefaultAsync<Total>(countTemplate.RawSql, countTemplate.Parameters);
        var messages = await db.QueryAsync(selectTemplate.RawSql, selectTemplate.Parameters);
        return new
        {
            collection = messages.Select(c => new
            {
                id = c.id,
                sender = new
                {
                    id = c.user_id_from,
                    name = c.user_from_name,
                    displayName = c.user_from_name,
                },
                recipient = new
                {
                    id = c.user_id_to,
                    name = c.user_to_name,
                    displayName = c.user_to_name,
                },
                subject = c.subject,
                body = c.body,
                created = c.created_at,
                updated = c.updated_at,
                isRead = c.is_read,
                isSystemMessage = c.user_id_from == 1,
                isReportAbuseDisplayed = c.user_id_from != 1,
            }),
            totalCollectionSize = total.total,
            totalPages = total.total / limit,
            pageNumber = offset / limit,
        };
    }

    public bool IsThreadSafe()
    {
        return true;
    }

    public bool IsReusable()
    {
        return false;
    }
}