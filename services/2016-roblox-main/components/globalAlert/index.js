import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getAlert } from "../../services/api";

const useStyles = createUseStyles({
  alertBg: {
    background: '#F68802',
  },
  alertText: {
    color: '#fff',
    textAlign: 'center',
    padding: '12px 0',
    fontSize: '18px',
    fontWeight: 'bold',

  },
  alertLink: {
    color: '#fff',
    '&:hover': {
      textDecoration: 'underline',
    },
  },
  fakeAlert: {
    width: '100%',
    position: 'relative',
    height: '40px',
  }
});

const GlobalAlert = props => {
  const s = useStyles();
  const [alert, setAlert] = useState(null);
  useEffect(() => {
    // Always cache alert for 30 seconds
    const alertStorageKey = 'alert1';
    const existingAlert = sessionStorage.getItem(alertStorageKey);
    if (existingAlert !== null) {
      try {
        const parsed = JSON.parse(existingAlert);
        const expires = parsed.CreatedAt + (30*1000);
        if (expires > Date.now()) {
          // It is still OK
          setAlert(parsed);
          return;
        }
      }catch(e) {
        // Nothing to do here
      }
    }
    getAlert().then(msg => {
      msg.CreatedAt = Date.now();
      sessionStorage.setItem(alertStorageKey, JSON.stringify(msg));
      setAlert(msg);
    }).catch(e => {
      console.error('[error] could not fetch global alert:',e);
    })
  }, []);

  if (alert === null || !alert.IsVisible) {
    return <div className={s.fakeAlert}></div>;
  }

  return <div className={s.alertBg}>
    <p className={s.alertText}>
      {alert.LinkUrl ? <a className={s.alertLink} href={alert.LinkUrl}>{alert.Text}</a> : alert.Text}
    </p>
  </div>
}

export default GlobalAlert;