import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { getCollectibleOwners } from "../../../services/inventory";
import ActionButton from "../../actionButton";
import CreatorLink from "../../creatorLink";
import GenericPagination from "../../genericPagination";
import PlayerImage from "../../playerImage";

const Owners = props => {
  const { assetId } = props;
  const [page, setPage] = useState(1);
  const [cursor, setCursor] = useState(null);
  const [owners, setOwners] = useState(null);
  const [locked, setLocked] = useState(false);
  const [feedback, setFeedback] = useState(null);

  useEffect(() => {
    setLocked(true);
    setFeedback(null);

    getCollectibleOwners({
      assetId,
      cursor,
      sort: 'Asc',
      limit: 50,
    }).then(d => {
      setOwners(d);
    }).catch(e => {
      setFeedback(e.message);
    }).finally(() => {
      setLocked(false);
    })
  }, [cursor]);

  return <div className='row'>
    <div className='col-12'>
      {
        feedback ? <p className='mb-4 mt-4 text-danger'>{feedback}</p> : null
      }
      {owners && owners.data.map(v => {
        const owner = v.owner;

        return <div key={v.userAssetId} className='row'>
          <div className='col-2'>
            {owner ? <PlayerImage id={owner.id} name={owner.name}/> : null}
          </div>
          <div className='col-8'>
            <p className='mb-0 mt-2'>{owner ? <CreatorLink id={owner.id} name={owner.name} type='User'/> : 'Deleted/Private'}</p>
            <p className='mt-1 mb-0'>Serial {v.serialNumber ? '#' + v.serialNumber : 'N/A'}</p>
            <p className='mt-1 mb-0'>Updated {dayjs(v.updated).fromNow()}</p>
            {
              !owner ? <div className='mb-4' /> : null
            }
          </div>
          <div className='col-2'>
            <div className='mt-4'>
              {owner ? <ActionButton label='Trade' onClick={() => {
                window.open("/Trade/TradeWindow.aspx?TradePartnerID=" + owner.id, "_blank", "scrollbars=0, height=608, width=914");
              }}/> : null}
            </div>
          </div>
        </div>
      })}
    </div>
    <div className='col-12 col-lg-6 mx-auto'>
      {owners && (owners.nextPageCursor || owners.previousPageCursor) ? <GenericPagination page={page} onClick={newPage => {
        return e => {
          e.preventDefault();
          if (newPage === 1) {
            if (!owners.nextPageCursor || locked) return
            setCursor(owners.nextPageCursor);
            setPage(page + 1);
          } else if (newPage === -1) {
            if (!owners.previousPageCursor || locked) return;
            setCursor(owners.previousPageCursor);
            setPage(page - 1);
          }
        }
      }}/> : null}
    </div>
  </div>
}

export default Owners;