import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import useCardStyles from "../userProfile/styles/card";

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

  return <p className={s.entry + ' ' + (props.tab === props.children ? s.entryActive : '')} onClick={() => {
    props.setTab(props.children);
  }}>{props.children}</p>
}

const useStyles = createUseStyles({
  row: {
    paddingLeft: '10px',
    paddingRight: '10px',
  },
})

const Tabs2016 = props => {
  const s = useStyles();
  const cardStyles = useCardStyles();
  const [tab, setTab] = useState(props.options[0]);

  return <div className='row mt-4'>
    <div className='col-12'>
      <div className={cardStyles.card}>
        <div className={'row ' + s.row}>
          {
            props.options.map(v => {
              return <div className='col ps-0 pe-0' key={v}>
                <TabEntry tab={tab} setTab={() => {
                  props.onChange(v);
                  setTab(v);
                }}>{v}</TabEntry>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}

export default Tabs2016;