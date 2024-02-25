import dayjs from "dayjs";
import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import {getItemDetails, getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import {getGroupTransactions, getTransactions} from "../../../services/economy";
import AuthenticationStore from "../../../stores/authentication";
import Robux from "../../catalogDetailsPage/components/robux";
import PlayerHeadshot from "../../playerHeadshot";
import Table from "./table";
import Link from "../../link";
import Tickets from "../../tickets";

const DeletedEntryMessage = props => {
  const [showMessage, setShowMessage] = useState(false);
  return <span>Deleted Item <span onClick={() => {
    setShowMessage(!showMessage);
  }} style={{ fontSize: '10px', cursor: 'pointer' }}>?</span>
    {showMessage && <p className='mt-2 mb-0'>{props.message}</p>}
  </span>
}

const DescriptionEntry = props => {
  const deletedMessage = `This item was deleted. For more details, contact customer support, and reference transaction ID: #${props.id || '-'}`;
  let verb = '';
  let link = '';
  /**@type {string | JSX.Element} */
  let noun = '';
  if (props.assetDetails) {
    noun = props.assetDetails.name;
    link = getItemUrl({assetId: props.assetDetails.id, name: props.assetDetails.name});
  }
  if (props.transactionType === 'Purchase') {
    verb = 'Purchased';
    if (!noun && props.details) {
      if (props.details.type === 'RobloxProduct') {
        noun = props.details.name;
      }
    }
  } else if (props.transactionType === 'Sale') {
    verb = 'Sold';
    if (props.details && props.details.name && !noun) {
      noun = props.details.name;
    }
  }
  if (!noun) {
    // item was deleted ?
    noun = <DeletedEntryMessage message={deletedMessage}/>
  }
  // console.log(props)
  return <span>
    {verb} {link ? <Link href={link}><a>{noun}</a></Link> : noun}
  </span>;
}

const useSellerStyles = createUseStyles({
  playerHeadshot: {
    width: '30px',
    height: '30px',
    border: '1px solid #c3c3c3',
    borderRadius: '100%',
    overflow: 'hidden',
    float: 'left',
  },
  sellerName: {
    marginLeft: '4px',
    float: 'left',
    marginTop: '7px',
    color: '#000',
  },
});

const SellerEntry = props => {
  const s = useSellerStyles();
  if (props.type === 'Group') {
    // TODO
    return <div>{props.name}</div>
  }
  return <div>
    <Link href={`/users/${props.id}/profile`}>
      <a>
        <div className={s.playerHeadshot}>
          <PlayerHeadshot id={props.id} name={props.name}/>
        </div>
        <div className={s.sellerName}>
          {props.name}
        </div>
      </a>
    </Link>
  </div>
}

const useStyles = createUseStyles({
  inline: {
    display: 'inline-block',
  },
  more: {
    textAlign: 'center',
    marginTop: '10px',
    cursor: 'pointer',
  },
})

const applyAssetDetails = (transactionsArray, detailsArray) => {
  for (const item of transactionsArray) {
    item.assetDetails = detailsArray.find(v => v.id === item.details.id)
  }
}

/**
 * Transactions table
 * @param props {{creatorType: string, creatorId: number, hideTransactionTypeSelector: boolean}}
 * @returns {JSX.Element}
 * @constructor
 */
const MyTransactionsTable = props => {
  const hideSelector = props.hideTransactionTypeSelector === true;

  const s = useStyles();
  const [type, setType] = useState(props.creatorType === 'Group' ? 'sale' : 'purchase');
  const [cursor, setCursor] = useState(null);
  const [entries, setEntries] = useState(null);
  const auth = AuthenticationStore.useContainer();
  useEffect(() => {
    if (auth.isPending) return;
    let f = props.creatorType === 'User' ? getTransactions({
        cursor,
        type: type,
        userId: auth.userId,
      }) :getGroupTransactions({
      cursor,
      type,
      groupId: props.creatorId,
    });

    f.then(values => {
      let existing = entries;
      if (!existing) {
        existing = {
          nextPageCursor: null,
          data: [],
        };
      }
      let ref = [];
      let assetIds = values.data.filter(v => {
        return v.details && typeof v.details.id === 'number';
      }).map(v => { ref.push(v); return v.details.id });
      if (assetIds.length > 0) {
        getItemDetails(assetIds).then(itemDetails => {
          applyAssetDetails(ref, itemDetails.data.data);
          existing.nextPageCursor = values.nextPageCursor;
          existing.data = [...existing.data, ...values.data];
          setEntries({ ...existing });
        })
      } else {
        existing.nextPageCursor = values.nextPageCursor;
        existing.data = [...existing.data, ...values.data];
        setEntries({ ...existing });
      }
    })
  }, [cursor, auth.userId, auth.isPending, type, props]);

  return <div className='row'>
    {!hideSelector ? <div className='col-12 mb-3 mt-3'>
      <div className={s.inline}>
        <p className='mb-0 fw-700 lighten-1 pe-2'>Transaction Type: </p>
      </div>
      <div className={s.inline}>
        <select value={type} onChange={(e) => {
          setType(e.currentTarget.value);
          setCursor(null);
          setEntries(null);
        }}>
          <option value='purchase'>Purchases</option>
          <option value='sale'>Sales</option>
          <option value='commision'>Commisions</option>
          <option value='adrevenue'>Ad Revenue</option>
          <option value='group-payout'>Group Payouts</option>
        </select>
      </div>
    </div> : null}
    <div className='col-12'>
      <Table
        keys={
          [
            'Date',
            'Member',
            'Description',
            'Amount',
          ]
        }
        entries={entries && entries.data && entries.data.map(v => {
          return [
            dayjs(v.created).format('M/D/YY'),
            <SellerEntry key={v.id} {...v.agent}></SellerEntry>,
            <DescriptionEntry {...v}></DescriptionEntry>,
            v.currency.amount === 0 ? '0' : 
            (v.currency.type === 'Tix' || v.currency.type === 'Tickets') ? 
            <Tickets>{v.currency.amount}</Tickets> : 
            <Robux>{v.currency.amount}</Robux>
          ];
        })}></Table>
    </div>
    <div className='col-12'>
      {
        entries && entries.nextPageCursor && <p className={s.more} onClick={() => {
          setCursor(entries.nextPageCursor);
        }}>Load More</p>
      }
    </div>
  </div>
}

export default MyTransactionsTable;