import { createUseStyles } from "react-jss";

const useLimitedOverlayStyles = createUseStyles({
  overlay: {
    marginTop: '-48px',
    width: '70px',
    height: '24px',
  },
});

const LimitedOverlay = props => {
  const s = useLimitedOverlayStyles();
  return <img src='/img/CatalogOverlays/Limited.png' className={s.overlay}></img>
}

export default LimitedOverlay;