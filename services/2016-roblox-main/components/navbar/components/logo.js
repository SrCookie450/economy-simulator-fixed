import { createUseStyles } from "react-jss";
import NavigationStore from "../../../stores/navigation";

const useLogoStyles = createUseStyles({
  imgDesktop: {
    width: '118px',
    height: '30px',
    backgroundImage: `url(/img/roblox_logo.svg)`,
    backgroundSize: '118px 30px',
    display: 'none',
    '@media(min-width: 1301px)': {
      display: 'block',
    },
  },
  imgMobile: {
    backgroundImage: `url(/img/logo_R.svg)`,
    width: '30px',
    height: '30px',
    display: 'block',
    backgroundSize: '30px',
    '@media(min-width: 1301px)': {
      display: 'none',
    },
  },
  imgMobileWrapper: {
    marginLeft: '40px',
  },
  col: {
    maxWidth: '140px',
  },
  openSideNavMobile: {
    display: 'none',
    '@media(max-width: 1300px)': {
      display: 'block',
      float: 'left',
      height: '30px',
      width: '30px',
      cursor: 'pointer',
    },
  },
});
const Logo = () => {
  const s = useLogoStyles();
  const navStore = NavigationStore.useContainer();

  return <div className={`${s.col} col-2 col-lg-2`}>
    <div className={s.openSideNavMobile + ' icon-menu'} onClick={() => {
      navStore.setIsSidebarOpen(!navStore.isSidebarOpen);
    }}></div>
    <div className={s.imgDesktop}></div>
    <div className={s.imgMobileWrapper}>
      <div className={s.imgMobile}></div>
    </div>
  </div>
}

export default Logo;