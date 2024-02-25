import { createUseStyles } from "react-jss";
import {useEffect, useState} from "react";
import {getMembershipType} from "../../services/users";

const useStyles = createUseStyles({
  overlay: {
    width: '66px',
    height: '19px',
    marginTop: '-40px',
    zIndex: 2,
  },
});

const iconsMap = {
  1: `/img/overlay_bcOnly.png`,
  2: `/img/overlay_tbcOnly.png`,
  3: `/img/overlay_obcOnly.png`,
  4: `/img/overlay_bcOnly.png`,
};

const Icon = props => {
  const s= useStyles();
  return <img className={s.overlay} src={iconsMap[props.type]} />;
}

let statusCache = {}
let pendingCache = {}
const BcOverlay = props => {
  const {id} = props;
  const [type, setType] = useState(0);

  useEffect(() => {
    if (typeof window === undefined)
      return;

    if (statusCache[id] !== undefined) {
      setType(statusCache[id]);
      return;
    }
    if (pendingCache[id]) {
      pendingCache[id].push((status) => {
        setType(status);
      });
      return;
    }
    setType(0);
    pendingCache[id] = [];
    getMembershipType({userId: id}).then(d => {
      setType(d);
      pendingCache[id].forEach(v => v(d));
      delete pendingCache[id];
    })
  }, [id]);

  if (type === 0) {
    return null;
  }

  return <Icon type={type} />
}

export default BcOverlay;