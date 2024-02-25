import getFlag from "../lib/getFlag";
import request, { getBaseUrl, getFullUrl } from "../lib/request"

/**
 * Get chat settings for authenticated user
 * @returns {Promise<{chatEnabled: boolean, isActiveChatUser: boolean}>}
 */
export const getChatSettings = () => {
  return request('GET', getFullUrl('chat', '/v2/chat-settings')).then(d => {
    return d.data;
  });
}

/**
 * Get conversations for the authenticated user
 * @param pageNumber
 * @param pageSize
 * @returns {Promise<{id: number, title: string, hasUnreadMessages: boolean, participants: {type: 'User', targetId: number, name: string}[], conversationType: 'OneToOneConversation' | 'MultiUserConversation' | 'CloudEditConversation', conversationTitle: {titleForViewer: string, isDefaultTitle: boolean}, lastUpdated: string, conversationUniverse: {universeId: number, rootPlaceId: number}}[]>}
 */
export const getUserConversations = async ({pageNumber, pageSize}) => {
  return await request('GET', getFullUrl('chat', '/v2/get-user-conversations?pageNumber='+encodeURIComponent(pageNumber.toString())+'&pageSize='+encodeURIComponent(pageSize.toString()))).then(d => {
    return d.data
  });
}

/**
 * Mark a conversation as read
 * @param conversationId
 * @param endMessageId
 * @returns {Promise<any>}
 */
export const markAsRead = async ({conversationId, endMessageId}) => {
  return await request('POST', getFullUrl('chat', '/v2/mark-as-read'), {
    conversationId,
    endMessageId,
  });
}

/**
 * Start a one-to-one conversation
 * @param userId
 * @returns {Promise<{conversation: {id: number}}>}
 */
export const startOneToOneConversation = async ({userId}) => {
  return await request('POST', getFullUrl('chat', '/v2/start-one-to-one-conversation'), {
    participantUserId: userId,
  }).then(d => d.data);
}

export const updateTypingStatus = async ({conversationId, isTyping}) => {
  return await request('POST', getFullUrl('chat', '/v2/update-user-typing-status'), {
    isTyping: isTyping,
    conversationId,
  });
}

export const sendMessage = async ({conversationId, message}) => {
  return await request('POST', getFullUrl('chat', '/v2/send-message'), {
    conversationId,
    message,
  }).then(d => d.data);
}

/**
 * Multi-get the latest messages for a set of conversation IDs
 * @param conversationIds
 * @returns {Promise<{conversationId: number, chatMessages: {id: string, sent: string, read: boolean, senderTargetId: number, content: string}[]}[]>}
 */
export const multiGetLatestMessages = async ({conversationIds}) => {
  let ids = encodeURIComponent(conversationIds.join(','));
  return await request('GET', getFullUrl('chat', '/v2/multi-get-latest-messages?conversationIds=' + ids)).then(d => {
    return d.data;
  });
}

/**
 * Get messages for a conversation
 * @param conversationId
 * @param pageSize
 * @param startId
 * @returns {Promise<{id: string, sent: string, read: boolean, messageType: string, senderTargetId: number, content: string}[]>}
 */
export const getMessages = async ({conversationId,pageSize, startId}) => {
  return await request('GET', getFullUrl('chat', '/v2/get-messages?conversationId=' + encodeURIComponent(conversationId.toString()) + '&pageSize=' + encodeURIComponent(pageSize.toString()) + '&exclusiveStartMessageId=' + encodeURIComponent(startId
    .toString()))).then(d => {
      return d.data;
  });
}