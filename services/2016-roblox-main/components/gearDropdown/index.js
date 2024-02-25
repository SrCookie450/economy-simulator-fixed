import { useState } from "react";
import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  box: {
    width: '40px',
    float: 'right',
    border: '1px solid #777777',
    background: 'linear-gradient(0deg, rgba(224,224,224,1) 0%, rgba(255,255,255,1) 100%)',
    '&:hover': {
      background: 'linear-gradient(0deg, rgba(203,216,255,1) 0%, rgba(255,255,255,1) 100%)',
    },
    cursor: 'pointer',
    userSelect: 'none',
  },
  boxOpen: {
    background: 'rgb(224,224,224)',
  },
  gear: {
    backgroundImage: `url("/img/Unofficial/settings-gear.png")`,
    height: '12px',
    width: '12px',
    display: 'block',
    backgroundSize: '12px 12px',
    backgroundPosition: '0 0',
  },
  gearWrapper: {
    paddingLeft: '5px',
    paddingTop: '5px',
    paddingBottom: '2px',
    display: 'inline-block',
  },
  caretWrapper: {
    display: 'inline-block',
    color: '#666',
    fontSize: '12px',
    paddingLeft: '5px',
    position: 'relative',
    top: '-3px',
    right: '1px',
  },
  caret: {
  },
  boxDropdown: {
    background: 'rgb(224,224,224)',
    position: 'absolute',
    width: '100px',
    top: '24px',
    right: '-13px',
    border: '1px solid #777777',
    zIndex: 99,
  },
  boxDropdownEntry: {
    fontSize: '12px',
    padding: '3px 6px',
    '&:hover': {
      background: '#d8d8d8',
    },
    color: 'black',
    fontFamily: 'Arial,Helvetica,sans-serif',
  },
  container: {
    position: 'relative',
  },
});

/**
 * Basic gear dropdown
 * @param {{options: {url?: string; onClick?: (e: any) => void; name: string}[]; boxDropdownRightAmount?: number}} props
 * @returns 
 */
const GearDropdown = props => {
  // const { boxDropdownRightAmount } = props;
  const boxDropdownRightAmount = 0;
  const s = useStyles();
  const [open, setOpen] = useState(false);

  return <div className={s.container}>
    <div className={s.box + ' ' + (open ? s.boxOpen : '')} onClick={() => {
      setOpen(!open);
    }}>
      <div className={s.gearWrapper}>
        <div className={s.gear}></div>
      </div>
      <div className={s.caretWrapper}>
        <div className={s.caret}>
          ▼
        </div>
      </div>
    </div>
    {open && <div className={s.boxDropdown} style={typeof boxDropdownRightAmount !== 'undefined' && { right: boxDropdownRightAmount + 'px' } || undefined}>
      {
        props.options.map((v, i) => {
          if (v.name === 'separator') {
            return <div key={'separator ' + i} className='divider-top'></div>
          }
          return <a key={v.name} href={v.url || '#'} onClick={v.onClick}>
            <p className={`mb-0 ${s.boxDropdownEntry}`}>
              {v.name}
            </p>
          </a>
        })
      }
    </div>}
  </div>
}

export default GearDropdown;