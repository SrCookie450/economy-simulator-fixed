import React, { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { followUser, unfollowUser } from "../../../services/friends";
import { multiGetPresence } from "../../../services/presence";
import {getMembershipType, updateStatus} from "../../../services/users";
import AuthenticationStore from "../../../stores/authentication";
import Dropdown2016 from "../../dropdown2016";
import PlayerHeadshot from "../../playerHeadshot";
import Activity from "../../userActivity";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import FriendButton from "./friendButton";
import MessageButton from "./messageButton";
import RelationshipStatistics from "./relationshipStatistics";

const useHeaderStyles = createUseStyles({
  iconWrapper: {
    border: '1px solid #B8B8B8',
    margin: '0 auto',
    maxWidth: '110px',
  },
  username: {
    fontSize: '30px',
    fontWeight: 400,
  },
  userStatus: {
    fontSize: '16px',
    fontWeight: 300,
  },
  dropdown: {
    float: 'right',
  },
  updateStatusInput: {
    width: 'calc(100% - 140px)',
    border: '1px solid #c3c3c3',
    borderRadius: '4px',
    '@media(max-width: 992px)': {
      width: '100%',
    }
  },
  updateStatusButton: {
    cursor: 'pointer',
    marginTop: '2px',
    fontSize: '12px',
  },
  activityWrapper: {
    position: "relative",
    float: 'right',
    marginRight: '-10px',
    marginTop: '-18px',
  },
});

const ProfileHeader = props => {
  const auth = AuthenticationStore.useContainer();
  const store = UserProfileStore.useContainer();

  const statusInput = useRef(null);

  const [dropdownOptions, setDropdownOptions] = useState(null);
  const [editStatus, setEditStatus] = useState(false);
  const [status, setStatus] = useState(null);
  const [bcLevel, setBcLevel] = useState(0);

  useEffect(() => {
    // reset
    setStatus(null);
    setBcLevel(0);
    setEditStatus(false);
    setDropdownOptions(null);

  }, [store.userId]);

  useEffect(() => {
    if (auth.isPending) return;

    multiGetPresence({ userIds: [store.userId] }).then((d) => {
      setStatus(d[0]);
    });
    getMembershipType({userId: store.userId}).then(d => {
      setBcLevel(d);
    }).catch(e => {
      // can fail when not logged in :(
    })
    const buttons = [];
    const isOwnProfile = auth.userId == store.userId;
    if (isOwnProfile) {
      // Exclusive to your own profile
      buttons.push({
        name: 'Update Status',
        onClick: e => {
          e.preventDefault();
          setEditStatus(!editStatus);
        },
      })
    } else {
      // Exclusive to profiles other than your own
      buttons.push({
        name: store.isFollowing ? 'Unfollow' : 'Follow',
        onClick: (e) => {
          e.preventDefault();
          store.setIsFollowing(!store.isFollowing);
          if (store.isFollowing) {
            store.setFollowersCount(store.followersCount - 1);
            unfollowUser({ userId: store.userId });
          } else {
            store.setFollowersCount(store.followersCount + 1);
            followUser({ userId: store.userId });
          }
        },
      });
    }
    // TODO: wait for accountsettings to add "get blocked status" endpoint
    /*
    arr.push({
      name: 'Block User',
      onClick: () => {
        console.log('Block user!');
      },
    });
    */
    buttons.push({
      name: 'Inventory',
      url: `/users/${store.userId}/inventory`,
    });
    buttons.push({
      name: 'Collectibles',
      url: `/internal/collectibles?userId=${store.userId}`,
    });
    if (!isOwnProfile) {
      buttons.push({
        name: 'Trade',
        onClick: (e) => {
          e.preventDefault();
          window.open("/Trade/TradeWindow.aspx?TradePartnerID=" + store.userId, "_blank", "scrollbars=0, height=608, width=914");
        }
      });
    }
    setDropdownOptions(buttons);
  }, [auth.userId, auth.isPending, store.isFollowing, editStatus, store.userId]);

  const s = useHeaderStyles();
  const cardStyles = useCardStyles();

  const showButtons = auth.userId != store.userId && !auth.isPending;

  const BcIcon = () => {
    if (bcLevel === 0) {
      return null;
    }
    // 1 = BC
    // 2 = TBC
    // 3 = OBC
    // 4 = Premium
    // 0 = None
    switch(bcLevel) {
      case 1:
      case 4:
        return <span className="icon-bc" />
      case 2:
        return <span className="icon-tbc" />
      case 3:
        return <span className="icon-obc" />
      default:
        return null;
    }
  }

  return <div className='row mt-2'>
    <div className='col-12'>
      <div className={'card ' + cardStyles.card}>
        <div className='card-body'>
          <div className='row'>
            <div className='col-12 col-lg-2 pe-0'>
              <div className={s.iconWrapper}>
                <PlayerHeadshot id={store.userId} name={store.username}/>
                {status && <div className={s.activityWrapper}><Activity relative={false} {...status}></Activity></div>}
              </div>
            </div>
            <div className='col-12 col-lg-10 ps-0'>
              <h2 className={s.username}>{store.username} {<BcIcon />} <div className={s.dropdown}>
                {dropdownOptions && <Dropdown2016 options={dropdownOptions}/>}
              </div></h2>
              {editStatus ? <div>
                  <input ref={statusInput} type='text' className={s.updateStatusInput} maxLength={255} defaultValue={store.status?.status || ''}/>
                  <p className={s.updateStatusButton} onClick={() => {
                    let v = statusInput.current.value;
                    store.setStatus({
                      status: v,
                    });
                    setEditStatus(false);
                    updateStatus({
                      newStatus: v,
                      userId: auth.userId,
                    })
                  }}>Update Status</p>
              </div> : (!store.status || !store.status.status) ? <p>&emsp;</p> : <p className={s.userStatus}>&quot;{store.status.status}&quot;</p>
              }
              <div className='row'>
                <RelationshipStatistics id='friends' label='Friends' value={store.friends?.length} userId={store.userId}/>
                <RelationshipStatistics id='followers' label='Followers' value={store.followersCount} userId={store.userId}/>
                <RelationshipStatistics id='followings' label='Following' value={store.followingsCount} userId={store.userId}/>
                {
                  showButtons && <>
                    <div className='col-6 col-lg-2 offset-lg-2 pe-1'>
                      <MessageButton/>
                    </div>
                    <div className='col-6 col-lg-2 ps-1'>
                      <FriendButton/>
                    </div>
                  </>
                }
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default ProfileHeader;