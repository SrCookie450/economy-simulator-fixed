import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../stores/authentication";
import NavigationStore from "../../stores/navigation";
import LinkEntry from "./components/linkEntry";

const useNavSideBarStyles = createUseStyles({
  container: {
    position: 'fixed',
    top: 0,
    left: 0,
    zIndex: 999,
  },
  card: {
    width: '175px',
    background: '#f2f2f2',
    height: '100vh',
    paddingLeft: '10px',
    paddingRight: '10px',
  },
  username: {
    fontSize: '18px',
    paddingTop: '8px',
    paddingBottom: '5px',
    marginBottom: 0,
    color: '#1e1e1f',
  },
  divider: {
    borderBottom: '2px solid #c3c3c3',
    height: '2px',
    width: '100%',
  },
  upgradeNowButton: {
    marginTop: '10px',
    background: '#01a2fd',
    fontSize: '15px',
    fontWeight: 500,
    width: '100%',
    paddingTop: '8px',
    paddingBottom: '8px',
    textAlign: 'center',
    color: 'white',
    borderRadius: '4px',
    '&:hover': {
      background: '#3ab8ff',
    },
  },
});

const NavSideBar = props => {
  const authStore = AuthenticationStore.useContainer();
  const navStore = NavigationStore.useContainer();
  const mainNavBarRef = props.mainNavBarRef;
  const [dimensions, setDimensions] = useState({
    height: window.innerHeight,
    width: window.innerWidth
  })
  const s = useNavSideBarStyles();
  useEffect(() => {
    window.addEventListener('resize', () => {
      setDimensions({
        height: window.innerHeight,
        width: window.innerWidth
      });
    });
  }, []);
  const paddingTop = mainNavBarRef.current && mainNavBarRef.current.clientHeight + 'px' || 0;

  if (navStore.isSidebarOpen === false && dimensions.width <= 1300) {
    return null;
  }

  return <div className={s.container}>
    <div className={s.card} style={{ paddingTop: paddingTop }}>
      <p className={s.username}>{authStore.username}</p>
      <div className={s.divider} />
      <LinkEntry name='Home' url='/home' icon='icon-nav-home' />
      <LinkEntry name='Profile' url={'/users/' + authStore.userId + '/profile'} icon='icon-nav-profile' />
      <LinkEntry name='Messages' url='/My/Messages' icon='icon-nav-message' count={authStore.notificationCount.messages} />
      <LinkEntry name='Friends' url={'/users/' + authStore.userId + '/friends'} icon='icon-nav-friends' count={authStore.notificationCount.friendRequests} />
      <LinkEntry name='Character' url='/My/Character.aspx' icon='icon-nav-charactercustomizer' />
      <LinkEntry name='Inventory' url={'/users/' + authStore.userId + '/inventory'} icon='icon-nav-inventory' />
      <LinkEntry name='Trade' url='/My/Trades.aspx' icon='icon-nav-trade' count={authStore.notificationCount.trades} />
      <LinkEntry name='Groups' url='/My/Groups.aspx' icon='icon-nav-group' />
      <LinkEntry name='Forums' url='/Forum/Default.aspx' icon='icon-nav-forum' />
      <LinkEntry name='Blog' url='/info/blog' icon='icon-nav-blog' />
      <a href='/BuildersClub/Upgrade.ashx'><p className={s.upgradeNowButton}>Upgrade Now</p></a>
    </div>
  </div>
}

export default NavSideBar;