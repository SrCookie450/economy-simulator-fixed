import ExchangeStore from "./stores/exchangeStore";
import PositionsTab from "./components/positionsTab";

const Container = props => {
  const store = ExchangeStore.useContainer();

  if (store.tab === 'My R$ Positions' || store.tab === 'My TX Positions') {
    return <PositionsTab currency={store.tab === 'My R$ Positions' ? 1 : 2} />
  }
  return null;
}

export default Container;