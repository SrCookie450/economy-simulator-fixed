import UserInventoryStore from "../stores/userInventoryStore";
import Paging from "../../pagination2016";

const InventoryPaging = props => {
  const store = UserInventoryStore.useContainer();
  if (!store.data)
    return null;

  return <Paging page={store.data.Page} totalItems={store.data.TotalItems} limit={store.limit} nextPageAvailable={store.nextPageAvailable} previousPageAvailable={store.previousPageAvailable} loadNextPage={store.loadNextPage} loadPreviousPage={store.loadPreviousPage} />
}

export default InventoryPaging;