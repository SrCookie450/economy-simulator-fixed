import request, { getFullUrl } from "../lib/request"

export const getUserGroups = ({ userId }) => {
  return request('GET', getFullUrl('groups', `/v1/users/${userId}/groups/roles`)).then(d => d.data.data);
}

export const getPermissionsForRoleset = ({ groupId, rolesetId }) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}/roles/${rolesetId}/permissions`)).then(d => d.data);
}

export const joinGroup = ({ groupId }) => {
  return request('POST', getFullUrl('groups', `/v1/groups/${groupId}/users`));
}

export const leaveGroup = ({ groupId, userId }) => {
  return request('DELETE', getFullUrl('groups', `/v1/groups/${groupId}/users/${userId}`));
}

export const setStatus = ({ groupId, message }) => {
  return request('PATCH', getFullUrl('groups', `/v1/groups/${groupId}/status`), {
    message,
  });
}

export const createGroup = ({ name, description, iconElement }) => {
  const f = new FormData();
  f.append('name', name);
  f.append('description', description);
  f.append('icon', iconElement.files[0]);
  return request('POST', getFullUrl('groups', `/v1/groups/create`), f).then(d => d.data);
}

export const getRoles = ({ groupId }) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}/roles`)).then(d => d.data.roles);
}

export const getMembers = ({groupId, cursor, limit = 10, sortOrder}) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}/users?cursor=${encodeURIComponent(cursor || '')}&limit=${limit}&sortOrder=${sortOrder}`)).then(d => d.data);
}

export const getRolesetMembers = ({ groupId, roleSetId, cursor, limit = 10, sortOrder }) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}/roles/${roleSetId}/users?cursor=${encodeURIComponent(cursor || '')}&limit=${limit}&sortOrder=${sortOrder}`)).then(d => d.data);
}

export const getWall = ({ groupId, cursor, sort, limit }) => {
  return request('GET', getFullUrl('groups', `/v2/groups/${groupId}/wall/posts?sortOrder=${sort}&limit=${limit}&cursor=${encodeURIComponent(cursor || "")}`)).then(d => d.data);
}

export const postToWall = ({ groupId, content }) => {
  return request('POST', getFullUrl('groups', `/v1/groups/${groupId}/wall/posts`), {
    body: content,
  })
}

export const deletePost = ({ groupId, postId }) => {
  return request('DELETE', getFullUrl('groups', `/v1/groups/${groupId}/wall/posts/${postId}`))
}

export const getInfo = ({ groupId }) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}`)).then(d => d.data);
}

export const claimGroupOwnership = ({ groupId }) => {
  return request('POST', getFullUrl('groups', `/v1/groups/${groupId}/claim-ownership`))
}

export const setGroupAsPrimary = ({ groupId }) => {
  return request('POST', getFullUrl('groups', `/v1/user/groups/primary`), { groupId })
}

export const removePrimaryGroup = () => {
  return request('DELETE', getFullUrl('groups', `/v1/user/groups/primary`))
}

export const getPrimaryGroup = ({ userId }) => {
  return request('GET', getFullUrl('groups', `/v1/users/${userId}/groups/primary/role`)).then(d => d.data);
}

export const setUserRole = ({groupId, userId, roleId}) => {
  return request('PATCH', getFullUrl('groups', `/v1/groups/${groupId}/users/${userId}`), {
    roleId: roleId,
  });
}

export const setGroupIcon = ({groupId, icon}) => {
  const f = new FormData();
  f.append('file', icon);
  return request('PATCH', getFullUrl('groups', `/v1/groups/icon?groupId=${groupId}`), f).then(d => d.data);
}

export const setGroupDescription = ({groupId, description}) => {
  return request('PATCH', getFullUrl('groups', `/v1/groups/${groupId}/description`), {
    description,
  });
}

export const getGroupSettings = ({groupId}) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}/settings`)).then(d => d.data);
}

export const setGroupSettings = ({groupId, isApprovalRequired, areEnemiesAllowed, areGroupFundsVisible, areGroupGamesVisible}) => {
  return request('PATCH', getFullUrl('groups', `/v1/groups/${groupId}/settings`), {
    isApprovalRequired, areEnemiesAllowed, areGroupFundsVisible, areGroupGamesVisible
  })
}

export const changeGroupOwner = async ({groupId, userId}) => {
  return request('PATCH', getFullUrl('groups', `/v1/groups/${groupId}/change-owner`), {
    userId,
  })
}

export const createRole = async ({groupId, name, description, rank}) => {
  return request('POST', getFullUrl('groups', `/v1/groups/${groupId}/rolesets/create`), {
    name,
    description,
    rank,
  });
}

export const editRole = async ({groupId, roleId, name, description, rank}) => {
  return request('PATCH', getFullUrl('groups', `/v1/groups/${groupId}/rolesets/${roleId}`), {
    name,
    description,
    rank,
  });
}

export const deleteRole = async ({groupId, roleId}) => {
  return request('DELETE', getFullUrl('groups', `/v1/groups/${groupId}/rolesets/${roleId}`));
}

export const setRolePermissions = async (groupId, roleId, permissions) => {
  return request('PATCH',getFullUrl('groups', `/v1/groups/${groupId}/roles/${roleId}/permissions`), {permissions: permissions});
}

export const oneTimePayout = async ({groupId, userId, amount}) => {
  return request('POST', getFullUrl('groups', `/v1/groups/${groupId}/payouts`), {
    PayoutType: 'FixedAmount',
    Recipients: [
      {
        recipientId: userId,
        recipientType: 'User',
        amount,
      }
    ],
  });
}

export const getGroupInfo = ({ groupId }) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}`)).then(d => d.data);
}

export const getGroupAuditLog = ({ groupId, cursor, userId, action }) => {
  return request('GET', getFullUrl('groups', `/v1/groups/${groupId}/audit-log?cursor=${cursor}&action=${action}&userId=${userId}&sortOrder=desc&limit=100`)).then(d => d.data);
}