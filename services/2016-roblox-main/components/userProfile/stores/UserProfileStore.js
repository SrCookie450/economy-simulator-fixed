import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import getFlag from "../../../lib/getFlag";
import { getFollowersCount, getFollowingsCount, getFriends, getFriendStatus, isAuthenticatedUserFollowingUserId } from "../../../services/friends";
import { getUserGames } from "../../../services/games";
import { getUserGroups } from "../../../services/groups";
import { getPreviousUsernames, getUserInfo, getUserStatus } from "../../../services/users";

const UserProfileStore = createContainer(() => {
  const [userId, setUserId] = useState(null);
  const [username, setUsername] = useState(null);
  const [lastError, setLastError] = useState(null);
  const [userInfo, setUserInfo] = useState(null);
  const [status, setStatus] = useState(null);
  const [previousNames, setPreviousNames] = useState(null);
  const [friends, setFriends] = useState(null);
  const [followersCount, setFollowersCount] = useState(null);
  const [followingsCount, setFollowingsCount] = useState(null);
  const [friendStatus, setFriendStatus] = useState(null);
  const [groups, setGroups] = useState(null);
  const [createdGames, setCreatedGames] = useState(null);
  const [tab, setTab] = useState('About');
  const [isFollowing, setIsFollowing] = useState(null);

  useEffect(() => {
    if (!userId) return;
    getUserInfo({ userId }).then(result => {
      setUserInfo(result);
      setUsername(result.name);
    }).catch(e => {
      setLastError('InvalidUserId');
    });
    getPreviousUsernames({ userId: userId }).then(setPreviousNames);
    if (getFlag('userProfileUserStatusEnabled', true))
      getUserStatus({ userId }).then(setStatus);
    getFollowersCount({ userId }).then(setFollowersCount);
    getFollowingsCount({ userId }).then(setFollowingsCount);
    getFriends({ userId }).then(setFriends);
    getUserGroups({ userId }).then(setGroups);
    getUserGames({ userId, cursor: '' }).then(d => {
      setCreatedGames(d.data);
    });
    isAuthenticatedUserFollowingUserId({
      userId,
    }).then(setIsFollowing);
  }, [userId]);

  return {
    userId,
    setUserId,

    lastError,
    setLastError,

    username,
    userInfo,

    status,
    setStatus,

    previousNames,
    setPreviousNames,

    followersCount,
    setFollowersCount,
    followingsCount,
    setFollowingsCount,

    friends,
    setFriends,
    friendStatus,
    setFriendStatus,

    groups,
    setGroups,

    createdGames,
    setCreatedGames,

    tab,
    setTab,

    isFollowing,
    setIsFollowing,

    getFriendStatus: (authenticatedUserId) => {
      getFriendStatus({ authenticatedUserId, userId }).then(setFriendStatus);
    },
  }
});

export default UserProfileStore;