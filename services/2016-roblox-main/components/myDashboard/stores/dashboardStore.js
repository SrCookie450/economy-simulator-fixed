import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import { multiGetPresence } from "../../../services/presence";
import dayjs from "dayjs";

const DashboardStore = createContainer(() => {
  const [friends, setFriends] = useState(null);
  const [friendStatus, setFriendStatus] = useState(null);

  useEffect(() => {
    if (!friends || friendStatus) {
      return
    }
    multiGetPresence({
      userIds: friends.map(v => v.id),
    }).then(d => {
      let obj = {}
      for (const user of d) {
        obj[user.userId] = user;
      }
      setFriendStatus(obj);
      let sortedFriends = friends.sort((a,b) => {
        const onlineA = dayjs(obj[a.id].lastOnline);
        const onlineB = dayjs(obj[b.id].lastOnline);
        return onlineA.isAfter(onlineB) ? -1 : onlineA.isSame(onlineB) ? 0 : 1;
      });
      setFriends([...sortedFriends]);
    }).catch(e => {
      console.error('[error] friends err', e);
    })
  }, [friends, friendStatus]);
  return {
    friends,
    setFriends,

    friendStatus,
    setFriendStatus,
  }
});

export default DashboardStore;