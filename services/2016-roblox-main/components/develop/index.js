import React, {useEffect, useReducer, useState} from "react";
import { createUseStyles } from "react-jss";
import AdBanner from "../ad/adBanner";
import OldVerticalTabs from "../oldVerticalTabs";
import CreationsTab from './components/creationsTab';
import NotAvailable from "./components/notAvailable";
import authentication from "../../stores/authentication";
import {getPermissionsForRoleset, getUserGroups} from "../../services/groups";

const useStyles = createUseStyles({
  developerContainer: {
    backgroundColor: '#fff',
    padding: '4px 8px',
    overflow: 'hidden',
  },
});

const myGroups = (prev, act) => {
  let ns = [];
  if (prev) {
    ns = [...prev];
  }
  if (act.action === 'ADD') {
    ns.push(act.group);
  }
  return ns;
}

const Develop = props => {
  const s = useStyles();
  const [options, setOptions] = useState([]);
  const [tab, setTab] = useState(null);

  const [groupId, setGroupId] = useState(null);
  const [groups, dispatchGroups] = useReducer(myGroups, null);
  const auth = authentication.useContainer();
  useEffect(() => {
    if (!auth.userId) return;

    getUserGroups({
      userId: auth.userId,
    }).then(myGroups => {
      myGroups.forEach(v => {
        getPermissionsForRoleset({
          groupId: v.group.id,
          rolesetId: v.role.id,
        }).then(roleData => {
          if (roleData.permissions.groupEconomyPermissions.manageGroupGames) {
            dispatchGroups({
              action: 'ADD',
              group: v.group,
            });
          }
        }).catch(e => {
          // doesn't have permission to view permissions!
        })
      })
    })
  }, [auth.userId]);

  useEffect(() => {
    if (groupId === null && groups && groups.length > 0) {
      setGroupId(groups[0].id);
    }
  }, [groupId, groups]);

  useEffect(() => {
    let defaultOpts = [
      {
        name: 'My Creations',
        element: <CreationsTab id={props.id} />,
      },
      {
        name: 'Group Creations',
        element: <CreationsTab id={props.id} group={true} groupId={groupId} groups={groups} setGroupId={setGroupId} />,
      },
      {
        name: 'Library',
        element: <NotAvailable />,
      },
      {
        name: 'Developer Exchange',
        element: <NotAvailable />,
      },
    ];
    if (tab === null)
      setTab(defaultOpts[0].name);
    setOptions(defaultOpts);
  }, [props.id, groupId, tab]);
  if (!options.length)
    return null;

  return <div className='container'>
    <AdBanner/>
    <div className={s.developerContainer}>
      <OldVerticalTabs options={options} default={tab} onChange={n => setTab(n.name)} />
    </div>
  </div>
}

export default Develop;