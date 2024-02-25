// reference: https://www.youtube.com/watch?v=Fge61b89IVM
import React, { useEffect } from "react";
import { createUseStyles } from "react-jss";
import AdBanner from "../ad/adBanner";
import Bar from "./components/bar";
import MoneyPageStore from "./stores/moneyPageStore";

const useStyles = createUseStyles({
  moneyContainer: {
    backgroundColor: '#fff',
    overflow: 'hidden',
    padding: '2px 4px',
  }
})

const MyMoney = props => {
  const s = useStyles();

  const store = MoneyPageStore.useContainer();
  useEffect(() => {
    // There are two types: 'Trades' and 'Money'
    store.setTab(props.type);
  }, [props.type]);

  if (!store.tab) return null;
  return <div className='container'>
    <AdBanner></AdBanner>
    <div className={s.moneyContainer}>
      <Bar></Bar>
    </div>
  </div>
}

export default MyMoney;