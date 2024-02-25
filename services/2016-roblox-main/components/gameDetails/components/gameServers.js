// Most of this is just guess work. I don't know what the server list looked like, and I can't find any photos/videos.
import { createUseStyles } from "react-jss";
import { getServers } from "../../../services/games";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import CreatorLink from "../../creatorLink";
import PlayerImage from "../../playerImage";
import GameDetailsStore from "../stores/gameDetailsStore";

const ServerEntry = props => {
  const store = GameDetailsStore.useContainer();
  const buttonStyles = useButtonStyles();

  return <div className='row'>
    <div className='col-12 col-lg-2'>
      <ActionButton label='Join' className={buttonStyles.continueButton + ' pt-1 pb-1'} onClick={() => {
        alert('Feature is not implemented'); // todo
      }}></ActionButton>
    </div>
    <div className='col-12 col-lg-10'>
      <p className='mb-0 font-size-15 mt-1'>
        <span>
          <span className='fw-600'>Player Count: </span>{props.CurrentPlayers.length} / {store.universeDetails.maxPlayers} Players
        </span>
        <span>
          <span className='fw-600 ps-4'>FPS: </span> {props.Fps}
        </span>
        <span>
          <span className='fw-600 ps-4'>Ping: </span> {props.Ping}
        </span>
      </p>
    </div>
    <div className='col-12 mt-2 mb-2'>
      <div className='row'>
        {
          props.CurrentPlayers.map(v => {
            return <div className='col-lg-2 col-md-3 col-4' key={v.Id}>
              <div className='row'>
                <div className='col-6'>
                  <PlayerImage id={v.Id} name={v.Username}></PlayerImage>
                </div>
                <div className='col-6 mt-3 overflow-hidden'>
                  <CreatorLink id={v.Id} name={v.Username} type='User'></CreatorLink>
                </div>
              </div>
            </div>
          })
        }
      </div>
    </div>
    <div className='col-12 mb-4'>
      <div className='divider-top mt-1'></div>
    </div>
  </div>
}

const useStyles = createUseStyles({
  buttonWrapper: {
    margin: '0 auto',
    width: '100%',
    maxWidth: '200px',
  },
})

const GameServers = props => {
  const store = GameDetailsStore.useContainer();
  const s = useStyles();
  const showButton = !store.servers || store.servers && store.servers.areMoreAvailable && !store.servers.loading;


  return <div className='row'>
    <div className='col-12 mt-4 mb-4'>
      {
        store.servers && store.servers.Collection && store.servers.Collection.map(v => {
          return <ServerEntry key={v.Guid} {...v}></ServerEntry>
        }) || null
      }
      {
        store.servers && store.servers.Collection && store.servers.Collection.length === 0 && <p>Nobody is playing this game.</p> || null
      }
      <div className={s.buttonWrapper}>
        {showButton && <ActionButton label='Load Games' onClick={(e) => {
          if (store.servers && store.servers.loading) {
            return;
          }
          if (!store.servers) {
            store.setServers({
              loading: true,
            });
          }
          getServers({
            placeId: store.details.id,
            offset: store.servers && store.servers.offset || 0,
          }).then(servers => {
            store.setServers({
              ...servers,
              loading: false,
              areMoreAvailable: servers.Collection.length >= 10,
              offset: (store.servers && store.servers.offset || 0) + 10,
            })
          })
        }}></ActionButton>}
      </div>
    </div>
  </div>
}

export default GameServers;