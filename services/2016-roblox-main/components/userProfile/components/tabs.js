import { createUseStyles } from "react-jss";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";

const useTabEntryStyles = createUseStyles({
  entry: {
    textAlign: 'center',
    marginBottom: 0,
    fontSize: '18px',
    paddingBottom: '8px',
    paddingTop: '8px',
    cursor: 'pointer',
    '&:hover': {
      boxShadow: '0 -4px 0 0 #00a2ff inset',
    },
  },
  entryActive: {
    boxShadow: '0 -4px 0 0 #00a2ff inset',
  },
});

const TabEntry = (props) => {
  const s = useTabEntryStyles();
  const store = UserProfileStore.useContainer();

  return <p className={s.entry + ' ' + (store.tab === props.children ? s.entryActive : '')} onClick={() => {
    store.setTab(props.children);
  }}>{props.children}</p>
}

const useTabStyles = createUseStyles({

});

const Tabs = props => {
  const cardStyles = useCardStyles();
  const s = useTabStyles();
  const store = UserProfileStore.useContainer();

  return <div className='row mt-4'>
    <div className='col-12'>
      <div className={cardStyles.card}>
        <div className='row'>
          <div className='col-6 pe-0'>
            <TabEntry>About</TabEntry>
          </div>
          <div className='col-6 ps-0'>
            <TabEntry>Creations</TabEntry>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default Tabs;