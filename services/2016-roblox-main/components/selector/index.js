import { useRef, useState } from "react";
import { createUseStyles } from "react-jss";

const useSelectorStyles = createUseStyles({
  selectorWrapper: {

  },
  selectorClosed: {
    padding: '10px 15px',
    textAlign: 'left',
    width: '100%',
    color: '#666',
    background: 'white',
    borderRadius: '4px',
    border: '1px solid #c3c3c3',
    fontSize: '16px',
    userSelect: 'none',
    cursor: 'pointer',
    '&:hover': {
      background: '#01a2fd',
      color: '#ffffff',
    },
  },
  selectorOpen: {
    background: '#01a2fd',
    color: '#ffffff',
  },
  selectorCaret: {
    float: 'right',
  },
  selectorMenuOpen: {
    position: 'absolute',
    width: '100%',
    background: 'white',
    zIndex: 3,
  },
  selectOption: {
    padding: '10px 15px',
    marginBottom: 0,
    cursor: 'pointer',
    userSelect: 'none',
    fontSize: '16px',
    '&:hover': {
      boxShadow: '4px 0 0 0 #00a2ff inset',
    },
  },
});

/**
 * 
 * @param {{options: {name: string; value: any}[]; onChange: (v: any) => void; value?: any}} props
 * @returns 
 */
const Selector = props => {
  const s = useSelectorStyles();
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState(() => {
    if (props.value) {
      return props.options.find(v => v.value === props.value)
    }
    return props.options[0]
  });
  const selectorRef = useRef(null);

  return <div className={s.selectorWrapper}>
    <div ref={selectorRef} className={s.selectorClosed + ' ' + (open ? s.selectorOpen : '')} onClick={() => {
      setOpen(!open);
    }}>
      <span>{selected.name}</span>
      <span className={s.selectorCaret}>V</span>
    </div>
    {
      open && selectorRef.current && <div className={s.selectorMenuOpen} style={{ width: selectorRef.current.clientWidth + 'px' }}>
        {
          props.options.map(v => {
            return <p className={s.selectOption} key={v.value} onClick={() => {
              setSelected(v);
              setOpen(false);
              props.onChange(v);
            }}>{v.name}</p>
          })
        }
      </div>
    }
  </div>
}

export default Selector;