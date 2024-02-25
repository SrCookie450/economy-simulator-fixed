import React from "react";
import { createUseStyles } from "react-jss";
import PlayerImage from "../../playerImage";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import SmallButtonLink from "./smallButtonLink";
import Subtitle from "./subtitle"
import Link from "../../link";

const useFriendStyles = createUseStyles({
  friendCol: {
    width: '12%',
    paddingLeft: 0,
    paddingRight: 0,
    minWidth: '100px',
  },
  imageWrapper: {
    border: '1px solid #c3c3c3',
  },
  username: {
    fontSize: '16px',
    marginBottom: 0,
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    fontWeight: 300,
    color: '#666',
  },
  cardWrapper: {
    margin: '0 auto',
    paddingLeft: '8px',
    paddingRight: '8px',
  },
  buttonWrapper: {
    marginTop: '10px',
    width: '100px',
    float: 'right',
  },
  sideRow: {
    flexFlow: 'row',
    overflow: 'auto',
  },
})

const Friends = props => {
  const store = UserProfileStore.useContainer();
  const cardStyles = useCardStyles();
  const s = useFriendStyles();
  if (!store.friends || store.friends.length === 0) {
    return null;
  }
  return <div className='row'>
    <div className='col-10'>
      <Subtitle>Friends ({store.friends.length})</Subtitle>
    </div>
    <div className='col-2'>
      <div className={s.buttonWrapper}>
        <SmallButtonLink href={`/users/${store.userId}/friends`}>See All</SmallButtonLink>
      </div>
    </div>
    <div className='col-12'>
      <div className={cardStyles.card}>
        <div className={'row pt-3 pb-3 pe-3 ps-3 me-0 ms-0 ' + s.sideRow}>
          {
            store.friends.slice(0, 10).map(v => {
              return <div className={`col-1 ${s.friendCol}`} key={v.id}>
                <div className={s.cardWrapper}>
                  <Link href={`/users/${v.id}/profile`}>
                    <a>
                      <div className={s.imageWrapper}>
                        <PlayerImage id={v.id}/>
                      </div>
                      <p className={s.username}>{v.name}</p>
                    </a>
                  </Link>
                </div>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}

export default Friends;