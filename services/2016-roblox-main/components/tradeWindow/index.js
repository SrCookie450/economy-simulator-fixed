import { useRouter } from "next/dist/client/router";
import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { counterTrade, createTrade, getTradeDetails } from "../../services/trades";
import { getUserInfo } from "../../services/users";
import AuthenticationStore from "../../stores/authentication";
import useButtonStyles from "../../styles/buttonStyles";
import ActionButton from "../actionButton";
import Robux from "../catalogDetailsPage/components/robux";
import InventoryWithPagination from "./components/inventoryWithPagination";
import OfferRequest from "./components/offerRequest";
import TradeWindowStore from "./stores/tradeWindowStore";

const useStyles = createUseStyles({
  '@global': {
    '.navbar-wrapper-main': {
      display: 'none!important',
    },
    'footer': {
      display: 'none!important',
    }
  },
  wrapper: {
    marginTop: '10px',
  },
  exitText: {
    float: 'right',
    marginBottom: 0,
    cursor: 'pointer',
  },
  exitButton: {
    marginLeft: '4px',
    fontWeight: 600,
    color: 'white',
    background: '#666',
    borderRadius: '100%',
    paddingLeft: '5px',
    paddingRight: '5px',
  },
  offerRequestCard: {
    background: '#e1e1e1',
    width: '100%',
    border: '1px solid #d0d0d0',
    borderRadius: '2px',
    padding: '8px 6px',
  },
  buttonWrapper: {
    margin: '0 auto',
    maxWidth: '160px',
    display: 'block',
  },
  sendButton: {
    fontSize: '22px',
    fontWeight: 600,
    paddingTop: '8px',
    paddingBottom: '8px',
  },
  container: {
    minWidth: '800px',
    overflow: 'auto',
    background: '#fff',
  },
  feedbackWrapper: {
    padding: '4px',
    border: '1px solid red',
    background: '#fbe7e5',
  },
})

const TradeWindow = props => {
  const store = TradeWindowStore.useContainer();
  const auth = AuthenticationStore.useContainer();

  const router = useRouter();
  const userId = router.query['TradePartnerID'];
  const sessionId = router.query['TradeSessionId'];

  const [name, setName] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    if (!userId) return
    getUserInfo({ userId }).then(info => {
      setName(info.name);
      store.setPartnerUserId(info.id);
    })
  }, [userId]);

  useEffect(() => {
    if (!sessionId || !auth.userId) return;
    getTradeDetails({
      tradeId: sessionId,
    }).then((d) => {
      console.log('[info] countering trade. id=' + d.id)
      store.setCounterId(d.id);
      for (const item of d.offers) {
        let isMine = item.user.id === auth.userId;
        if (isMine) {
          store.setOfferItems(item.userAssets.map(v => {
            v.userAssetId = v.id;
            return v;
          }));
          store.setOfferRobux(item.robux);
        } else {
          store.setRequestItems(item.userAssets.map(v => {
            v.userAssetId = v.id;
            return v;
          }));
          store.setRequestRobux(item.robux)
        }
      }
    }).catch(e => {
      store.setFeedback(`This trade session is invalid or you are not authorized to view it.`);
    })
  }, [sessionId, auth.userId]);

  const s = useStyles();
  const buttonStyles = useButtonStyles();

  if (!userId || !name || !store.partnerUserId) return null;
  return <div className={s.wrapper}>
    <div className={'container' + ' ' + s.container}>
      <div className='row'>
        <div className='col-6'>
          <p className='mb-0'><span className='font-size-18 fw-700'>TRADING</span> with {name}</p>
        </div>
        <div className='col-6'>
          <p className={s.exitText} onClick={() => {
            window.close();
          }}>Exit Trading <span className={s.exitButton}>X</span></p>
        </div>
      </div>
      <div className='row mt-3'>
        <div className='col-4'>
          <div className={s.offerRequestCard}>
            <OfferRequest mode='Offer'></OfferRequest>
            <div className='mt-4'>&emsp;</div>
            <div className='row mt-4 mb-4'>
              <div className='col-12'>
                <div className='divider-top'></div>
              </div>
            </div>
            <div className='mt-4'>&emsp;</div>
            <OfferRequest mode='Request'></OfferRequest>
            <div className='mt-4 mb-4'>&emsp;</div>
            <div className='mt-4'>&emsp;</div>
            <div className={s.buttonWrapper}>
              <ActionButton label='Send Request' className={buttonStyles.buyButton + ' ' + s.sendButton} onClick={() => {
                if (locked) return;
                store.setFeedback(null);
                if (store.requestItems.length === 0 || store.offerItems.length === 0) {
                  store.setFeedback(`Your must request and offer must both contain at least one item.`)
                  return;
                }
                setLocked(true);
                let promise = null;
                let request = {
                  offerUserId: auth.userId,
                  offerUserAssets: store.offerItems.map(v => v.userAssetId),
                  offerRobux: store.offerRobux,
                  requestUserId: userId,
                  requestUserAssets: store.requestItems.map(v => v.userAssetId),
                  requestRobux: store.requestRobux,
                }
                if (store.counterId) {
                  request.tradeId = store.counterId;
                  // @ts-ignore
                  promise = counterTrade(request);
                } else {
                  promise = createTrade(request)
                }
                promise.then(result => {
                  // Trade sent
                  alert('Your trade request has been sent.');
                  window.close();
                }).catch(e => {
                  let msg = e.response?.data?.errors[0]?.message;
                  store.setFeedback(msg || e.message);
                }).finally(() => {
                  setLocked(false);
                })
              }}></ActionButton>
            </div>
          </div>
          {
            store.feedback && <div className={s.feedbackWrapper + ' mt-2'}>
              <p className='mb-0'>{store.feedback}</p>
            </div>
          }
        </div>
        <div className='col-8'>
          <InventoryWithPagination mode='Offer'></InventoryWithPagination>
          <div className='mt-4 mb-4 divider-top'>
          </div>
          <InventoryWithPagination mode='Request'></InventoryWithPagination>
        </div>
      </div>
      <div className='row'>
        <div className='col-12'>
          <p className='mb-0 fw-400 font-size-12 mt-2'><span className='fw-700'>*</span><span className='lighten-3'> A 30% fee will be taken from the amount.</span></p>
        </div>
      </div>
    </div>
  </div >
}

export default TradeWindow;