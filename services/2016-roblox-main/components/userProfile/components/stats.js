import dayjs from "dayjs";
import { createUseStyles } from "react-jss";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import Subtitle from "./subtitle";

const useStatisticEntryStyles = createUseStyles({
  label: {
    fontWeight: 400,
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    color: '#B8B8B8',
    marginBottom: 0,
    textAlign: 'center',
    fontSize: '16px',
  },
  value: {
    marginTop: '5px',
    textAlign: 'center',
    fontSize: '18px',
  },
});

const StatisticEntry = props => {
  const s = useStatisticEntryStyles();
  return <div className='col-4'>
    <p className={s.label}>{props.label}</p>
    <p className={s.value}>{props.value}</p>
  </div>
}

const useStatisticStyles = createUseStyles({})

const Statistics = props => {
  const store = UserProfileStore.useContainer();
  const cardStyles = useCardStyles();
  return <div className='row'>
    <div className='col-12'>
      <Subtitle>Statistics</Subtitle>
    </div>
    <div className='col-12'>
      <div className={cardStyles.card}>
        <div className='row pt-4 pb-2'>
          <StatisticEntry label='Join Date' value={dayjs(store.userInfo.created).format('M/DD/YYYY')}></StatisticEntry>
          <StatisticEntry label='Place Visits' value={(0).toLocaleString()}></StatisticEntry>
          <StatisticEntry label='Forum Posts' value={store.userInfo.postCount}></StatisticEntry>
        </div>
      </div>
    </div>
  </div>
}

export default Statistics;