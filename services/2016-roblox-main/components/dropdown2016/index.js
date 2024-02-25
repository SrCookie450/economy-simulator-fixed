import { useState } from "react";
import { createUseStyles } from "react-jss";
import useCardStyles from "../userProfile/styles/card";

const useStyles = createUseStyles({
  wrapper: {
    marginTop: '-20px',
    cursor: 'pointer',
    userSelect: 'none',
    '&:hover': {
    },
  },
  dropdown: {
    width: '125px',
    background: 'white',
    position: 'absolute',
    right: '-20px',
    borderRadius: '2px',
  },
  dropdownEntry: {
    width: '100%',
    '&:hover': {
      background: '#e3e3e3',
    },
  },
  dropdownText: {
    marginBottom: 0,
    fontSize: '16px',
    padding: '8px 10px',
    color: 'rgb(33, 37, 41)',
  },
  dropdownDots: {
    letterSpacing: '3px',
    fontWeight: 100,
  },
});

/**
 * Dropdown
 * @param {{options: {url?: string; name: string; onClick?: (e: any) => void}[]}} props 
 * @returns 
 */
const Dropdown2016 = props => {
  const [isOpen, setIsOpen] = useState(false);
  const cardStyles = useCardStyles();
  const s = useStyles();
  return <div className={s.wrapper}>
    <p className={'mb-0 ' + s.dropdownDots} onClick={() => {
      setIsOpen(!isOpen);
    }}>...</p>
    {
      isOpen && <div className={s.dropdown + ' ' + cardStyles.card}>
        {
          props.options.map(v => {
            return <a href={v.url || '#'} onClick={v.onClick}>
              <div className={s.dropdownEntry} key={v.name}>
                <p className={s.dropdownText}>{v.name}</p>
              </div>
            </a>
          })
        }
      </div>
    }
  </div>
}

export default Dropdown2016;