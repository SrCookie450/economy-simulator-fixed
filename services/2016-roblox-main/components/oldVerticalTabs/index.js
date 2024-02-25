import {useEffect, useState} from "react";
import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../lib/numberUtils";

const useStyles = createUseStyles({
  vTab: {
    display: 'inline-block',
    cursor: 'pointer',
    borderTop: '1px solid #9e9e9e',
    borderLeft: '1px solid #9e9e9e',
    borderRight: '1px solid #9e9e9e',
    marginRight: '4px',
  },
  vTabLabel: {
    fontSize: '16px',
    padding: '10px 5px 8px 5px',
    marginBottom: 0,
    fontWeight: 600,
  },
  vTagSelected: {
  },
  buttonCol: {
    borderBottom: '2px solid #c3c3c3',
  },
  btnBottomSeperator: {
    width: '100%',
    height: '5px',
    background: 'white',
    marginBottom: '-5px',
  },
  vTabUnselected: {
    background: '#d6d6d6',
    paddingTop: '7px',
    '&:hover': {
      background: '#e8e8e8'
    },
    // 9e9e9e
  },
  count: {
    background: '#e0f1fc',
    border: '1px solid #84a5c9',
    paddingLeft: '4px',
    paddingRight: '4px',
  },
});

/**
 * Vertical tabs in old style
 * @param {{options: {name: string; element: JSX.Element; count?: number}[]; onChange?: (arg: {name: string; element: JSX.Element; count?: number;}) => void; default?: string}} props 
 */
const OldVerticalTabs = props => {
  const s = useStyles();
  const { options } = props;
  const [selected, setSelected] = useState(props.default ? options.find(v => v.name === props.default) : options[0]);
  useEffect(() => {
    setSelected(props.default ? options.find(v => v.name === props.default) : options[0]);
  }, [props.default, options]);
  return <div className='row'>
    <div className={`${s.buttonCol} col-12`}>
      {
        options.map(v => {
          const isSelected = v.name === selected.name;
          return <div key={v.name} className={s.vTab} onClick={() => {
            setSelected(v);
            if (props.onChange) {
              props.onChange(v);
            }
          }}>
            <p className={`${!isSelected ? s.vTabUnselected : ''} ${s.vTabLabel}`}>{v.name} {typeof v.count === 'number' ? <span className={s.count}>{abbreviateNumber(v.count)}</span> : null}</p>
            {isSelected && <div className={s.btnBottomSeperator}/>}
          </div>
        })
      }
    </div>
    <div className='col-12'>
      {selected.element}
    </div>
  </div>
}

export default OldVerticalTabs;