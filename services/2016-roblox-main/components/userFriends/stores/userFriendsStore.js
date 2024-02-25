import { useState } from "react";
import { createContainer } from "unstated-next";

const UserFriendsStore = createContainer(() => {
  const [userId, setUserId] = useState(null);

  return {
    userId,
    setUserId,
  }
});

export default UserFriendsStore;