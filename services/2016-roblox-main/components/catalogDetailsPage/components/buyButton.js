import React, { useState } from "react";
import { createUseStyles } from "react-jss";
import AuthenticationStore from "../../../stores/authentication";
import ActionButton from "../../actionButton";
import Tickets from "../../tickets";
import CatalogDetailsPage from "../stores/catalogDetailsPage";
import CatalogDetailsPageModal from "../stores/catalogDetailsPageModal";
import BuyItemModal from "./buyItemModal";
import Robux from "./robux";
import OffsaleDeadline from "./offsaleDeadline";

const useBestPriceStyles = createUseStyles({
  text: {
    fontSize: '14px',
    marginBottom: '4px',
    textAlign: 'center',
    marginTop: '4px',
  },
  noSellers: {
    textAlign: 'center',
    marginTop: '30px',
    marginBottom: '30px',
  },
});

const BestPriceEntry = props => {
  const s = useBestPriceStyles();
  const store = CatalogDetailsPage.useContainer();
  const lowestSeller = store.getPurchaseDetails();
  if (!lowestSeller) {
    return <p className={s.noSellers}> No one is currently selling this item. </p>
  }
  const lowestPrice = lowestSeller.price;
  return <p className={s.text}>
    <Robux prefix="Best Price: ">{lowestPrice}</Robux>
  </p>
}

const useBuyButtonStyles = createUseStyles({
  wrapper: {
    width: '100%',
    border: '1px solid #a7a7a7',
    background: '#e1e1e1',
  }
});

const PrivateSellersCount = props => {
  const store = CatalogDetailsPage.useContainer();
  return <p className='mt-0 mb-0 text-center'>
    <a className='a'>See all private sellers ({store.resellersCount || 0})</a>
  </p>
}

const useSaleCountStyles = createUseStyles({
  text: {
    color: '#666',
    fontSize: '12px',
  }
})

const SaleCount = props => {
  const store = CatalogDetailsPage.useContainer();
  const s = useSaleCountStyles();
  return <p className={'mt-2 mb-2 text-center ' + s.text}>
    ( <span className='text-black'>{store.saleCount}</span> Sold)
  </p>
}

const OwnedCount = props => {
  const store = CatalogDetailsPage.useContainer();
  const s = useSaleCountStyles();
  if (!store.ownedCopies || store.ownedCopies.length === 0) return null;
  return <p className={'mt-2 mb-0 text-center ' + s.text}>
    ( <span className='text-black'>{store.ownedCopies.length}</span> Owned)
  </p>
}

const useTicketPriceStyles = createUseStyles({
  ticketPrice: {
    width: '100%',
    height: '23px',
  },
});

const PriceTickets = props => {
  const s = useTicketPriceStyles();
  const store = CatalogDetailsPage.useContainer();

  return <div className={s.ticketPrice}>
    <span>Price: </span><Tickets>{store.details.priceTickets}</Tickets>
  </div>
}

const BuyAction = props => {
  const currency = props.currency; // 1 = Robux, 2 = Tickets
  const store = CatalogDetailsPage.useContainer();
  const authenticationStore = AuthenticationStore.useContainer();
  const modalStore = CatalogDetailsPageModal.useContainer();
  const productInfo = store.getPurchaseDetails();
  const auth = AuthenticationStore.useContainer();

  if (store.ownedCopies === null) return null;
  const isOwned = store.ownedCopies.length !== 0;
  const showPriceText = store.details.isForSale;
  const isResaleItem = store.isResellable;
  const isDisabled = !store.isResellable && !store.details.isForSale ||
    (isOwned && !store.isResellable) ||
    (store.isResellable && !productInfo ||
      store.isResellable && productInfo.sellerId === authenticationStore.userId
    );
  const isFree = !isDisabled && store.details.price === 0;

  const tooltipTitle = isOwned ? 'You already own this item.' : 'This item is not for sale';
  const actionBuyText = (() => {
      if (isFree && !isResaleItem)
        return 'Take One';

      if (currency === 2) {
        return 'Buy with Tx';
      }
      return 'Buy with R$';
  })();

  if (store.isResellable) {
    if (!store.allResellers || store.allResellers.length === 0) {
      return null;
    }
  }

  return <div>
    {showPriceText &&
      <p className='mb-1 text-center'>
        {
          currency === 1 ? <Robux prefix="Price: ">{isFree ? 'FREE' : store.details.price}</Robux> :
          <PriceTickets />
        }
      </p>
    }
    <ActionButton onClick={(e) => {
      modalStore.openPurchaseModal(store.getPurchaseDetails(), auth.robux, auth.tix, currency);
    }} label={actionBuyText} disabled={isDisabled} tooltipText={tooltipTitle}/>
  </div>
}

const useOrTabStyles = createUseStyles({
  wrapper: {
    borderBottom: '1px solid #a7a7a7',
    marginBottom: '10px',
  },
  label: {
    padding: '0 10px',
    marginBottom: 0,
    width: 'width',
    textAlign: 'center',
  },
  labelBg: {
    background: '#e1e1e1',
    position: 'relative',
    bottom: '-10px',
  },
});

const PurchaseWithRobuxOrTicketsLabel = props => {
  const s = useOrTabStyles();
  return <div className={s.wrapper}>
    <p className={s.label}>
      <span className={s.labelBg}>OR</span>
    </p>
  </div>
}

/**
 * The purchase button
 * @param {*} props 
 */
const BuyButton = props => {
  const s = useBuyButtonStyles();
  const store = CatalogDetailsPage.useContainer();
  const isResellAsset = store.isResellable;
  // Show buy button if item has ticket price and no sale price, or if item has sale price
  const showBuyButton = (() => {
    if (isResellAsset) return true;

    if (store.details.priceTickets) {
      if (store.details.price === null) {
        return false;
      }
    }
    return true;
  })();
  const showBuyTicketsButton = store.details.priceTickets !== null && !isResellAsset;
  const showOrTab = !isResellAsset && showBuyButton && showBuyTicketsButton;
  const hasOffsaleLabel = store.offsaleDeadline !== null && !isResellAsset && (showBuyButton || showBuyTicketsButton);

  return <div className={s.wrapper}>
    <div>
      {hasOffsaleLabel ? <OffsaleDeadline offsaleDeadline={store.offsaleDeadline} /> : null}
    </div>
    <div>
      {isResellAsset ? <BestPriceEntry details={store.details}/> : null}
    </div>
    <div>
      {!isResellAsset  ? <div className='mt-2'/> : null}
      {showBuyButton ? <BuyAction currency={1} /> : null}
    </div>
    <div>
      {showOrTab ? <PurchaseWithRobuxOrTicketsLabel /> : null}
      {showBuyTicketsButton ? <BuyAction currency={2} /> : null}
    </div>
    <div>
      {isResellAsset ? <PrivateSellersCount details={store.details}/> : null}
    </div>
    <div>
      {isResellAsset ? <OwnedCount/> : null}
    </div>
    <div>
      <SaleCount/>
    </div>
  </div >
}

export default BuyButton;