import { useState } from "react";
import { createUseStyles } from "react-jss";
import {getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import Robux from '../../robux';
import ItemImage from "../../itemImage";

const useTradeItemStyles = createUseStyles({
  col: {
    width: `calc(20% - 5px)`,
    border: '1px solid #c3c3c3',
    marginRight: '5px',
    padding: 0,
    height: '100px',
    zIndex: 99,
  },
  expandedCol: {
    transform: 'scale(1.3)',
    height: '125px',
    zIndex: 200,
    background: 'white',
    marginBottom: '-50px',
  },
  itemName: {
    height: 'auto',
    lineHeight: '1',
    overflow: 'hidden',
    fontSize: '12px',
    textAlign: 'center',
  },
  expandedItemName: {
    fontSize: '9px',
  },
  imageWrapper: {
    width: '80px',
    height: '80px',
    margin: '0 auto',
    display: 'block',
  },
})

const useTradeLabelStyles = createUseStyles({
  rapText: {
    fontSize: '8px',
    fontWeight: 700,
    color: '#777',
    letterSpacing: -0.1,
    paddingLeft: '4px',
    marginBottom: '4px',
  },
  robux: {
    color: '#060',
    letterSpacing: -0.1,
  },
  robuxWrapper: {
    display: 'inline',
  },
  robuxIcon: {
    height: '8px',
    width: '12px',
    display: 'inline-block',
    verticalAlign: 'middle',
    margin: '0 0 0 4px',
  },
  serialText: {
    marginTop: '-14px',
  },
});

const TradeLabelWithRobux = props => {
  const s = useTradeLabelStyles();
  return <p className={s.rapText}>{props.name} <img className={s.robuxIcon} src='/img/img-robux.png' /> {props.amount || '-'}</p>;
}

const TradeItem = props => {
  const s = useTradeItemStyles();
  const labelStyles = useTradeLabelStyles();
  const [expanded, setExpanded] = useState(null);

  return <div className={expanded ? s.expandedCol + ' ' + s.col : s.col} onMouseEnter={() => {
    if (!props.name) return
    setExpanded(true);
  }} onMouseLeave={() => {
    setExpanded(false);
  }}>
    {props.name && <p className={`text-truncate ${s.itemName} ${expanded ? s.expandedItemName : ''} mb-0 ps-1 pe-1`}>
      <a href={getItemUrl({assetId: props.assetId, name: props.name})}>
        {props.name}
      </a>
    </p>}
    {props.robux && <p className={`text-center`}>{props.robux} Robux</p>}
    <div className={s.imageWrapper}>
      {props.robux && <img src='/img/test.png' alt='Robux Image'/>}
      {props.assetId && <ItemImage className='pt-1' id={props.assetId}/>}
    </div>
    {expanded && props.serialNumber && <p className={labelStyles.rapText + ' ' + labelStyles.serialText}>#{props.serialNumber}/{props.assetStock || '-'}</p>}
    {expanded && <TradeLabelWithRobux name='Avg. Price: ' amount={props.recentAveragePrice}/>}
    {expanded && <TradeLabelWithRobux name='Orig. Price: ' amount={props.originalPrice}/>}
  </div>
}

const TradeItemRow = ({ items, robux }) => {
  let placeholders = 5 - items.length;
  if (robux) {
    placeholders--;
  }
  return <div className='row ms-1 mb-4'>
    {
      items.map(v => {
        return <TradeItem key={v.id} {...v}/>
      })
    }
    {robux && <TradeItem robux={robux}/>}
    {
      [... new Array(placeholders)].map((v, i) => {
        return <TradeItem key={`placeholder ${i}`}/>
      })
    }
  </div>
}

export default TradeItemRow;