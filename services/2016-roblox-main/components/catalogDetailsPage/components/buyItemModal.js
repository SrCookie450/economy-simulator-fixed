import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import CatalogDetailsPage from "../stores/catalogDetailsPage";
import CatalogDetailsPageModal from "../stores/catalogDetailsPageModal";
import ItemImage from "../../itemImage";
import Robux from "./robux";
import Link from "../../link";
import Tickets from "../../tickets";

const useDescStyles = createUseStyles({
  purchaseText: {
    fontWeight: 700,
    marginTop: '10px',
  },
});

const ModalDescription = props => {
  const modalStore = CatalogDetailsPageModal.useContainer();
  const sellerDetails = modalStore.purchaseDetails;
  const store = CatalogDetailsPage.useContainer();
  const auth = AuthenticationStore.useContainer();
  const s = useDescStyles();

  switch (modalStore.purchaseState) {
    case 'PURCHASE_PENDING':
    case 'PURCHASE':
      let priceElement = <Robux inline={true}>{sellerDetails.price}</Robux>
      let action = 'buy';
      if (modalStore.currency === 2) {
        priceElement = <Tickets inline={true}>{sellerDetails.priceTickets}</Tickets>
      }else{
        if (sellerDetails.price === 0) {
          priceElement = <span>Free</span>
          action = 'take';
        }
      }
      return <p className={s.purchaseText}>Would you like to {action} the {store.details.name} {store.subCategoryDisplayName} from {sellerDetails.sellerName} for {priceElement}?</p>;
    case 'PURCHASE_OK':
      let priceStuff = modalStore.currency === 2 ?
        <Tickets inline={true}>{sellerDetails.priceTickets}</Tickets> :
        <Robux inline={true}>{sellerDetails.price}</Robux>;

      return <p className={s.purchaseText}>You have successfully bought the {store.details.name} {store.subCategoryDisplayName} from {sellerDetails.sellerName} for {priceStuff}.</p>;
    case 'INSUFFICIENT_FUNDS':
      if (modalStore.currency === 1) {
        return <p className={s.purchaseText}>
          You need <Robux inline={true}>{sellerDetails.price - auth.robux}</Robux> more to purchase this item.
        </p>;
      }
      return <p className={s.purchaseText}>
          You need <Tickets inline={true}>{sellerDetails.priceTickets - auth.tix}</Tickets> more to purchase this item.
        </p>;
    case 'PURCHASE_ERROR':
      return <p className={s.purchaseText}>
        An error ocurred purchasing this item. You have not been charged.
      </p>;
    default:
      return null;
  }
}

const ModalButtons = props => {
  const modalStore = CatalogDetailsPageModal.useContainer();
  const sellerDetails = modalStore.purchaseDetails;
  const store = CatalogDetailsPage.useContainer();
  const s = useButtonStyles();

  switch (modalStore.purchaseState) {
    case 'PURCHASE':
    case 'PURCHASE_PENDING':
      return <div className='row'>
        <div className='col-8 offset-2'>
          <div className='row'>
            <div className='col-6'>
              <ActionButton disabled={modalStore.purchaseState === 'PURCHASE_PENDING'} label={sellerDetails.price === 0 ? 'Take' : 'Buy Now'} className={s.buyButton} onClick={(e) => {
                e.preventDefault();
                modalStore.setPurchaseState('PURCHASE_PENDING');
                modalStore.purchaseItem().then(() => {
                  modalStore.setPurchaseState('PURCHASE_OK');
                }).catch(e => {
                  modalStore.setPurchaseState(e.state || 'PURCHASE_ERROR');
                })
              }}/>
            </div>
            <div className='col-6'>
              <ActionButton disabled={modalStore.purchaseState === 'PURCHASE_PENDING'} label='Cancel' className={s.cancelButton} onClick={(e) => {
                e.preventDefault();
                modalStore.closePurchaseModal();
              }}/>
            </div>
          </div>
        </div>
      </div>
    case 'PURCHASE_OK':
      return <div className='row'>
        <div className='col-10 offset-1'>
          <ActionButton className={s.continueButton} label='Continue Shopping' onClick={() => {
            modalStore.closePurchaseModal();
            window.location.reload();
          }}/>
        </div>
      </div>
    case 'INSUFFICIENT_FUNDS':
      return <div className={`row ${s.badPurchaseRow}`}>
        <div className='col-6 pe-0 offset-1'>
          <ActionButton className={s.buyButton} label='Purchase ROBUX' onClick={() => {
            modalStore.closePurchaseModal();
            window.location.href = '/'
          }}/>
        </div>
        <div className='col-4'>
          <ActionButton disabled={false} label='Cancel' className={s.cancelButton} onClick={(e) => {
            e.preventDefault();
            modalStore.closePurchaseModal();
          }}/>
        </div>
      </div>
    case 'PURCHASE_ERROR':
      return <div className={`row ${s.badPurchaseRow}`}>
        <div className='col-4 offset-4'>
          <ActionButton disabled={false} label='Cancel' className={s.cancelButton} onClick={(e) => {
            e.preventDefault();
            modalStore.closePurchaseModal();
          }}/>
        </div>
      </div>
    default:
      return null;
  }
}

