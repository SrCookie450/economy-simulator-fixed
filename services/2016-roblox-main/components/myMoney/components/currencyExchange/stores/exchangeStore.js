import {useEffect, useState} from "react";
import { createContainer } from "unstated-next";
import {getMarketActivity} from "../../../../../services/economy";


const ExchangeStore = createContainer(() => {
  const [tab, setTab] = useState('My R$ Positions');
  const [newPositionVisible, setNewPositionVisible] = useState(false);
  const [statistics, setStatistics] = useState(null);

  useEffect(() => {
    getMarketActivity().then(d => {
      setStatistics(d);
    })
  }, []);

  return {
    tab,
    setTab,

    newPositionVisible,
    setNewPositionVisible,

    statistics,
    setStatistics,
  }
});

export default ExchangeStore;