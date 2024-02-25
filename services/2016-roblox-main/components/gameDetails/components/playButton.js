import { useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { launchGame } from "../../../services/games";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";

const useStyles = createUseStyles({
  buttonWrapper: {
    width: '100%',
    maxWidth: '200px',
  },
  button: {
    width: '100%',
    paddingTop: '2px',
    paddingBottom: '4px',
  },
})

/**
 * Play button
 * @param {{placeId: number}} props 
 * @returns 
 */
const PlayButton = props => {
  const [error, setError] = useState(null);
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const onClick = e => {
    if (getFlag('launchUsingEsURI', false)) {
      e && e.preventDefault && e.preventDefault();
      if (!auth.isAuthenticated) {
        window.location.href = '/Login';
        return
      }
      launchGame({
        placeId: props.placeId,
      }).catch(e => {
        // todo: modal
        setError(e.message);
      });
    }else if (getFlag('launchUsingEsWeb', false)) {
      window.location.href = '/RobloxApp/Play?placeId=' + props.placeId;
    }else{
      // TODO: Roblox URI handling here (is this even possible?)
      alert('Support for joining ROBLOX games is not implemented. You will be redirected to ROBLOX to play this game.');
      window.location.href = 'https://www.roblox.com/games/' + props.placeId + '/--';
    }
  }

  return <div className='row'>
    <div className={'col-12 mx-auto ' + s.buttonWrapper}>
      {error && <p className='text-danger mb-1 mt-1'>{error}</p>}
      <ActionButton label='Play' className={s.button + ' ' + buttonStyles.buyButton} onClick={onClick}></ActionButton>
    </div>
  </div>
}

export default PlayButton;