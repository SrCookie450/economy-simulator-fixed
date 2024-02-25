import React from "react";
import Tabs2016 from "../../tabs2016";
import MyAccountStore from "../stores/myAccountStore";

const Tabs = props => {
  const store = MyAccountStore.useContainer();
  return <Tabs2016
    onChange={(newTab) => {
      store.setTab(newTab);
    }}
    options={[
      'Account Info',
      'Security',
      'Social',
      'Privacy',
      'Billing',
    ]}></Tabs2016>
}

export default Tabs;