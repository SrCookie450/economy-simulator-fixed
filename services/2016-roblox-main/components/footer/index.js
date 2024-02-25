import { createUseStyles } from "react-jss";

const useFooterStyles = createUseStyles({
  text: {
    color: '#B8B8B8',
    fontSize: '12px',
    fontWeight: '400',
  },
  link: {
    fontSize: '21px',
    textAlign: 'center',
    fontWeight: '300',
    textDecoration: 'none',
    '&:hover': {
      color: '#191919',
    },
  },
  footer: {
    background: '#ffffff',
  },
  footerContainer: {
    paddingTop: '5px',
    paddingBottom: '20px',
  },
});

const footerLinks = {
  '/about-us': 'About Us',
  '/jobs': 'Jobs',
  '/info/blog': 'Blog',
  '/privacy': 'Privacy',
  '/help': 'Help',
};

const Footer = props => {
  const s = useFooterStyles();
  return <footer className={s.footer}>
    <div className={'container mt-4 mb-0 ' + s.footerContainer}>
      <div className='row'>
        {
          Object.getOwnPropertyNames(footerLinks).map(v => {
            return <div className='col-2 mb-2' key={v}>
              <h2 className={s.text + ' ' + s.link}>
                <a className={s.text + ' ' + s.link} href={v}>{footerLinks[v]}</a>
              </h2>
            </div>
          })
        }
        <div className='col-12 col-lg-10 offset-lg-1'>
          <p className={`${s.text}`}>
            ROBLOX, "Online Building Toy", characters, logos, names, and all related indicia are trademarks of <a href="https://corp.roblox.com"> ROBLOX Corporation</a>, ©2016. Patents pending. ROBLOX is not sponsored, authorized or endorsed by any producer of plastic building bricks, including The LEGO Group, MEGA Brands, and K'Nex, and no resemblance to the products of these companies is intended. Use of this site signifies your acceptance of the <a href='/terms-and-conditions'>Terms and Conditions</a>.
          </p>
        </div>
      </div>
    </div>
  </footer>
}

export default Footer;