import React from "react";
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import MyGroupsStore from "../stores/myGroupsStore"
import OldCard from "./oldCard";
import SidebarGroupEntry from "./sidebarGroupEntry";
import GroupPageStore from "../stores/groupPageStore";
import groupPage from "./groupPage";

const useStyles = createUseStyles({
  container: {
    minHeight: '300px',
    height: '85vh',
    overflowY: 'auto',
    overflowX: 'hidden'
  },
})

const SideBar = props => {
  const groupPageStore = GroupPageStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const store = MyGroupsStore.useContainer();
  const buttonStyles = useButtonStyles();
  const s = useStyles();

  // Below is all just for group sorting.
  // Preferred order:
  // GroupID of the current page,
  // Primary Group,
  // Owned Groups,
  // Ranks, DESC (Highest rank first)
  let primaryGroup;
  let groupMatchingPage;
  const groups = [];
  if (store.groups) {
    for (const item of store.groups) {
      if (groupPageStore.primary) {
        if (groupPageStore.primary.group.id === item.group.id) {
          primaryGroup = item;
          continue;
        }
      }
      if (item.group.id === groupPageStore.groupId) {
        groupMatchingPage = item;
        continue;
      }
      groups.push(item);
    }
    groups.sort((a, b) => {
      return a.role.rank > b.role.rank ? 1 : -1
    });
    if (primaryGroup) {
      groups.unshift(primaryGroup);
    }
    if (groupMatchingPage) {
      groups.unshift(groupMatchingPage);
    }
  }

  return <OldCard>
    <div className={s.container}>
      <div className='row'>
        <div className='col-12'>
          <ActionButton label='Create' className={buttonStyles.buyButton + ' pt-2 pb-2 font-size-25'} onClick={() => {
            window.location.href = '/My/CreateGroup.aspx';
          }} />
        </div>
      </div>
      <div className='row mt-4'>
        <div className='col-12'>
          {groups.map(v => {
            return <SidebarGroupEntry key={v.group.id} {...v} />
          })}
        </div>
      </div>
    </div>
  </OldCard>
}

export default SideBar;