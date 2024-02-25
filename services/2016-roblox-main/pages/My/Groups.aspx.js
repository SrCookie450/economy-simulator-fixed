import { useRouter } from "next/dist/client/router";
import React from "react";
import MyGroups from "../../components/myGroups";
import GroupPageStore from "../../components/myGroups/stores/groupPageStore";
import MyGroupsStore from "../../components/myGroups/stores/myGroupsStore";

const MyGroupsPage = props => {
  const router = useRouter();
  return <MyGroupsStore.Provider>
    <GroupPageStore.Provider>
      <MyGroups id={router.query['gid']}/>
    </GroupPageStore.Provider>
  </MyGroupsStore.Provider>
}

export default MyGroupsPage;
