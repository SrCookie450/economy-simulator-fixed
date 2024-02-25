<script lang="ts">
    import client from "../../lib/request";


    export let userId: string;
    let permissionToAdd: string;
    let permissions = [];
    let permissionsAvailable = [];

    client.get('/staff/permissions?userId=' + userId).then(d => {
        permissions = d.data;
    });

    client.get('/staff/permissions/list').then(d => {
        permissionsAvailable = d.data;
    });

    const quickConfig = (arr) => {
        let promises = [];
                for (const perm of arr) {
                    if (permissions.find(a => a.permission === perm)) continue;
                    promises.push(client.request({
                        method: 'POST',
                        url: '/staff/permissions/?userId=' + userId + "&permission=" + perm,
                    }));
                }
                Promise.all(promises).then(() => {
                    let newPermissions = [...permissions];
                    for (const item of arr) {
                        newPermissions.push({
                            permission: item,
                            userId: parseInt(userId, 10),
                        });
                    }
                    permissions = newPermissions;
                }).catch(e => {
                    alert(e.message);
                })
    }

</script>

<div class="row mt-4">
    <div class="col-12">
        <h3>Permissions</h3>
        <table class="table">
            <thead>
                <tr>
                    <th>Permission</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
                    {#each permissions as permission}
                        <tr>
                            <td>
                                <p class="mb-0 mt-2">{permission.permission}</p>
                            </td>
                            <td>
                                <button class="btn btn-outline-danger" on:click={() => {
                                    client.request({
                                        method: 'DELETE',
                                        url: '/staff/permissions?userId=' + userId + '&permission=' + permission.permission,
                                    }).then(() => {
                                        permissions = permissions.filter(v => v.permission !== permission.permission);
                                    }).catch(e => {
                                        alert(e.message);
                                    })
                                }}>Remove</button>
                            </td>
                        </tr>
                    {/each}
            </tbody>
        </table>
        
    </div>
        <div class="col-4">
            <h3>Add Permission</h3>
            <select class="form-control" bind:value={permissionToAdd}>
                {#each permissionsAvailable as permission}
                    <option value={permission}>{permission}</option>
                {/each}
            </select>
        </div>
        <div class="col-2">
            <button class="btn btn-primary mt-4" on:click={() => {
                if (permissions.find(a => a.permission === permissionToAdd)) return;
                client.request({
                    method: 'POST',
                    url: '/staff/permissions/?userId=' + userId + "&permission=" + permissionToAdd,
                }).then(() => {
                    permissions = [...permissions, {
                        permission: permissionToAdd,
                        userId: parseInt(userId, 10),
                    }];
                }).catch(e => {
                    alert(e.message);
                })
            }}>Add Permission</button>
        </div>
        <div class="col-12 mt-4">
            <h3>Quick Config</h3>
            <button class="btn btn-primary mt-4" on:click={() => {
                const permissions = ['GetStats','GetAlert','SetAlert','CreateUser','GetPendingGroupIcons','GetAssetModerationDetails','GetPendingModerationItems','GetPendingModerationGameIcons','SetGameIconModerationStatus','SetAssetModerationStatus','SetGroupIconModerationStatus','GetGroupManageInfo','GetUserJoinCount','GetUsersList','GetUserDetailed','UnbanUser','BanUser', 'GetUserModerationHistory', 'CreateMessage','GetAdminMessages','NullifyPassword','DestroyAllSessionsForUser','LockAccount','RegenerateAvatar','ResetAvatar','GetAdminLogs','GetUserBadges','GiveUserBadge','DeleteUserBadge','GiveUserRobux','GetUserCollectibles','RemoveUserItem','TrackItem','GiveUserItem','DeleteUser','GetPreviousUsernames','DeleteUsername','DeleteComment','DeleteForumPost','RequestAssetReRender','GetProductDetails','SetAssetProduct','CreateAsset','CreateClothingAsset','CopyClothingFromRoblox','CreateAssetVersion','MigrateAssetFromRoblox','CreateGameForUser','RequestWebsiteUpdate','RunLottery','GetUserStatusHistory','DeleteUserStatus','GetUserCommentHistory','ManageFeatureFlags','GetUsersOnline','GetUsersInGame','GetUserTransactions','ResetUsername','ResetDescription','ManageApplications','ClearApplications','ManageInvites','GetGroupWall','DeleteGroupWallPost','GetAllAssetComments','GetAllUserStatuses','LockAndUnlockGroup','GetGroupStatus','DeleteGroupStatus','ResetGroup','LockForumThread','ManageReports','GetAllAssetOwners','GetDetailsFromThumbnail','SetPermissions','GetGameServers','MakeItemLimited','CreateAssetCopiedFromRoblox','CreateBundleCopiedFromRoblox','GetSaleHistoryForAsset','RefundAndDeleteFirstPartyAssetSale', 'ForceApplication', 'ChangeUsername'];

                quickConfig(permissions);
            }}>All</button>
            <button class="btn btn-primary mt-4" on:click={() => {
                const permissions = ['GetStats', 'GetPendingGroupIcons', 'GetAssetModerationDetails', 'GetPendingModerationItems', 'GetPendingModerationGameIcons', 'SetGameIconModerationStatus', 'SetAssetModerationStatus', 'SetGroupIconModerationStatus', 'GetDetailsFromThumbnail'];

                quickConfig(permissions);
            }}>Image Mod</button>
            <button class="btn btn-primary mt-4 ml-2" on:click={() => {
                const permissions = ['GetStats', 'GetAllAssetComments', 'DeleteComment', 'GetGroupWall', 'DeleteGroupWallPost', 'GetGroupStatus', 'DeleteGroupStatus', 'GetAllUserStatuses', 'DeleteUserStatus', 'DeleteForumPost'];

                quickConfig(permissions);
            }}>Text Mod</button>
            <button class="btn btn-primary mt-4 ml-2" on:click={() => {
                const permissions = [
                    'CreateAssetCopiedFromRoblox',
                    'GetProductDetails',
                    'SetAssetProduct',
                ];
                quickConfig(permissions);
            }}>Asset Copy (Non Limited)</button>
        </div>
</div>/*