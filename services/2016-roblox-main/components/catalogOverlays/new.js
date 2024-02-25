import { createUseStyles } from "react-jss";

const useNewOverlayStyles = createUseStyles({
  overlay: {
    marginBottom: '-68px',
    float: 'right',
    width: '53px',
    height: '53px',
    zIndex: 2,
    position: 'relative',
  },
});

const NewOverlay = props => {
  const s = useNewOverlayStyles();
  return <img src='/img/CatalogOverlays/New.png' className={s.overlay} alt='Newly Released'></img>
}

export default NewOverlay;