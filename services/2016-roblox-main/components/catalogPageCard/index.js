import dayjs from "dayjs";
import React, {useEffect, useRef, useState} from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../lib/getFlag";
import { getBaseUrl } from "../../lib/request";
import {getItemUrl, itemNameToEncodedName} from "../../services/catalog";
import LimitedOverlay from "../catalogOverlays/limited";
import LimitedUniqueOverlay from "../catalogOverlays/limitedUnique";
import NewOverlay from "../catalogOverlays/new";
import SaleOverlay from "../catalogOverlays/sale";
import TimerOverlay from "../catalogOverlays/timer";
import CreatorLink from "../creatorLink";
import Robux from "../robux";
import thumbnailStore from "../../stores/thumbnailStore";
import Link from "../link";
import Tickets from "../tickets";

const useCatalogPageStyles = createUseStyles({
  image: {
    width: '100%',
    height: 'auto',
    border: '1px solid #eee',
    margin: '0 auto',
    display: 'block',
  },
  imageBig: {
    maxWidth: '152px',
    display: 'block',
    margin: '0 auto',
  },
  imageSmall: {
    maxWidth: '112px',
    display: 'block',
    margin: '0 auto',
  },
  detailsLarge: {

  },
  detailsSmall: {

  },
  detailsWrapper: {
    position: 'absolute',
    display: 'none',
    background: '#fff',
    paddingBottom: '5px',
    border: '1px solid #c3c3c3',
    transform: 'scale(110%)',
    zIndex: 2,
    paddingLeft: '4px',
  },
  detailsOpen: {
    display: 'block',
    marginTop: '-10px',
  },
  detailsKey: {
    fontSize: '10px',
    marginLeft: '0',
    marginRight: '2px',
    opacity: 0.7,
    fontWeight: 600,
  },
  detailsValue: {
    fontSize: '10px',
  },
  detailsEntry: {
    marginBottom: 0,
    marginTop: '-5px',
  },
  overviewDetails: {

  },
  itemName: {
    overflow: 'hidden',
  },
});

const usePriceStyles = createUseStyles({
  remainingText: {
    fontWeight: 700,
    fontSize: '12px',
  },
  remainingLabel: {
    color: 'red',
    fontWeight: 600,
    fontSize: '12px',
  }
})

const PriceText = (props) => {
  const s = usePriceStyles();
  const isLimited = props.itemRestrictions && (props.itemRestrictions.includes('Limited') || props.itemRestrictions.includes('LimitedUnique'));
  const copiesRemaining = props.unitsAvailableForConsumption;

  let priceElements = [];
  if (props.isForSale) {
    if (props.price === 0) {
      priceElements.push(<p className='mb-0 text-dark'>Free</p>)
    } else if (props.price !== null) {
      priceElements.push(<p className='mb-0'><Robux>{props.price}</Robux></p>)
    }
    // If item is free, why would anyone pay in tickets?
    if (props.priceTickets !== null && props.price !== 0) {
      priceElements.push(<p className='mb-0'><Tickets>{props.priceTickets}</Tickets></p>)
    }
  }

  if (props.isForSale && props.price !== 0 && props.price !== null) {
    if (copiesRemaining) {
      return <div>
        <>{priceElements.map((v, i) => <React.Fragment key={i}>{v}</React.Fragment>)}</>
        <span>
          <span className={s.remainingLabel}>Remaining: </span> <span className={s.remainingText + ' text-dark'}>{copiesRemaining.toLocaleString()}</span>
        </span>
      </div>
    }
    return <>{priceElements.map((v, i) => <React.Fragment key={i}>{v}</React.Fragment>)}</>
  }
  if (props.isForSale && (props.price === 0 || props.price === null)) {
    return <>{priceElements.map((v, i) => <React.Fragment key={i}>{v}</React.Fragment>)}</>
  }
  if (isLimited && !props.isForSale) {
    // Limited and not for sale anymore
    return <div>
      <p className='mb-0'><Robux prefix="was ">{props.price || '-'}</Robux></p>
      <p className='mb-0'><Robux prefix="now ">{props.lowestPrice || '-'}</Robux></p>
    </div>
  }
  return <p className='mb-0 text-dark'>offsale</p>;
}

