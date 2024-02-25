import request, { getBaseUrl } from "../lib/request"
import { getFullUrl } from "../lib/request";

const baseUrl = getFullUrl('users', '');

export const getMyInfo = () => {
  return request('GET', baseUrl + '/v1/users/authenticated').then(d => d.data)
}

export const getUserInfo = ({ userId }) => {
  return request('GET', baseUrl + '/v1/users/' + userId).then(d => d.data)
}

export const getUserStatus = ({ userId }) => {
  return request('GET', baseUrl + '/v1/users/' + userId + '/status').then(d => d.data)
}

export const updateStatus = ({ newStatus, userId }) => {
  return request('PATCH', getFullUrl('users', '/v1/users/' + userId + '/status'), {
    status: newStatus,
  }).then(d => d.data)
}

export const getPreviousUsernames = async ({ userId }) => {
  let cursor = '';
  let names = [];
  do {
    let results = await request('GET', getFullUrl('users', '/v1/users/' + userId + '/username-history?limit=100&cursor=' + encodeURIComponent(cursor)));
    results.data.data.forEach(v => names.push(v.name));
    cursor = results.data.nextPageCursor;
  } while (cursor !== null);
  return names;
}

export const searchUsers = async ({keyword, limit, offset}) => {
  return await request('GET', getBaseUrl() + 'search/users/results?keyword=' + (keyword||'') + '&maxRows='+limit+'&startIndex='+offset).then(d => d.data);
}

export const getMembershipType = async ({userId}) => {
  return request('GET', getFullUrl('premiumfeatures', '/v1/users/'+userId+'/validate-membership')).then(d => d.data).then(data => {
    if (data === true) {
      return 4;
    }else if (data === false) {
      return 0;
    }
    return data;
  })
}

export const getUserIdByUsername = async (username) => {
  let result = await request('POST', getFullUrl('users', `/v1/usernames/users`), {
    usernames: [username],
  });
  if (!result.data.data.length)
    throw new Error('Invalid username');
  return result.data.data[0].id;
}
