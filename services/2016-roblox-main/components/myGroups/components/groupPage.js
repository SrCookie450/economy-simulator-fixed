import dayjs from "dayjs";
import React, { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { getGroupGames } from "../../../services/games";
import { claimGroupOwnership, getPermissionsForRoleset, getPrimaryGroup, joinGroup, setStatus } from "../../../services/groups";
import { multiGetGroupIcons } from "../../../services/thumbnails";
import AuthenticationStore from "../../../stores/authentication";
import ActionButton from "../../actionButton";
import CreatorLink from "../../creatorLink";
import OldVerticalTabs from "../../oldVerticalTabs";
import GroupPageStore from "../stores/groupPageStore";
import Button from "./button";
import GroupWall from "./groupWall";
import MembersRow from "./membersRow";
import GroupStore from "./store";

const useStyles = createUseStyles({
  icon: {
    width: '100%',
    borderRadius: '16px',
  },
  iconWrapper: {
    width: '100%',
    margin: '0 auto',
    display: 'block',
  },
  statEntry: {
    color: '#888',
    fontSize: '13px',
  },
  input: {
    width: '100%',
  },
  shoutBg: {
    background: '#ffeeb2',
    padding: '4px',
    borderRadius: '2px',
    border: '1px solid #f1e0a5',
  },
  rankText: {
    color: '#000',
  },
  description: {
    minHeight: '100px',
    whiteSpace: 'break-spaces',
  },
  groupShoutButton: {
    marginTop: '0',
  },
})

const GroupPage = props => {
  const store = GroupPageStore.useContainer();
  const auth = AuthenticationStore.useContainer();

  const [icon, setIcon] = useState(null);
  const shoutRef = useRef(null);
  useEffect(() => {
    if (!store.groupId) return;
    multiGetGroupIcons({
      groupIds: [store.groupId],
    }).then(d => {
      setIcon(d[0].imageUrl);
    });

    getGroupGames({
      groupId: store.groupId,
      cursor: null,
    }).then(store.setGames);
  }, [store.groupId]);

  useEffect(() => {
    if (!store.groupId) return;
    if (store.rank) {
      getPermissionsForRoleset({
        groupId: store.groupId,
        rolesetId: store.rank.id,
      }).then(d => {
        let obj = {}
        for (const key in d.permissions) {
          for (const nested in d.permissions[key]) {
            obj[nested] = d.permissions[key][nested];
          }
        }
        console.log('[info] authenticated user permissions:', obj)
        // @ts-ignore
        store.setPermissions(obj)
      })
    }
  }, [store.rank]);

  useEffect(() => {
    if (!auth.userId) return;
    getPrimaryGroup({
      userId: auth.userId,
    }).then(store.setPrimary).catch(e => {
      // endpoint fails with "Network error" if response body is null (which is returned when user has no primary group). I'm just gonna ignore it for now.
    })
  }, [auth.userId]);

  const s = useStyles();

  if (!store.info || !store.games) return null;
  return <div className='row mt-4 me-2 ms-2'>
    <div className='col-3'>
      <div className={s.iconWrapper}>
        <img className={s.icon} src={icon}/>
      </div>
      <p className={'mb-0 mt-2 ' + s.statEntry}>Owned By: {store.info.owner ? <CreatorLink id={store.info.owner.userId} name={store.info.owner.username} type='User'/> : 'Nobody!'}</p>
      <p className={'mb-0 ' + s.statEntry}>Members: {store.info.memberCount.toLocaleString()}</p>
      {store.rank && store.rank.rank !== 0 && <p className={'mb-0 mt-3 ' + s.statEntry}>My Rank: <span className={s.rankText}>{store.rank.name}</span></p>}
      {
        store.rank && store.rank.rank === 0 && <div className='mt-4'><ActionButton label='Join' onClick={() => {
          joinGroup({
            groupId: store.groupId,
          }).then(() => {
            window.location.reload();
          })
        }}/></div>
      }
      {
        store.rank && store.rank.rank !== 0 && !store.info.owner && <div className='mt-4'>
          <ActionButton label='Claim' onClick={() => {
            claimGroupOwnership({
              groupId: store.groupId,
            }).then(() => {
              window.location.reload();
            })
          }} />
        </div>
      }
    </div>
    <div className='col-9 ps-0'>
      <h2>{store.info.name}</h2>
      <p className={s.description}>{store.info.description}</p>
      {store.permissions['viewStatus'] && store.info.shout && store.info.shout.body && <div className='row'>
        <div className='col-10 mt-4'>
          <div className={s.shoutBg}>
            <p className='mb-0'>{store.info.shout.body}</p>
          </div>
          <div className='mt-2 ms-4'>
            <p className='mb-0'>
              <span className='fst-italic me-1'><CreatorLink id={store.info.shout.poster.userId} name={store.info.shout.poster.username} type='User' /></span>
              <span className='font-size-12 lighten-3'>{dayjs(store.info.shout.created).format('M/D/YYYY h:mm:ss A')}</span>
            </p>
          </div>
        </div>
      </div>}
      {store.permissions['postToStatus'] && <div className='row'>
        <div className='col-10 mt-4'>
          <div className='row'>
            <div className='col-8 pe-0'>
              <input ref={shoutRef} type='text' placeholder='Enter your shout' className={s.input} maxLength={255} />
            </div>
            <div className='col-4 ps-1'>
              <Button className={s.groupShoutButton} onClick={(e) => {
                setStatus({
                  groupId: store.groupId,
                  message: shoutRef.current.value,
                }).then(() => {
                  window.location.reload();
                }).catch(e => {
                  alert(e.message);
                })
              }}>Group Shout</Button>
            </div>
          </div>
        </div>
      </div>}
    </div>
    <div className='col-12 mt-4'>
      <OldVerticalTabs options={[
        store.games.data.length && {
          name: 'Games',
          element: null,
        },
        {
          name: 'Members',
          element: <MembersRow groupId={store.groupId}/>,
        },
        {
          name: 'Store',
          element: <GroupStore groupId={store.groupId} />,
        }
      ].filter(v => !!v)} />
    </div>
    <div className='divider-top mt-2' />
    <div className='col-12 mt-2'>
      <GroupWall groupId={store.groupId} canView={store.permissions['viewWall']} canPost={store.permissions['postToWall']} canDelete={store.permissions['deleteFromWall']} />
    </div>
  </div>
}

export default GroupPage;