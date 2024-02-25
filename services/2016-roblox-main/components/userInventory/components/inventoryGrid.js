import userInventoryStore from "../stores/userInventoryStore";
import {createUseStyles} from "react-jss";
import InventoryItemEntry from "./inventoryItemEntry";
import Paging from "./paging";
import getFlag from "../../../lib/getFlag";

const useStyles = createUseStyles({
  categoryValue: {
    fontWeight: 300,
    fontSize: '24px',
  },
  showingLabel: {
    fontWeight: 400,
    color: '#757575',
  },
})

const InventoryGrid = props => {
  const s = useStyles();
  const store = userInventoryStore.useContainer();
  const myPage = store.data ? store.data.Page : null;
  const isEmpty = store.data && store.data.Items.length === 0 && !store.previousPageAvailable();
  const showPaging = store.data && !isEmpty;

  let total = store.data ? (store.data?.TotalItems?.toLocaleString() || 'many') : null; // roblox started returning "null" for TotalItems :(
  if (myPage === 1 && store.data.Items.length <= 24) {
    total = '1';
  }

  return <div className='col-12 col-lg-10'>
    <div className='row'>
      <div className='col-12 pe-1 ps-1'>
        <h2 className={s.categoryValue}>{store.category.name.toUpperCase()}</h2>
        <p className={s.showingLabel}>{myPage ? `Showing ${myPage} to ${store.data.Items.length} of ${total}` : null}</p>
      </div>
      <div className='col-12'>
        <div className='row'>
          {store.data ? store.data.Items.map(v => {
              const isLimited = v.AssetRestrictionIcon.CssTag === 'limited';
              const isLimitedUnique = v.AssetRestrictionIcon.CssTag === 'limited-unique';
              const serialNumber = v.Product.SerialNumber;
              return <InventoryItemEntry key={v.Item.AssetId + ' ' + serialNumber} id={v.Item.AssetId} name={v.Item.Name} creatorId={v.Creator.Id} creatorType={v.Creator.Type} creatorName={v.Creator.Name} isLimited={isLimited} isLimitedUnique={isLimitedUnique} serialNumber={serialNumber} />
            })
            : null}
        </div>
        {isEmpty ? <p className='text-center mt-4'>Player does not have any items in this category.</p> : null}
        {showPaging ? <Paging /> : null}
      </div>
    </div>
  </div>
}

export default InventoryGrid;