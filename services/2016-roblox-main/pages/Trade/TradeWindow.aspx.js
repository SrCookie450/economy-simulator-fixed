import TradeWindow from "../../components/tradeWindow";
import TradeWindowStore from "../../components/tradeWindow/stores/tradeWindowStore";

const TradeWindowPage = props => {
  return <TradeWindowStore.Provider>
    <TradeWindow></TradeWindow>
  </TradeWindowStore.Provider>
}

export default TradeWindowPage;