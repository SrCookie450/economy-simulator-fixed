// @ts-nocheck
import React from "react";
import getFlag from "../../../lib/getFlag";
import OldVerticalTabs from "../../oldVerticalTabs"
import MoneyPageStore from "../stores/moneyPageStore";
import CurrencyExchange from "./currencyExchange";
import MySummaryTable from "./mySummaryTable";
import MyTradesTable from "./myTradesTable";
import MyTransactionsTable from "./myTransactionsTable";

const Bar = props => {
  const store = MoneyPageStore.useContainer();
  const options = [
    {
      name: 'My Transactions',
      element: <MyTransactionsTable creatorType='User'></MyTransactionsTable>,
    },
    {
      name: 'Summary',
      element: <MySummaryTable></MySummaryTable>,
    },
    {
      name: 'Trade Currency',
      element: <CurrencyExchange></CurrencyExchange>,
    },
    {
      name: 'Trade Items',
      element: <MyTradesTable></MyTradesTable>,
    },
  ].filter(v => !!v);
  if (getFlag('moneyPagePromotionTabVisible', false)) {
    options.push({
      name: 'Promotion',
      element: null,
    });
  }

  return <div className='row'>
    <div className='col-12'>
      <OldVerticalTabs
        default={store.tab}
        onChange={(newTab) => {
          store.setTab(newTab.name);
          const newUrl = store.getUrl(newTab.name);
          if (window.location.pathname !== newUrl) {
            window.location.href = newUrl;
          }
        }}
        options={options}></OldVerticalTabs>
    </div>
  </div>
}

export default Bar;