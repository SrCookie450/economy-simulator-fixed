import { createUseStyles } from "react-jss";

const useTimerStyles = createUseStyles({
  overlay: {
    marginBottom: '-42px',
    float: 'right',
    width: '36px',
    height: '36px',
    zIndex: 2,
    position: 'relative',
  },
});

const TimerOverlay = props => {
  const s = useTimerStyles();
  return <img src='/img/CatalogOverlays/Timer.png' className={s.overlay} alt='Available for a Limited Time'></img>
}

export default TimerOverlay;