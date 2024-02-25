import request, { getFullUrl } from "../lib/request"

export const getFriends = ({ userId }) => {
  return request('GET', getFullUrl('friends', `/v1/users/${userId}/friends`)).then(d => d.data.data);
}

/**
 * Get friend request count for the authenticated user
 * @returns {Promise<number>}
 */
export const getFriendRequestCount = () => {
  return request('GET', getFullUrl('friends', `/v1/user/friend-requests/count`)).then(d => d.data.count);
}

/**
 * @returns {Promise<number>}
 */
export const getFollowersCount = ({ userId }) => {
  return request('GET', getFullUrl('friends', `/v1/users/${userId}/followers/count`)).then(d => d.data.count);
}

export const getFollowers = ({ userId, cursor, limit, sort = 'Asc' }) => {
  return request('GET', getFullUrl('friends', `/v1/users/${userId}/followers?cursor=${encodeURIComponent(cursor || '')}&sort=${sort}&limit=${limit}`)).then(d => d.data);
}

/**
 * @returns {Promise<number>}
 */
export const getFollowingsCount = ({ userId }) => {
  return request('GET', getFullUrl('friends', `/v1/users/${userId}/followings/count`)).then(d => d.data.count);
}

export const getFollowings = ({ userId, cursor, limit, sort = 'Asc' }) => {
  return request('GET', getFullUrl('friends', `/v1/users/${userId}/followings?cursor=${encodeURIComponent(cursor || '')}&sort=${sort}&limit=${limit}`)).then(d => d.data);
}

/**@returns {Promise<string>} */
export const getFriendStatus = ({ authenticatedUserId, userId }) => {
  return request('GET', getFullUrl('friends', `/v1/users/${authenticatedUserId}/friends/statuses?userIds=${userId}`)).then(d => d.data.data[0].status);
}

export const getFriendRequests = ({cursor, limit}) => {
  return request('GET', getFullUrl('friends', `/v1/my/friends/requests?limit=${limit}&cursor=${cursor}`)).then(d => d.data);
}

export const unfriendUser = ({ userId }) => {
  return request('POST', getFullUrl('friends', `/v1/users/${userId}/unfriend`));
}

export const acceptFriendRequest = ({ userId }) => {
  return request('POST', getFullUrl('friends', `/v1/users/${userId}/accept-friend-request`));
}

export const declineFriendRequest = ({ userId }) => {
  return request('POST', getFullUrl('friends', `/v1/users/${userId}/decline-friend-request`));
}

export const sendFriendRequest = ({ userId }) => {
  return request('POST', getFullUrl('friends', `/v1/users/${userId}/request-friendship`));
}

export const followUser = ({ userId }) => {
  return request('POST', getFullUrl('friends', `/v1/users/${userId}/follow`));
}

export const unfollowUser = ({ userId }) => {
  return request('POST', getFullUrl('friends', `/v1/users/${userId}/unfollow`));
}

export const getFollowingStatus = ({ userIds }) => {
  return request('POST', getFullUrl('friends', `/v1/user/following-exists`), {
    targetUserIds: userIds,
  }).then(d => d.data.followings);
}

/**
 * Get if the authenticated user is following the provided userId
 * @returns {Promise<boolean>}
 */
export const isAuthenticatedUserFollowingUserId = ({ userId }) => {
  return getFollowingStatus({ userIds: [userId] }).then(result => {
    return result[0] && result[0].isFollowing || false;
  });
}