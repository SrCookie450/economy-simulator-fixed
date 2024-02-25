import React from "react";
import MyAccount from "../../components/myAccount2016";
import MyAccountStore from "../../components/myAccount2016/stores/myAccountStore";
import Theme2016 from "../../components/theme2016";
import getFlag from "../../lib/getFlag";

const MyAccountPage = () => {
  if (getFlag('myAccountPage2016Enabled', false)) {
    return <Theme2016>
      <MyAccountStore.Provider>
        <MyAccount></MyAccount>
      </MyAccountStore.Provider>
    </Theme2016>
  }
  return <p className='text-center'>Flag myAccountPage2016Enabled is false, but the old settings page is not implemented.</p>;
}

export default MyAccountPage;