import request from "../lib/request"
import { getFullUrl } from "../lib/request";

export const login = ({ username, password }) => {
  return request('POST', getFullUrl('auth', '/v2/login'), {
    ctype: 'username',
    cvalue: username,
    password,
  });
}

export const logout = () => {
  return request('POST', getFullUrl('auth', '/v2/logout'), {});
}

export const changePassword = ({ existingPassword, newPassword }) => {
  return request('POST', getFullUrl('auth', `/v2/user/passwords/change`), {
    currentPassword: existingPassword,
    newPassword,
  });
}

export const validateUsername = ({ username, context }) => {
  return request('GET', getFullUrl('auth', `/v1/usernames/validate?username=${encodeURIComponent(username)}&context=${encodeURIComponent(context)}`)).then(d => d.data)
}

export const changeUsername = ({ username, password }) => {
  return request('POST', getFullUrl('auth', `/v1/username`), {
    username,
    password,
  })
}

export const logoutFromAllOtherSessions = () => {
  return request('POST', getFullUrl('auth', '/v2/logoutfromallsessionsandreauthenticate'))
}