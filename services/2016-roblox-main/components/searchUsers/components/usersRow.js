import searchUsersStore from "../stores/searchUsersStore";
import {createUseStyles} from "react-jss";
import PlayerImage from "../../playerImage";
import dayjs from "../../../lib/dayjs";
import Link from "../../link";

const useStyles = createUseStyles({
  textRight: {
    textAlign: 'right',
  },
  unknownStatus: {
    color: '#494949',
  },
  onlineStatus: {
    color: '#00bf00',
  },
  offlineStatus: {
    color: 'red',
  },
  status: {
    fontSize: '80px',
    position: 'relative',
    top: '-65px',
    display: 'block',
    height: 0,
  },
  username: {
    marginLeft: '20px',
  },
  userRow: {
    color: 'inherit',
    borderTop: '1px solid #c3c3c3',
    paddingTop: '20px',
    paddingBottom: '20px',
    '&:hover': {
      background: '#e1e1e1',
      cursor: 'pointer',
    },
  },
  colorNormal: {
    color: 'rgb(20,20,20)',
  },
});

const UsersRow = props => {
  const store = searchUsersStore.useContainer();
  const s = useStyles();

  if (!store.data || !store.data.UserSearchResults)
    return null;

  return <div className='row'>
    <div className='col-12'>
      {
        store.data.UserSearchResults.length ? store.data.UserSearchResults.map(v => {
          const presence = store.presence[v.UserId];
          const isUnknown = !presence;
          const isOnline = presence && dayjs(presence.lastOnline).isAfter(dayjs().subtract(5, 'minutes'))

          return <div className={s.userRow} key={v.UserId}>
               <Link href={v.UserProfilePageUrl}>
                 <a>
                   <div className='row'>
                     <div className='col-6 col-md-2 col-lg-1'>
                       <PlayerImage id={v.UserId} />
                     </div>
                     <div className='col-6 col-md-7 col-lg-8'>
                       <p><span className={s.status + ' ' + (isUnknown ? s.unknownStatus : isOnline ? s.onlineStatus : s.offlineStatus)}>.</span> <span className={s.username}>{v.Name}</span> </p>
                     </div>
                     <div className='col-12 col-md-3 col-lg-3'>
                       <p className={s.textRight + ' ' + s.colorNormal}>{presence ? dayjs(presence.lastOnline).format('M/D/YYYY h:mm A') : null}</p>
                     </div>
                   </div>
                 </a>
               </Link>
          </div>
        }) : <div className='row'>
          <div className='col-12'>
            <p className='mt-4'>No results for "{store.keyword}"</p>
          </div>
        </div>
      }
    </div>
  </div>
}

export default UsersRow;