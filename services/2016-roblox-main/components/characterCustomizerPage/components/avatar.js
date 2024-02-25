import { createUseStyles } from "react-jss";
import CharacterCustomizationStore from "../../../stores/characterPage";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import PlayerImage from "../../playerImage";

const useMyAvatarStyles = createUseStyles({
  header: {
    fontSize: '24px',
  },
  renderButtonWrapper: {
    float: 'left',
    marginBottom: '20px',
  },
  renderButton: {
    fontSize: '14px',
  },
});

const MyAvatar = props => {
  const s = useMyAvatarStyles();
  const characterStore = CharacterCustomizationStore.useContainer();
  const buttonStyles = useButtonStyles();

  return <div className='row'>
    <div className='col-12'>
      <h2 className={s.header}>Avatar</h2>
    </div>
    <div className='col-12'>
      {characterStore.isRendering ? <p>Loading...</p> : <PlayerImage url={characterStore.thumbnail} id={characterStore.userId}/>}
      <div className={s.renderButtonWrapper}>
        {characterStore.isModified && <ActionButton disabled={characterStore.isRendering} label='Save Changes' className={s.renderButton + ' ' + buttonStyles.continueButton} onClick={() => {
          characterStore.requestRender(false);
        }}/>}
      </div>
    </div>
    <div className='col-12'>
      <p className='mb-0'>Something wrong with your Avatar?</p>
      <p className='mb-0'><a href='#' onClick={(e) => {
        e.preventDefault();
        characterStore.requestRender(true);
      }}>Click here to re-draw it!</a></p>
    </div>
  </div>
}

export default MyAvatar;