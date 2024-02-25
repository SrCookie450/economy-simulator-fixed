import request, { getFullUrl } from "../lib/request"

/**
 * Multi-get presence for the userIds
 * @param {{userIds: (number | string)[]}} param0 
 * @returns {Promise<{userPresenceType: string; lastLocation: string; placeId: number | null; gameId: number | null; userId: number; lastOnline: string;}[]>}
 */
export const multiGetPresence = ({ userIds }) => {
  return request('POST', getFullUrl('presence', `/v1/presence/users`), {
    userIds,
  }).then(d => d.data.userPresences);
}