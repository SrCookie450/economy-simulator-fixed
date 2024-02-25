import { useState } from "react";
import { createContainer } from "unstated-next";

const TradeWindowStore = createContainer(() => {
  const [partnerUserId, setPartnerUserId] = useState(null);
  const [offerItems, setOfferItems] = useState([]);
  const [requestItems, setRequestItems] = useState([]);
  const [offerRobux, setOfferRobux] = useState(null);
  const [requestRobux, setRequestRobux] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [counterId, setCounterId] = useState(null);

  return {
    counterId,
    setCounterId,

    offerItems,
    setOfferItems,

    requestItems,
    setRequestItems,

    offerRobux,
    setOfferRobux,

    requestRobux,
    setRequestRobux,

    partnerUserId,
    setPartnerUserId,

    feedback,
    setFeedback,
  }
});

export default TradeWindowStore;