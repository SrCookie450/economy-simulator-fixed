import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { getTradeDetails } from "../../../services/trades";
import AuthenticationStore from "../../../stores/authentication";
import CreatorLink from "../../creatorLink";
import OldModal from "../../oldModal";
import PlayerHeadshot from "../../playerHeadshot";
import TradeStore from "../stores/tradeStore";
import TradeButtons from "./tradeButtons";
import TradeOfferEntry from "./tradeOfferEntry";
import BcOverlay from "../../bcOverlay";

const TradeBelowNameText = props => {
  const state = props.status;
  switch (state) {
    case 'Open':
      return <div>
        <span>Trade with <CreatorLink id={props.user.id} name={props.user.name} type='User'/> has been opened.</span>
        <br/>
        <br/>
        <span className='font-size-12 fw-700 lighten-3 mb-0'>Expires {dayjs(props.expiration).fromNow()}</span>
      </div>
    case 'Pending':
    case 'Expired':
    case 'Finished':
      return <span>Trade with <CreatorLink id={props.user.id} name={props.user.name} type='User'/> is {state}</span>
    default:
      return <span>Trade with <CreatorLink id={props.user.id} name={props.user.name} type='User'/> was {state}!</span>

  }
}

const TradeModal = props => {
  const [details, setDetails] = useState(null);
  const [authenticatedOffer, setAuthenticatedOffer] = useState(null);
  const [otherOffer, setOtherOffer] = useState(null);
  const auth = AuthenticationStore.useContainer();
  const trades = TradeStore.useContainer();
  const trade = trades.selectedTrade;
  const labelGiving = trade.status === 'Open' ? 'ITEMS YOU WILL GIVE' : (trade.status === 'Inactive' || trade.status === 'Declined' || trade.status === 'Countered') ? 'ITEMS YOU WOULD GIVEN' : 'ITEMS YOU GAVE'
  const labelReceiving = trade.status === 'Open' ? 'ITEMS YOU WILL RECEIVE' : (trade.status === 'Inactive' || trade.status === 'Declined' || trade.status === 'Countered') ? 'ITEMS YOU WOULD HAVE RECEIVED' : 'ITEMS YOU RECEIVED'


  useEffect(() => {
    if (auth.userId === null) return null;
    setDetails(null);
    getTradeDetails({
      tradeId: trade.id,
    }).then(data => {
      setDetails(data);
      setAuthenticatedOffer(data.offers.find(v => v.user.id === auth.userId));
      setOtherOffer(data.offers.find(v => v.user.id !== auth.userId));
    })
  }, [trade]);

  return <OldModal showClose={true} title='Trade Request' height={425} width={700} onClose={() => {
    trades.setSelectedTrade(null);
  }}>
    <div className='row pt-3 pb-3 ps-4 pe-0'>
      <div className='col-3 divider-right'>
        <PlayerHeadshot id={trade.user.id} name={trade.user.name}/>
        <BcOverlay id={trade.user.id} />
        <p className='mb-0 font-size-15 fw-700 text-center'>
          <TradeBelowNameText {...trade}/>
        </p>
      </div>
      <div className='col-9'>
        <TradeOfferEntry label={labelGiving} offer={authenticatedOffer}/>
        <div className='row'><div className='col-12 divider-top mb-2'/></div>
        <TradeOfferEntry label={labelReceiving} offer={otherOffer}/>
      </div>
      <div className='col-12'>
        {otherOffer && authenticatedOffer && details ? <TradeButtons trade={details}/> : null}
      </div>
    </div>
  </OldModal>
}

export default TradeModal;