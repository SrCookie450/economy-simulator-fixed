import CatalogPageStore from "../../stores/catalogPage";
import GenericPagination from "../genericPagination";

/**
 * Catalog pagination component
 * @param {*} props 
 */
const CatalogPagination = props => {
  const store = CatalogPageStore.useContainer();
  const { limit, total, page } = store;

  let pageCount = typeof total === 'number' ? Math.ceil(total / limit) : null;
  if (pageCount === 0) {
    pageCount = 1;
  }

  const onClick = increment => {
    return (e) => {
      e.preventDefault();
      if (store.locked) {
        return;
      }
      let cursor = '';
      if (increment === 1) {
        if (pageCount !== null && page >= pageCount) return;
        store.setPage(page + 1);
        cursor = store.nextCursor;
      } else if (increment === -1) {
        if (page === 1) return;
        store.setPage(page - 1);
        cursor = store.previousCursor;
      }
      store.setCursor(cursor);
    }
  }


  return <div className='row'>
    <div className='col-6 col-lg-3 mx-auto'>
      <GenericPagination
        page={page}
        pageCount={pageCount}
        onClick={onClick}
      />
    </div>
  </div>
}

export default CatalogPagination;