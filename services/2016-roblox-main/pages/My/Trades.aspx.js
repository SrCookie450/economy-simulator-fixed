import React from "react";
import MyMoney from "../../components/myMoney";
import MoneyPageStore from "../../components/myMoney/stores/moneyPageStore";
import TradeStore from "../../components/myMoney/stores/tradeStore";

const MyTradesPage = props => {
  return <MoneyPageStore.Provider>
    <TradeStore.Provider>
      <MyMoney type='Trade Items'></MyMoney>
    </TradeStore.Provider>
  </MoneyPageStore.Provider>
}

export default MyTradesPage;