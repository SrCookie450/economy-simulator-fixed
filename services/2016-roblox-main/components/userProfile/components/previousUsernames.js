import { useState } from "react";
import { createUseStyles } from "react-jss";
import { isTouchDevice } from "../../../lib/utils";
import UserProfileStore from "../stores/UserProfileStore";

const useStyles = createUseStyles({
  body: {
    fontWeight: 300,
    marginBottom: 0,
    fontSize: '16px',
    padding: '15px 20px',
  },
  previousNamesLabel: {
    fontSize: '18px',
    cursor: 'pointer',
    userSelect: 'none',
  },
  icon: {

  },
  previousNamesToolTip: {
    position: 'absolute',
    width: '150px',
    padding: '4px 8px',
    background: 'rgba(0,0,0,0.65)',
    zIndex: 99,
  },
  previousName: {
    color: '#fff',
    marginBottom: 0,
  },
});

const PreviousUsernames = props => {
  const store = UserProfileStore.useContainer();
  const [hasTouchScreen] = useState(isTouchDevice());
  const s = useStyles();
  const [tooltipOpen, setTooltipOpen] = useState(false);

  const onMouseEnter = () => {
    if (hasTouchScreen) return;
    setTooltipOpen(true);
  }

  const onMouseLeave = () => {
    if (hasTouchScreen) return;
    setTooltipOpen(false);
  }

  const onClick = () => {
    if (!hasTouchScreen) return;
    setTooltipOpen(!tooltipOpen);
  }

  if (store.previousNames === null || store.previousNames.length === 0) return null;
  const requiredHeight = store.previousNames.length * 19.41;
  return <div>
    <p className={s.previousNamesLabel + ' ' + s.body}>
      <span onMouseEnter={onMouseEnter} onMouseLeave={onMouseLeave} onClick={onClick}><span className={'icon-pastname ' + s.icon} /> Previous Names</span>
    </p>
    {tooltipOpen && <div style={{ height: requiredHeight }} className={s.previousNamesToolTip + ' truncate'}>{store.previousNames.map((v, i) => <p key={i} className={s.previousName}>{v}</p>)}</div>}
  </div>
}

export default PreviousUsernames;