import styles from './getCookie.module.css';
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";

const GetCookie = props => {
  const {setVisible} = props;
  const btn = useButtonStyles();

  return <div className={styles.modal}>
    <div className='container'>
      <div className='row'>
        <div className='col-12 col-lg-8 offset-lg-2'>
          <div className='card card-body'>
            <h3 className='fw-bold'>Login with .ROBLOSECURITY Cookie</h3>
            <p>In order to login, you will have to use your Roblox cookie. You can get your Roblox cookie through a desktop web browser such as Google Chrome, Mozilla Firefox, Safari, etc. <span className='fw-bold'>You cannot get your Roblox cookie through a mobile web browser, you must use a desktop device.</span></p>
            <ol>
              <li>Open up a new tab and visit <a href='https://www.roblox.com/' target='_blank'>roblox.com</a>. You will need to be logged in for this to work.</li>
              <li>Next, open up inspect element. You can usually do this by right clicking on the roblox page and clicking "Inspect". You can also press "CTRL", "SHIFT", and "I" at the same time on your keyboard.</li>
              <li>Now you will have to find your cookie. You should see a list of buttons at the top of the inspect element menu: "Elements", "Console", "Sources", "Network", etc. If you see the "Application" button, click on it, otherwise click the two arrows pointing to the right and select "Application" from the dropdown.</li>
              <li>A menu should open up. On the left side, you should see a heading titled "Storage". Look for the "Cookies" section, then click on the arrow pointing next to it. <span className='fw-bold'>Note: In FireFox, the tab is called "Storage" instead of "Application".</span></li>
              <li>Click on "https://www.roblox.com". You should now be greeted with a table containing a ton of stuff. Look for one with the name ".ROBLOSECURITY". <span className='fw-bold'>If you can't see the full names of your cookies, you can increase the width of the browser window or the inspect element window to see their full names.</span></li>
              <li>Once you've found the ROBLOSECURITY cookie, double click on the value and copy it. The value should start with something like "WARNING_DO_NOT_SHARE". <span className='fw-bold'>Note: you should take the advice seriously: do not share your cookie with anyone, or else they can steal your account.</span> </li>
              <li>Finally, dismiss this window and paste the value into the box, then click "Login". If your cookie was copied correctly, it should work.</li>
            </ol>

            <ActionButton label='Dismiss' className={btn.cancelButton + ' w-auto'} onClick={() => {
              setVisible(false);
            }} />
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default GetCookie;