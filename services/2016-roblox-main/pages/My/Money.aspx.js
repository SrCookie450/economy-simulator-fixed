import React from "react";
import MyMoney from "../../components/myMoney";
import MoneyPageStore from "../../components/myMoney/stores/moneyPageStore";

const MyMoneyPage = props => {
  return <MoneyPageStore.Provider>
    <MyMoney type='My Transactions'></MyMoney>
  </MoneyPageStore.Provider>
}

export default MyMoneyPage;