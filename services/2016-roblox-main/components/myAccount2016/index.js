import React, { useEffect } from "react";
import { createUseStyles } from "react-jss";
import { getInventoryPrivacy, getTradePrivacy, getTradeValue } from "../../services/accountSettings";
import { getUserInfo } from "../../services/users";
import AuthenticationStore from "../../stores/authentication";
import AccountInfo from "./components/accountInfo";
import ModalHandler from "./components/modalHandler";
import PrivacySettings from "./components/privacySettings";
import Tabs from "./components/tabs";
import MyAccountStore from "./stores/myAccountStore";
import Security from "./components/security";

const useStyles = createUseStyles({
  settingsRow: {
    background: '#e3e3e3',
  },
})

const MyAccount = props => {
  const s = useStyles();

  const store = MyAccountStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  useEffect(() => {
    if (auth.isPending) return
    getUserInfo({
      userId: auth.userId
    }).then((d) => {
      store.setDescription(d.description);
    });

    getTradePrivacy().then(store.setTradePrivacy);
    getInventoryPrivacy().then(store.setInventoryPrivacy);
    getTradeValue().then(store.setTradeFilter);
  }, [auth.userId, auth.isPending]);
  if (auth.isPending || !auth.userId) return null;
  return <div className='container'>
    <ModalHandler></ModalHandler>
    <div className={'row pb-2 ' + s.settingsRow}>
      <div className='col-12'>
        <h1 className='mt-0 mb-0'>My Settings</h1>
      </div>
      <div className='col-12'>
        <Tabs></Tabs>
      </div>
      <div className='col-12'>
        {store.tab === 'Account Info' ? <AccountInfo/> : null}
        {store.tab === 'Security' ? <Security /> : null}
        {store.tab === 'Privacy' ? <PrivacySettings/> : null}
      </div>
    </div>
  </div>
}

export default MyAccount;