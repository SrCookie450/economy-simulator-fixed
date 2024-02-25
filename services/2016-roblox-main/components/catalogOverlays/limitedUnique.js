import { createUseStyles } from "react-jss";

const useLimitedOverlayStyles = createUseStyles({
  overlay: {
    marginTop: '-48px',
    width: '88px',
    height: '24px',
  },
});

const LimitedUniqueOverlay = props => {
  const s = useLimitedOverlayStyles();
  return <img src='/img/CatalogOverlays/LimitedUnique.png' className={s.overlay}></img>
}

export default LimitedUniqueOverlay;