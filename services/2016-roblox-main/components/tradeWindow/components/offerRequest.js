import React, { useRef } from "react";
import { createUseStyles } from "react-jss";
import Robux from "../../catalogDetailsPage/components/robux";
import ItemImage from "../../itemImage";
import TradeWindowStore from "../stores/tradeWindowStore";


const useStyles = createUseStyles({
  offerRequestCard: {
    background: '#e1e1e1',
    width: '100%',
    border: '1px solid #d0d0d0',
    borderRadius: '2px',
    padding: '8px 6px',
  },
  placeholderImage: {
    height: '100%',
    width: '100%',
    display: 'block',
  },
  row: {
    paddingLeft: '10px',
    paddingRight: '10px',
    marginTop: '10px',
  },
  inline: {
    display: 'inline-block',
  },
  centerInline: {
    margin: '0 auto',
    display: 'block',
    textAlign: 'center',
  },
  robuxInput: {
    width: '130px',
  },
  removeItem: {
    cursor: 'pointer',
  },
  valueText: {
    float: 'right',
    fontSize: '12px',
  },
})


const OfferRequest = (props) => {
  const store = TradeWindowStore.useContainer();
  const { mode } = props;
  const items = mode === 'Offer' ? store.offerItems : store.requestItems;
  const s = useStyles();
  return <>
    <div className='row'>
      <div className='col-6'>
        <h3 className='font-size-15 fw-700 mb-0'>Your {mode}</h3>
      </div>
      <div className='col-6'>
        <p className={s.valueText}><span className='pe-2'>Value:</span><Robux inline={true}>{items.map(v => v.recentAveragePrice).reduce((a, b) => a + b, (mode === 'Request' ? store.requestRobux : store.offerRobux) || 0)}</Robux></p>
      </div>
    </div>
    <div className={'row ' + s.row}>
      {items.map(v => {
        return <div key={v.userAssetId} className='col-3 ps-0 pe-0'>
          <div className={'card ' + s.removeItem} onClick={(e) => {
            e.preventDefault();
            if (mode === 'Offer') {
              store.setOfferItems(store.offerItems.filter(c => c.userAssetId !== v.userAssetId));
            } else {
              store.setRequestItems(store.requestItems.filter(c => c.userAssetId !== v.userAssetId));
            }
          }}>
            <ItemImage className='pt-0' name={v.name} id={v.assetId}></ItemImage>
          </div>
        </div>
      })}
      {[... new Array(4 - items.length)].map((v, i) => {
        return <div className='col-3 ps-0 pe-0' key={'placeholder ' + i}>
          <img className={s.placeholderImage + ' card'} src='/img/empty.png'></img>
        </div>
      })}
    </div>
    <div className='row mt-2 mb-4'>
      <div className='col-12'>
        <div className={s.centerInline}>
          <div className={s.inline}>
            <p className='mb-0'>Plus <Robux inline={true}></Robux></p>
          </div>
          <div className={s.inline}>
            <input value={(mode === 'Offer' ? store.offerRobux : store.requestRobux) || ''} type='text' placeholder='Enter amount' className={s.robuxInput} onChange={(e) => {
              let v = parseInt(e.currentTarget.value, 10);
              if (v < 0 || v > 100000000 || isNaN(v)) {
                if (mode === 'Offer') {
                  store.setOfferRobux(0);
                } else {
                  store.setRequestRobux(0);
                }
                return
              }
              if (mode === 'Offer') {
                store.setOfferRobux(v);
              } else {
                store.setRequestRobux(v);
              }
            }}></input>
          </div>
          <div className={s.inline + ' ps-1'}>*</div>
        </div>
      </div>
    </div>
  </>
}

export default OfferRequest;