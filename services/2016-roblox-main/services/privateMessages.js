import request, { getFullUrl } from "../lib/request";

export const sendMessage = ({ userId, subject, body, replyMessageId, includePreviousMessage }) => {
  return request('POST', getFullUrl('privatemessages', '/v1/messages/send'), {
    recipientid: userId,
    body,
    subject,
    replyMessageId,
    includePreviousMessage,
  }).then(d => d.data);
}

export const getAnnouncements = () => {
  return request('GET', getFullUrl('privatemessages', '/v1/announcements')).then(d => d.data);
}

export const getMessages = ({ tab, offset, limit }) => {
  return request('GET', getFullUrl('privatemessages', `/v1/messages?messageTab=${encodeURIComponent(tab)}&pageSize=${limit}&pageNumber=${offset / limit}`)).then(d => d.data);
}

export const toggleReadStatus = ({ messageIds, isRead }) => {
  return request('POST', getFullUrl('privatemessages', `/v1/messages/${isRead ? 'mark-read' : 'mark-unread'}`), {
    messageIds,
  });
}

export const toggleArchiveStatus = ({ messageIds, isArchived }) => {
  return request('POST', getFullUrl('privatemessages', `/v1/messages/${isArchived ? 'archive' : 'unarchive'}`), {
    messageIds,
  });
}

/**
 * Get the count of unread messages
 * @returns {Promise<number>}
 */
export const getUnreadMessageCount = () => {
  return request('GET', getFullUrl('privatemessages', `/v1/messages/unread/count`)).then(d => d.data.count);
}