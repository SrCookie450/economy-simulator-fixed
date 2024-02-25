import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { setResellableAssetPrice, takeResellableAssetOffSale } from "../../../services/economy";
import AuthenticationStore from "../../../stores/authentication";
import ActionButton from "../../actionButton";
import CreatorLink from "../../creatorLink";
import CatalogDetailsPage from "../stores/catalogDetailsPage";
import CatalogDetailsPageModal from "../stores/catalogDetailsPageModal";
import BuyItemModal from "./buyItemModal";
import PlayerImage from "../../playerImage";
import Robux from "./robux";
import useButtonStyles from "../../../styles/buttonStyles";

const useSellerEntryStyles = createUseStyles({
  button: {
    fontSize: '14px',
    marginTop: '20px',
    paddingLeft: '8px',
    paddingRight: '8px',
  },
  imageContainer: {
    maxWidth: '50px',
    display: 'block',
    margin: '0 auto',
  },
  takeOffSale: {
    background: 'grey',
    '&:hover': {
      background: 'darkgrey',
    },
  },
});

const SellerEntry = props => {
  const s = useSellerEntryStyles();
  const authStore = AuthenticationStore.useContainer();
  const isOwnItem = authStore.userId === props.seller.id;
  const store = CatalogDetailsPage.useContainer();
  const modalStore = CatalogDetailsPageModal.useContainer();
  const [locked, setLocked] = useState(false);

  const buttonStyles = useButtonStyles();
  return <div className='row'>
    <div className={`col-4 col-lg-3`}>
      <div className={s.imageContainer}>
        <PlayerImage size={50} id={props.seller.id} name={props.seller.name}></PlayerImage>
      </div>
    </div>
    <div className='col-6 col-lg-6'>
      <p className='mb-1'>
        <CreatorLink id={props.seller.id} type='User' name={props.seller.name}></CreatorLink>
      </p>
      <p className='mb-1'>
        <Robux>{props.price.toLocaleString()}</Robux>
      </p>
      <p className='mb-1'>
        Serial {props.serialNumber || 'N/A'}
      </p>
    </div>
    <div className='col-6 mx-auto col-lg-3 mb-4 mb-lg-0'>
      {
        isOwnItem ? <ActionButton disabled={locked} label='Take Off Sale' className={s.button + ' ' + s.takeOffSale} onClick={(e) => {
          e.preventDefault();
          setLocked(true);
          takeResellableAssetOffSale({
            assetId: store.details.id,
            userAssetId: props.userAssetId,
          }).then(() => {
            store.setAllResellers(store.allResellers.filter(c => {
              return c.userAssetId !== props.userAssetId;
            }))
          })
        }}></ActionButton> : <ActionButton className={s.button + ' ' + buttonStyles.buyButton} onClick={(e) => {
          e.preventDefault();
          modalStore.openPurchaseModal(store.getPurchaseDetails(props.userAssetId), authStore.robux, authStore.tix, 1);
        }}></ActionButton>
      }
    </div>
  </div>;
}

const usePaginationStyles = createUseStyles({
  text: {
    display: 'inline',
    marginBottom: 0,
    userSelect: 'none',
  },
  link: {
    paddingRight: '5px',
  },
  linkClickable: {
    color: '#0055b3',
    cursor: 'pointer',
  },
});

const ResellersPagination = props => {
  const s = usePaginationStyles();
  const store = CatalogDetailsPage.useContainer();
  if (!store.resellers) return null;
  const pages = Math.ceil(store.resellersCount / 6);
  const [pagesBack, setPagesBack] = useState([]); // Array of pages to appear on left side
  const [pagesForward, setPagesForward] = useState([]); // Array of pages to appear on right side
  const [showDots, setShowDots] = useState(false);

  useEffect(() => {
    let pagesAhead = [];
    let pagesBehind = [];
    for (let i = 1; i <= pages; i++) {
      if (i === store.resellersPage) continue;
      if (i > store.resellersPage) {
        pagesAhead.push(i);
      } else {
        pagesBehind.unshift(i);
      }
    }
    setPagesBack(pagesBehind.slice(0, 4));
    setPagesForward(pagesAhead.slice(0, 4));
    setShowDots(pagesAhead.length > 4);
  }, [store.resellersPage]);


  const onClick = v => {
    return (e) => {
      e.preventDefault();
      store.setResellersPage(v);
    }
  }

  const firstAvailable = store.resellersPage !== 1;
  const previousAvailable = firstAvailable;
  const lastAvailable = store.resellersPage !== pages;
  const nextAvailable = lastAvailable;

  /**
   * Condition link
   * @param {{page: number; condition: boolean; children: JSX.Element | string}} props 
   * @returns 
   */
  const LinkOnCondition = (props) => {
    if (props.condition) {
      return <span className={s.link + ' ' + s.linkClickable} onClick={(v) => {
        store.setResellersPage(props.page);
      }}>{props.children}</span>
    }
    return <span className={s.link}>{props.children}</span>
  }

  return <div>
    <p className={s.text}>
      <LinkOnCondition condition={firstAvailable} page={1}>First</LinkOnCondition>
      <LinkOnCondition condition={previousAvailable} page={store.resellersPage - 1}>Previous</LinkOnCondition>
      {
        pagesBack.map(v => {
          return <LinkOnCondition key={v} condition={true} page={v}>{v}</LinkOnCondition>
        })
      }
      <span className={s.link}>{store.resellersPage}</span>
      {
        pagesForward.map(v => {
          return <LinkOnCondition key={v} condition={true} page={v}>{v}</LinkOnCondition>
        })
      }
      {showDots && <span className={s.link}>...</span>}
      <LinkOnCondition condition={nextAvailable} page={store.resellersPage + 1}>Next</LinkOnCondition>
      <LinkOnCondition condition={lastAvailable} page={pages}>Last</LinkOnCondition>
    </p>
  </div>
}

const useMainStyles = createUseStyles({
  header: {
    fontSize: '26px',
    fontWeight: 400,
    paddingTop: '10px',
    paddingBottom: '10px',
  },
});

const Resellers = props => {
  const s = useMainStyles();
  const store = CatalogDetailsPage.useContainer();
  if (!store.resellers) return null;


  return <div className='row'>
    <div className='col-12'>
      <h3 className={s.header}>Private Sales</h3>
    </div>
    <div className='col-12 divider-right'>
      {store.allResellers && store.allResellers.length === 0 ? <p> Sorry, no one is privately selling this item at the moment. </p> : store.resellers.map(v => {
        return <SellerEntry key={v.userAssetId} {...v}></SellerEntry>
      })}
      <div className='row mt-2'>
        {store.allResellers && store.allResellers.length > 0 && <ResellersPagination></ResellersPagination>}
      </div>
    </div>
  </div>
}

export default Resellers;