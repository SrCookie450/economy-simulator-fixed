import { useRouter } from "next/dist/client/router";
import React from "react";
import Theme2016 from "../../../components/theme2016";
import UserProfile from "../../../components/userProfile";
import UserProfileStore from "../../../components/userProfile/stores/UserProfileStore";

const UserProfilePage = props => {
  const router = useRouter();
  const userId = router.query['userId'];
  return <UserProfileStore.Provider>
    <Theme2016>
      <UserProfile userId={userId}/>
    </Theme2016>
  </UserProfileStore.Provider>
}

export default UserProfilePage;