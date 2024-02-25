import ActionButton from "../../../../actionButton";
import VerticalSelector from "../../../../verticalSelector";
import ExchangeStore from "../stores/exchangeStore";

const TabSelector = props => {
  const store = ExchangeStore.useContainer();
  return <div className='col-2'>
    <ActionButton label='Trade' onClick={() => {
      store.setNewPositionVisible(true);
    }} />
    <VerticalSelector selected={store.tab} options={[
      {
        name: 'My R$ Positions',
        url: '#',
        onClick: (v) => {
          store.setTab('My R$ Positions')
        },
      },
      {
        name: 'My TX Positions',
        url: '#',
        onClick: (v) => {
          store.setTab('My TX Positions');
        },
      },
    ]} />
  </div>
}

export default TabSelector;