import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import t from "../../../lib/t";
import {getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import { getCollectibleInventory } from "../../../services/inventory";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import Robux from "../../catalogDetailsPage/components/robux";
import GenericPagination from "../../genericPagination";
import ItemImage from "../../itemImage";
import TradeWindowStore from "../stores/tradeWindowStore";
import Link from "../../link";


const ItemEntry = props => {
  const store = TradeWindowStore.useContainer();
  const { mode } = props;

  const [open, setOpen] = useState(false);

  const isInOfferOrRequest = mode === 'Offer' ? store.offerItems.find(v => v.userAssetId === props.userAssetId) : store.requestItems.find(v => v.userAssetId === props.userAssetId);

  const s = useStyles();
  const buttonStyles = useButtonStyles();
  return <div className={s.itemColEntry + ' ' + (open ? s.itemOpenEntry : '')} onMouseEnter={() => {
    setOpen(true);
  }} onMouseLeave={() => {
    setOpen(false);
  }}>
    <div className={s.itemCard}>
      <p className={'mb-0 font-size-12 text-center ' + (open ? s.itemNameOpen : s.itemName )}>
          <a target='_blank' href={getItemUrl({assetId: props.assetId, name: props.name})}>
            {props.name}
          </a>
      </p>
      <div className={open ? s.itemImageWrapperOpen : undefined}>
        <ItemImage className='pt-0' id={props.assetId} name={props.name}></ItemImage>
      </div>
      {open && <div>
        <p className={s.statEntry}><span className='fw-700'>Avg. Price: </span> <img style={{ height: '7px' }} src='/img/img-robux.png'></img> <span className={s.robux}>{props.recentAveragePrice || '-'}</span></p>
        <p className={s.statEntry}><span className='fw-700'>Orig. Price: </span> <img style={{ height: '7px' }} src='/img/img-robux.png'></img> <span className={s.robux}>{props.originalPrice || '-'}</span></p>
        <p className={s.statEntry}><span className='fw-700'>Serial #: </span> {props.serialNumber || '--'} / {props.copyCount || '--'}</p>

        <button onClick={() => {
          if (isInOfferOrRequest) {
            if (mode === 'Offer') {
              store.setOfferItems(store.offerItems.filter(v => v.userAssetId !== props.userAssetId))
            } else {
              store.setRequestItems(store.requestItems.filter(v => v.userAssetId !== props.userAssetId));
            }
          } else {
            if (mode === 'Offer') {
              if (store.offerItems.length >= 4) return
              store.setOfferItems([...store.offerItems, props]);
            } else {
              if (store.requestItems.length >= 4) return
              store.setRequestItems([...store.requestItems, props]);
            }
          }
        }} className={buttonStyles.cancelButton + ' ' + s.requestButton}>{isInOfferOrRequest ? 'Remove' : mode}</button>

      </div>}
    </div>
  </div>
}

const useStyles = createUseStyles({
  requestButton: {
    fontSize: '8px',
    textAlign: 'center',
    background: 'linear-gradient(0deg, rgba(187,187,187,1) 0%, rgba(255,255,255,1) 100%)',
    padding: 0,
    border: '1px solid #c3c3c3',
  },
  itemRow: {
    display: 'flex',
    flexWrap: 'wrap',
  },
  itemColEntry: {
    width: 'calc(14% - 4px)',
    display: 'flex',
    paddingLeft: '2px',
    paddingRight: '2px',
  },
  itemCard: {
    border: '1px solid #c3c3c3',
    padding: '0 4px',
    width: '100%',
  },
  inlineRowRight: {
    float: 'right',
  },
  inline: {
    display: 'inline-block',
    textAlign: 'right',
  },
  categorySelector: {
    display: 'inline-block',
    textAlign: 'left',
    fontSize: '12px',
  },
  itemOpenEntry: {
    transform: 'scale(1.25)',
    height: '125px',
    marginTop: '-30px',
    zIndex: 99,
    background: 'white',
    position: 'relative',
    bottom: '-20px',
  },
  itemImageWrapperOpen: {
    maxWidth: '50px',
    margin: '0 auto',
  },
  itemNameOpen: {
    fontSize: '10px',
    height: '24px',
  },
  statEntry: {
    marginBottom: 0,
    fontSize: '7px',
    color: '#999',
  },
  robux: {
    color: '#060',
  },
  itemName: {
    height: '31px',
    overflow: 'hidden',
    display: 'inline-block',
  },
})

const InventoryWithPagination = props => {
  const { mode } = props;
  const limit = getFlag('tradeWindowInventoryCollectibleLimit', 10);

  const auth = AuthenticationStore.useContainer();
  const store = TradeWindowStore.useContainer();

  const [response, setResponse] = useState(null);
  const [cursor, setCursor] = useState(null);
  const [page, setPage] = useState(1);
  const [assetType, setAssetType] = useState('null');
  const [feedback, setFeedback] = useState(null);
  useEffect(() => {
    getCollectibleInventory({
      userId: mode === 'Offer' ? auth.userId : store.partnerUserId,
      limit,
      assetTypeId: assetType,
      cursor,
    }).then(data => {
      setResponse(data);
    }).catch(e => {
      setResponse(null);
      if (e.response?.status === 403) {
        setFeedback('This player has a private inventory');
      } else if (e.response?.status === 400) {
        setFeedback('This player is not available');
      } else {
        setFeedback(e.message);
      }
    })
  }, [cursor, assetType]);

  const s = useStyles();
  const items = response && t.array(response.data);

  return <div className='row'>
    <div className='col-6'>
      <h3 className='font-size-14 fw-600 mb-0'>{mode === 'Offer' ? 'My Inventory' : 'Partner\'s Inventory'}</h3>
    </div>
    <div className='col-6'>
      <div className={s.inlineRowRight}>
        <p className={s.inline + ' mb-0 font-size-12 pe-1'}>Category:</p>
        <select className={s.categorySelector} value={assetType} onChange={(e) => {
          setCursor(null);
          setAssetType(e.currentTarget.value);
        }}>
          <option value='null'>All</option>
          <option value='8'>Accessory | Hats</option>
          <option value='41'>Accessory | Hair</option>
          <option value='42'>Accessory | Face</option>
          <option value='43'>Accessory | Neck</option>
          <option value='44'>Accessory | Shoulders</option>
          <option value='45'>Accessory | Front</option>
          <option value='46'>Accessory | Back</option>
          <option value='47'>Accessory | Waist</option>
          <option value='19'>Gear</option>
          <option value='18'>Faces</option>
        </select>
      </div>
    </div>
    <div className='col-12 mt-4'>
      {feedback && <p className='text-danger mt-4 mb-4 text-center'>{feedback}</p>}
      <div className={s.itemRow}>
        {
          items ? (items.length ? t.array(response.data).map(v => {
            return <ItemEntry key={v.userAssetId} mode={props.mode} {...v}/>
          }) : <p className='col-12 text-center mt-4'>User does not have any items in this category.</p>) : null
        }
      </div>
    </div>
    <div className='col-3 mx-auto mt-4'>
      <GenericPagination page={page} onClick={(m) => {
  return (e) => {
    if (m === 1) {
      if (!response.nextPageCursor) return;
      setCursor(response.nextPageCursor);
      setPage(page + 1);
    } else {
      if (!response.previousPageCursor) return;
      setCursor(response.previousPageCursor);
      setPage(page - 1);
    }
  }
}}/>
    </div>
  </div>
}

export default InventoryWithPagination;