const ModalImage = (props) => {
  const modalStore = CatalogDetailsPageModal.useContainer();
  const store = CatalogDetailsPage.useContainer();
  switch (modalStore.purchaseState) {
    case 'PURCHASE':
    case 'PURCHASE_PENDING':
    case 'PURCHASE_OK':
      return <ItemImage id={store.details.id}/>
    case 'INSUFFICIENT_FUNDS':
    case 'PURCHASE_ERROR':
      // TODO: yellow triangle icon
      return null;
    default:
      return null;
  }
}

const ModalTitle = () => {
  const modalStore = CatalogDetailsPageModal.useContainer();
  const store = CatalogDetailsPage.useContainer();

  switch (modalStore.purchaseState) {
    case 'PURCHASE':
    case 'PURCHASE_PENDING':
      return <span>Buy Item</span>
    case 'PURCHASE_OK':
      return <span>Purchase Complete!</span>
    case 'INSUFFICIENT_FUNDS':
      return <span>Insufficient Funds</span>
    case 'PURCHASE_ERROR':
      return <span>Purchase Error</span>
    default:
      return null;
  }
}

export const useModalStyles = createUseStyles({
  modalBg: {
    background: 'rgba(0,0,0,0.8)',
    position: 'fixed',
    top: 0,
    width: '100%',
    height: '100%',
    left: 0,
    zIndex: 9999,
  },
  modalWrapper: {
    width: '400px',
    height: '250px',
    backgroundColor: '#e1e1e1',
    margin: '0 auto',
    border: '1px solid #a3a3a3',
    marginTop: 'calc(50vh - 125px)',
  },
  title: {
    textAlign: 'center',
    fontWeight: 700,
    fontSize: '24px',
    marginTop: '10px',
  },
  innerSection: {
    padding: '4px 8px',
    background: 'white',
    width: '100%',
    height: '205px',
    border: '4px solid #e1e1e1',
  },
  footerText: {
    textAlign: 'center',
    marginBottom: '0',
    marginTop: '14px',
    fontSize: '12px',
    fontWeight: 600,
    color: 'grey',
  },
});
const BuyItemModal = props => {
  const s = useModalStyles();
  const store = CatalogDetailsPage.useContainer();
  const modalStore = CatalogDetailsPageModal.useContainer();
  const authStore = AuthenticationStore.useContainer();

  if (modalStore.isPurchasePromptOpen === false) {
    return null;
  }
  if (store.details === null) return null;

  const sellerDetails = modalStore.purchaseDetails;
  const newBalance = modalStore.currency === 1 ? authStore.robux - sellerDetails.price : authStore.tix - sellerDetails.priceTickets;

  const AfterTransactionBalance = () => {
    if (modalStore.currency === 1) {
      return <p className={s.footerText}>Your balance after this transaction will be R${newBalance.toLocaleString()} robux.</p>
    }
    return <p className={s.footerText}>Your balance after this transaction will be T${newBalance.toLocaleString()} tix.</p>
  }

  return <div className={s.modalBg}>
    <div className={s.modalWrapper}>
      <h3 className={s.title}>
        <ModalTitle/>
      </h3>
      <div className={s.innerSection}>
        <div className='row'>
          <div className='col-3'>
            <ModalImage/>
          </div>
          <div className='col-9'>
            <ModalDescription/>
          </div>
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            <ModalButtons/>
          </div>
          <div className='row'>
            <div className='col-12'>
              {
                (modalStore.purchaseState === 'PURCHASE' || modalStore.purchaseState === 'PURCHASE_PENDING') &&
                <AfterTransactionBalance />
                || modalStore.purchaseState === 'PURCHASE_OK' && <p className={s.footerText}><Link href='/My/Character.aspx'><a>Customize Character</a></Link></p> || null
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default BuyItemModal;