import dayjs from "../../../lib/dayjs";
import { createUseStyles } from "react-jss";
import { getStatusText } from "../lib/getStatusText";
import PlayerHeadshot from "../../playerHeadshot";
import TradeStore from "../stores/tradeStore";
import AuthenticationStore from "../../../stores/authentication";

const useStyles = createUseStyles({
  row: {},
  td: {
    paddingTop: '10px',
    paddingBottom: '6px',
    paddingLeft: '5px',
    borderBottom: '1px solid #e3e3e3',
  },
  block: {
    display: 'inline-block',
  },
  image: {
    borderRadius: '50%',
    border: '1px solid #c3c3c3',
    maxWidth: '30px',
  },
  senderName: {
    position: 'relative',
    top: '-10px',
    marginBottom: 0,
    left: '4px'
  },
  viewDetails: {
    cursor: 'pointer',
  },
  imageBorder: {
    borderRadius: '100%',
    overflow: 'hidden',
  },
});

const TradeEntry = props => {
  const trades = TradeStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const sender = props.user;

  return <tr className={s.row}>
    <td className={s.td}>
      {dayjs(props.created).format('M/D/YY')}
    </td>
    <td className={s.td}>
      {dayjs(props.expiration).format('M/D/YY')}
    </td>
    <td className={s.td}>
      <div className={s.block + ' ' + s.imageBorder}>
        <div className={s.image}>
          <PlayerHeadshot id={sender.id} name={sender.name}></PlayerHeadshot>
        </div>
      </div>
      <div className={s.block}>
        <p className={s.senderName}>{sender.name}</p>
      </div>
    </td>
    <td className={s.td}>
      {getStatusText(props, trades.tradeType, auth.userId)}
    </td>
    <td className={s.td}>
      <p className={`mb-0 ${s.viewDetails}`} onClick={() => {
        trades.setSelectedTrade(props);
      }}>View Details</p>
    </td>
  </tr>
}

export default TradeEntry;