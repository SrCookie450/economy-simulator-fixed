import {useEffect, useState} from "react";
import {getItemDetails, itemNameToEncodedName, searchCatalog} from "../../../services/catalog";
import ItemImage from "../../itemImage";
import Link from "../../link";
import Robux from "../../robux";
import Tickets from "../../tickets";

const GroupStore = props => {
  const {groupId} = props;
  const [items, setItems] = useState(null);
  const [cursor, setCursor] = useState(null);
  useEffect(() => {
    if (groupId) {
      searchCatalog({
        category: 'All',
        creatorType: 'Group',
        creatorId: groupId,
        sort: 'Updated',
        cursor: cursor,
        limit: 50,
      }).then(items => {
        getItemDetails(items.data.filter(v => v.itemType === 'Asset').map(v => v.id)).then(details => {
          items.data = details.data.data;
          setItems(items);
        })
      })
    }
  }, [groupId, cursor]);

  const canGoForwards = items && items.nextPageCursor;
  const canGoBackwards = items && items.previousPageCursor;

  // todo: i have no clue what this page actually looked like
  return <div className='row'>
    <div className='col-12'>
      {
        !items ? null : (!cursor && items.data.length === 0) ? <p className='mb-0 mt-4'>This group does not have any items for sale.</p> : <div className='row'>
          {
            items.data.map(v => {
              const url = `/catalog/${v.id}/${itemNameToEncodedName(v.name)}`;
              return <div className='col-6 col-lg-3'>
                <Link href={url}>
                  <a>
                    <ItemImage id={v.id} />
                  </a>
                </Link>
                <p className='mb-0 text-truncate'>
                  <Link href={url}>
                    <a>
                      {v.name}
                    </a>
                  </Link>
                </p>
                <div className='w-100'>
                  {
                    v.isForSale && v.price !== null ? <Robux>{v.price}</Robux> : null
                  }
                </div>
                <div className='w-100'>
                  {
                    v.isForSale && v.priceTickets !== null ? <Tickets>{v.priceTickets}</Tickets> : null
                  }
                </div>
              </div>
            })
          }
        </div>
      }

      {
        (canGoForwards || canGoForwards) ? <p className='mt-4 text-center'>
        <span className={canGoBackwards ? 'cursor-pointer' : ''} onClick={() => {
          if (canGoBackwards)
            setCursor(items.previousPageCursor);
        }}>Previous</span>
          <span>&emsp;</span>
          <span className={canGoForwards ? 'cursor-pointer' : ''} onClick={() => {
            if (canGoForwards)
              setCursor(items.nextPageCursor);
          }}>Next</span>
        </p> : null
      }
    </div>
  </div>
}

export default GroupStore;