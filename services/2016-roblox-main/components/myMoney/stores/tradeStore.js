import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import { getMyTrades } from "../../../services/trades";

const TradeStore = createContainer(() => {
  const [feedback, setFeedback] = useState(null);
  const [trades, setTrades] = useState(null);
  const [tradeType, setTradeType] = useState('inbound');
  const [cursor, setCursor] = useState(null);
  const [selectedTrade, setSelectedTrade] = useState(null);
  const [refresh, setRefresh] = useState(null);

  useEffect(() => {
    setFeedback(null);
    setTrades(null);
    setCursor(null);
    setSelectedTrade(null);
    getMyTrades({
      cursor: null,
      tradeType,
    }).then(setTrades)
  }, [tradeType, refresh]);

  return {
    trades,
    setTrades,

    tradeType,
    setTradeType,

    selectedTrade,
    setSelectedTrade,

    feedback,
    setFeedback,

    refershTrades: () => {
      getMyTrades({
        cursor: null,
        tradeType,
      }).then(setTrades)
    },
  }
});

export default TradeStore;