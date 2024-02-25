import React, { useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { abbreviateNumber } from "../../../lib/numberUtils";
import { logout } from "../../../services/auth";
import AuthenticationStore from "../../../stores/authentication";
import Link from "../../link";

const useDropdownStyles = createUseStyles({
  wrapper: {
    width: '125px',
    position: 'absolute',
    top: '45px',
    right: '10px',
    boxShadow: '0 -5px 20px rgba(25,25,25,0.15)',
    userSelect: 'none',
    background: 'white',
  },
  text: {
    padding: '10px',
    marginBottom: 0,
    fontSize: '16px',
    '&:hover': {
      background: '#eaeaea',
      borderLeft: '4px solid #0074BD',
    },
    '&:hover > a': {
      marginLeft: '-4px',
    },
  }
});

const SettingsDropdown = props => {
  const authStore = AuthenticationStore.useContainer();
  const s = useDropdownStyles();
  return <div className={s.wrapper}>
    <p className={`${s.text}`}>
      <Link href='/My/Account'><a className='text-dark'>Settings</a></Link>
    </p>
    <p className={`${s.text}`}>
      <Link href='/help'><a className='text-dark'>Help</a></Link>
    </p>
    <p className={`${s.text}`}>
      <a onClick={(e) => {
        e.preventDefault();
        logout().then(() => {
          window.location.reload();
        })
      }} className='text-dark'>Logout</a>
    </p>
  </div>
}


const useLoginAreaStyles = createUseStyles({
  text: {
    color: 'white',
    fontWeight: 400,
    fontSize: '16px',
    borderBottom: 0,
    marginTop: '2px',
    marginBottom: 0,
    textAlign: 'right',
    whiteSpace: 'nowrap',
    display: 'inline',
  },
  link: {
    color: 'white',
    textDecoration: 'none',
    padding: '4px 8px',
    '&:hover': {
      color: 'white',
      background: 'rgba(25,25,25,0.1)',
      cursor: 'pointer',
      borderRadius: '4px',
    },
  },
  settingsIcon: {
    float: 'right',
  },
  linkContainer: {
  },
  linkContainerCol: {
    maxWidth: '250px',
    float: 'right',
  },
  robuxText: {
    marginRight: '20px',
    marginLeft: '5px',
  },
});

const LoggedInArea = props => {
  const s = useLoginAreaStyles();
  const authStore = AuthenticationStore.useContainer();
  const [settingsOpen, setSettingsOpen] = useState(false);

  if (authStore.robux === null || authStore.tix === null) return null;
  return <div className={`${s.linkContainerCol} `}>
    <div className='row'>
      <div className={`col-12 ${s.linkContainer}`}>
        <p className={s.text} title={authStore.robux.toLocaleString()}>
          <Link href='/My/Money.aspx'>
            <a>
              <span className='icon-nav-robux'/>
            </a>
          </Link>
        </p>
        <p className={s.text + ' ' + s.robuxText} title={authStore.robux.toLocaleString()}>
          <span>{abbreviateNumber(authStore.robux)}</span>
        </p>
        <>
          <p className={s.text} title={authStore.tix.toLocaleString()}>
            <Link href='/My/Money.aspx'>
              <a>
                <span className='icon-nav-tix'/>
              </a>
            </Link>
          </p>
          <p className={s.text + ' ' + s.robuxText}>
            <span title={authStore.tix.toLocaleString()}>{abbreviateNumber(authStore.tix)}</span>
          </p>
        </>
        <p className={s.text}>
          <a onClick={(e) => {
            e.preventDefault();
            setSettingsOpen(!settingsOpen);
          }}>
            <span className={`icon-nav-settings ${s.settingsIcon}`} id="nav-settings"/>
          </a>
        </p>
        {settingsOpen && <SettingsDropdown/>}
      </div>
    </div>
  </div>
}

export default LoggedInArea;