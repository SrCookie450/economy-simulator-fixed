import React, { useState } from 'react';
import { createUseStyles } from 'react-jss';
import { setResellableAssetPrice } from '../../../services/economy';
import useButtonStyles from '../../../styles/buttonStyles';
import ActionButton from '../../actionButton';
import OldModal from '../../oldModal';
import CatalogDetailsPage from '../stores/catalogDetailsPage';
import ItemImage from '../../itemImage';
import Robux from './robux';
import SelectUserAsset from './selectUserAsset';

const useModalStyles = createUseStyles({
  inlineSelect: {
    display: 'inline-block'
  },
  select: {},
  inlineRow: {},
  buttonRow: {
    marginTop: '20px',
  },
  priceInput: {
    width: '125px',
  },
});

const SellItemModal = props => {
  const buttonStyles = useButtonStyles();
  const store = CatalogDetailsPage.useContainer();
  const s = useModalStyles();
  const [price, setPrice] = useState(0);
  const [toSell, setToSell] = useState(null);
  const [locked, setLocked] = useState(false);
  const [error, setError] = useState(null);

  if (!store.ownedCopies || !store.resaleModalOpen) return null;
  const sellableCopies = store.ownedCopies.filter(v => v.price === 0 || v.price === null);

  return <OldModal title="Sell Your Collectible Item">
    {error && <div className='row'><div className='col-12 text-danger mb-0'>{error}</div></div>}
    <div className='row'>
      <div className='col-3'>
        <ItemImage id={store.details.id}></ItemImage>
      </div>
      <div className='col-9 mt-4'>
        {sellableCopies.length > 1 &&
          <SelectUserAsset selected={toSell} setSelected={setToSell} userAssets={sellableCopies} locked={locked}></SelectUserAsset>
        }
        <div className={s.inlineRow}>
          <div className={s.inlineSelect}>
            <p className='mb-0'>Price (minimum 1): <Robux inline={true}></Robux></p>
          </div>
          <div className={s.inlineSelect}>
            <input disabled={locked} value={price} className={s.priceInput} type='text' onChange={(e) => {
              let parsed = parseInt(e.currentTarget.value, 10);
              if (!Number.isSafeInteger(parsed)) return;
              setPrice(parsed);
            }}></input>
          </div>
        </div>
        <div className={s.inlineRow}>
          <p className='mb-0'>Marketplace fee at 30%: {price && Math.ceil(price * 0.3)}</p>
        </div>
        <div className={s.inlineRow}>
          <p className='mb-0'>You get: {Math.trunc(price * 0.7)}</p>
        </div>
      </div>
    </div>
    <div className='row mt-4'>
      <div className='col-12'>
        <div className={`${s.buttonRow} row`}>
          <div className='col-8 offset-2'>
            <div className='row'>
              <div className='col-6 pe-0'>
                <ActionButton disabled={locked} label='Sell Now' className={buttonStyles.buyButton} onClick={(e) => {
                  e.preventDefault();
                  setLocked(true);
                  let userAssetId = toSell;
                  if (userAssetId === null) {
                    userAssetId = sellableCopies[0].userAssetId;
                  }
                  setResellableAssetPrice({
                    userAssetId: userAssetId,
                    price: price,
                    assetId: store.details.id,
                  }).then(() => {
                    window.location.reload();
                  }).catch(e => {
                    setLocked(false);
                    setError(e.message);
                  })
                }}></ActionButton>
              </div>
              <div className='col-6 ps-4'>
                <ActionButton disabled={locked} label='Cancel' className={buttonStyles.cancelButton} onClick={(e) => {
                  e.preventDefault();
                  store.setResaleModalOpen(false);
                }}></ActionButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </OldModal>
}

export default SellItemModal;