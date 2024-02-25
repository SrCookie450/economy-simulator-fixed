using System.Threading.Tasks;
using Xunit;

namespace Roblox.Services.IntegrationTest;

public class GroupsServiceIntegrationTest : TestBase
{
    [Fact]
    public async Task Create_Group_And_Join()
    {
        var gs = ServiceProvider.GetOrCreate<GroupsService>();
        var userOne = await CreateRandomUser();
        var userTwo = await CreateRandomUser();
        var groupOne = await CreateRandomGroup(userOne);
        // get info - make sure everything is valid!
        var groupData = await gs.GetGroupById(groupOne);
        Assert.NotNull(groupData);
        Assert.Equal(1, groupData.memberCount);
        Assert.NotNull(groupData.name);
        Assert.NotNull(groupData.description);
        Assert.NotNull(groupData.owner);
        Assert.Equal(userOne, groupData.owner.userId);
        Assert.True(groupData.publicEntryAllowed);
        Assert.False(groupData.isLocked);
        Assert.False(groupData.isBuildersClubOnly);
        // creator should have owner rank
        var userOneRole = await gs.GetUserRoleInGroup(groupOne, userOne);
        Assert.Equal(255, userOneRole.rank);
        Assert.Equal("Owner", userOneRole.name);
        // user two hasn't joined yet - make sure they are a guest
        var userTwoRole = await gs.GetUserRoleInGroup(groupOne, userTwo);
        Assert.Equal(0, userTwoRole.rank);
        Assert.Equal("Guest", userTwoRole.name);
        // now join
        await gs.JoinGroup(groupOne, userTwo);
        userTwoRole = await gs.GetUserRoleInGroup(groupOne, userTwo);
        Assert.Equal(1, userTwoRole.rank);
        // confirm member count was updated
        var newMemberCount = await gs.GetMemberCount(groupOne);
        Assert.Equal(2, newMemberCount);
        // now user two leaves
        await gs.RemoveUserFromGroup(groupOne, userTwo, userTwo);
        // confirm count is back down to one
        newMemberCount = await gs.GetMemberCount(groupOne);
        Assert.Equal(1, newMemberCount);
    }
    
    [Fact]
    public async Task Abandon_And_Claim_Group()
    {
        var gs = ServiceProvider.GetOrCreate<GroupsService>();
        var userOne = await CreateRandomUser();
        var userTwo = await CreateRandomUser();
        var groupOne = await CreateRandomGroup(userOne);
        // user two join
        await gs.JoinGroup(groupOne, userTwo);
        // now user one leaves
        await gs.RemoveUserFromGroup(groupOne, userOne, userOne);
        // confirm user was removed from role
        var role = await gs.GetUserRoleInGroup(groupOne, userOne);
        Assert.Equal(0, role.rank);
        Assert.Equal("Guest", role.name);
        // confirm count is back down to one
        Assert.Equal(1, await gs.GetMemberCount(groupOne));
        // should have no owner
        var groupData = await gs.GetGroupById(groupOne);
        Assert.Null(groupData.owner);
        // claim
        await gs.ClaimGroup(groupOne, userTwo);
        // should still be one member
        Assert.Equal(1, await gs.GetMemberCount(groupOne));
        // new guy should be owner
        groupData = await gs.GetGroupById(groupOne);
        Assert.NotNull(groupData.owner);
        Assert.Equal(userTwo, groupData.owner.userId);
    }

}