import { useState } from "react";
import { createContainer } from "unstated-next";

const MoneyPageStore = createContainer(() => {
  const [tab, setTab] = useState(null);

  return {
    tab,
    setTab,
    getUrl: (tab) => {
      switch (tab) {
        case 'Trade Items':
          return '/My/Trades.aspx';
        default:
          return '/My/Money.aspx'
      }
    }
  }
});

export default MoneyPageStore;