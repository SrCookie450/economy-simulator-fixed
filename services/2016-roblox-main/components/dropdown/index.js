import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss"

const useDropdownStyles = createUseStyles({
  wrapper: {
    border: '1px solid #565655',
    width: '100%',
  },
  heading: {
    background: 'linear-gradient(0deg, rgba(86,86,85,1) 0%, rgba(128,127,127,1) 100%)',
    padding: '2px',
  },
  mainBody: {
    backgroundColor: '#efefef',
  },
  itemDiv: {
    paddingLeft: '8px',
    paddingRight: '8px',
    paddingTop: '4px',
    paddingBottom: '4px',
    cursor: 'pointer',
    '&:hover': {
      background: '#d8d8d8',
    },
  },
  caret: {
    float: 'right',
    color: '#666666',
    paddingTop: '2px',
    fontSize: '12px',
  },
  leftMenu: {
    position: 'absolute',
    backgroundColor: '#efefef',
    border: '1px solid #565655',
    minWidth: '150px',
    '@media(max-width: 800px)': {
      marginLeft: '20%!important',
      width: '80%',
      boxShadow: '5px 5px 24px black',
    }
  },
  leftMenuTitle: {
    fontSize: '14px',
    fontWeight: 700,
    paddingTop: '8px',
    paddingLeft: '4px',
  },
  separator: {
    borderBottom: '1px solid #c3c3c3',
    width: '100%',
  },
});
/**
 * Ancient dropdown used for catalog page + other stuff
 * @param {{title: JSX.Element; onClick: (e: any, data: any) => void; items: {name: string; clickData: any; children?: {title: string; children?: {name: string; clickData: any;}[]}}[]}} props
 */
const Dropdown = props => {
  const s = useDropdownStyles();
  const [leftMenu, setLeftMenu] = useState(null);
  const wrapperRef = useRef(null);
  const leftMenuRef = useRef(null);

  const leftMenuStyles = {
    marginLeft: (wrapperRef.current?.clientWidth || 0) + 'px',
    zIndex: 11
  };

  return <div onMouseLeave={() => {
    setLeftMenu(null);
  }}>
    <div className={s.wrapper} ref={wrapperRef}>
      <div className={s.heading}>
        {props.title}
      </div>
      {leftMenu && <div ref={leftMenuRef} className={s.leftMenu} style={leftMenuStyles}>
        <h2 className={s.leftMenuTitle}>{leftMenu.title}</h2>
        <div className={s.separator}></div>
        {
          leftMenu.children.map(v => {
            return <div key={v.name} className={s.itemDiv}>
              <p className={`mb-0 mt-0`} onClick={(e) => {
                props.onClick(e, v.clickData);
              }}>{v.name}</p>
            </div>
          })
        }
      </div>}
      <div className={s.mainBody}>
        {
          props.items.map((v, i) => {
            if (v.name === 'separator') {
              return <div key={'separator' + i} className={s.separator}></div>
            }
            return <div key={v.name} className={s.itemDiv} onMouseEnter={() => {
              if (v.children) {
                setLeftMenu(v.children);
              }
            }}>
              <p className={`mb-0 mt-0`} onClick={(e) => {
                props.onClick(e, v.clickData);
              }}>{v.name} {v.children && <span className={s.caret}>►</span>}</p>
            </div>
          })
        }
      </div>
    </div>
  </div>
}

export default Dropdown;