import TabSelector from "./components/tabSelector";
import ExchangeStore from "./stores/exchangeStore";
import Container from "./container";
import UserAdvertisement from "../../../userAdvertisement";
import NewPositionModal from "./components/newPositionModal";

const CurrencyExchange = props => {
  return <div className='row'>
    <div className='col-12 mb-3 mt-3'>
      <div className='row'>
        <ExchangeStore.Provider>
          <NewPositionModal />
          <TabSelector />
          <Container />
        </ExchangeStore.Provider>
        <div className='col-2'>
          <UserAdvertisement type={2} />
        </div>
      </div>
    </div>
  </div>
}

export default CurrencyExchange;