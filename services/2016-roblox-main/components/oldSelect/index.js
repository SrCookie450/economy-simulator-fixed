import { forwardRef, useRef } from "react";
import { createUseStyles } from "react-jss";

const useInputStyles = createUseStyles({
  select: {
    width: '100%',
    padding: '2px',
    border: '1px solid #a7a7a7',
    paddingLeft: '2px',
    appearance: 'none',
  },
  col: {
    padding: 0,
    paddingLeft: '2px',
  },
  caret: {
    position: 'absolute',
    marginLeft: '-21px',
    fontSize: '8px',
    transform: 'rotate(90deg)',
    marginTop: '6px',
    border: '1px solid black',
    paddingLeft: '3px',
    paddingRight: '1px',
    background: 'linear-gradient(90deg, rgba(224,224,224,1) 0%, rgba(255,255,255,1) 100%)',
  },
});

/**
 * Old select component
 * @param {{onChange: (newVal: string) => void; disabled?: boolean; options: {key: string; value: string;}[]}} props 
 * @returns 
 */
const OldSelect = props => {
  const s = useInputStyles();
  return <>
    <select disabled={props.disabled} className={s.select} onChange={(v) => {
      props.onChange(v.currentTarget.value);
    }}>
      {
        props.options.map((v, i) => {
          return <option key={i} value={v.key}>{v.value}</option>
        })
      }
    </select>
    <span className={s.caret}>►</span>
  </>
}

export default OldSelect;