const CatalogPageCard = props => {
  const s = useCatalogPageStyles();
  const isLarge = props.mode === 'large';
  const c = isLarge ? 'col-6 col-md-6 col-lg-3 mb-4 ' : 'col-6 col-md-6 col-lg-2 mb-2';
  const thumbs = thumbnailStore.useContainer();

  const [image, setImage] = useState(thumbs.getPlaceholder());
  useEffect(() => {
    setImage(thumbs.getAssetThumbnail(props.id));
  }, [props, thumbs.thumbnails]);

  const [showDetails, setShowDetails] = useState(false);
  const cardRef = useRef(null);
  // various conditionals
  const isLimited = props.itemRestrictions && props.itemRestrictions.includes('Limited');
  const isLimitedU = props.itemRestrictions && props.itemRestrictions.includes('LimitedUnique');
  const hasBottomOverlay = isLimited || isLimitedU;

  const isTimedItem = props.isForSale && props.offsaleDeadline;
  const isNew = props.createdAt ? dayjs(props.createdAt).isAfter(dayjs().subtract(2, 'days')) : false;
  const isSale = false; // TODO
  const hasTopOverlay = isNew || isSale;

  const hasRobux = props.isForSale && props.price !== null;
  const hasTickets = props.isForSale && props.priceTickets !== null;
  const hasBeforePrice = !props.isForSale && (isLimited || isLimitedU);
  const nameHeight = ((hasRobux && hasTickets) || hasBeforePrice) ? 18 : 36;
  const nameRef = useRef(null);
  const [cardMarginBottom, setCardMarginBottom] = useState(0);

  useEffect(() => {
    if (!nameRef.current)
      return;
    const totalHeight = nameRef.current.clientHeight;
    if (nameHeight > totalHeight) {
      const diff = nameHeight - totalHeight;
      setCardMarginBottom(diff);
    }
  }, [hasTickets, hasBeforePrice]);

  return <div className={`${c}`} onMouseEnter={() => setShowDetails(true)} onMouseLeave={() => setShowDetails(false)}>
    <div ref={cardRef} className={isLarge ? s.imageBig : s.imageSmall} >
      <Link href={getItemUrl({assetId: props.id, name: props.name})}>
        <a>
          <div style={{ zIndex: showDetails ? 10 : 0, position: 'relative' }}>
            {isTimedItem ? <TimerOverlay/> : null}
            {isNew ? <NewOverlay/> : isSale ? <SaleOverlay/> : null}
            <img alt={props.name} src={image} className={`${s.image} ${props.mode === 'large' ? s.imageBig : s.imageSmall}`} onError={e => {
              if (e.currentTarget.src !== thumbs.getPlaceholder()) {
                setImage(thumbs.getPlaceholder());
              }
            }}/>
            {isLimited ? <LimitedOverlay/> : null}
            {isLimitedU ? <LimitedUniqueOverlay/> : null}
            <div className={s.overviewDetails} style={hasBottomOverlay ? { marginTop: '-18px' } : undefined}>
              <p ref={nameRef} className={`mb-0 ${s.itemName}`} style={{maxHeight: nameHeight}}>{props.name}</p>
              <PriceText {...props}/>
            </div>
          </div>
        </a>
      </Link>
      <div
        style={showDetails ? {
          width: cardRef.current.clientWidth,
          paddingTop: cardRef.current.clientHeight + 'px',
          marginTop: '-' + cardRef.current.clientHeight + 'px',
        } : undefined}
        className={s.detailsWrapper + ' ' + (isLarge ? s.detailsLarge : s.detailsSmall) + ' ' + (showDetails ? s.detailsOpen : '')}>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Creator: </span>
          <span className={s.detailsValue}>
            <CreatorLink id={props.creatorTargetId} name={props.creatorName} type={props.creatorType}/>
          </span>
        </p>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Updated: </span>
          <span className={s.detailsValue}>
            {dayjs(props.updatedAt).fromNow()}
          </span>
        </p>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Sales: </span>
          <span className={s.detailsValue}>
            {getFlag('catalogSaleCountVisibleFromDetailsEndpoint', true) ? props.saleCount.toLocaleString() : 0}
          </span>
        </p>
        <p className={s.detailsEntry}>
          <span className={s.detailsKey}>Favorited: </span>
          <span className={s.detailsValue}>
            {props.favoriteCount?.toLocaleString() || 0} times
          </span>
        </p>
      </div>
    </div>
    <div style={{
      marginBottom: cardMarginBottom,
    }} />
  </div>
}

export default CatalogPageCard;