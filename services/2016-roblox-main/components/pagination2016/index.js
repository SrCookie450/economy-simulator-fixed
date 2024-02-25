import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  buttonWrapper: {
    margin: '0 auto',
    display: 'flex',
    justifyContent: 'center',
    maxWidth: '300px',
  },
  buttonPaginate: {
    color: '#1a1a1a',
    background: '#fff',
    border: '1px solid #dbdbdb',
    padding: '0px 12px',
    fontSize: '24px',
    fontWeight: 600,
    borderRadius: '4px',
    marginBottom: 0,
    height: '34px',
    cursor: 'pointer',
  },
  textCurrentPage: {
    margin: '8px 16px 0 16px',
    fontWeight: '300',
  },
  buttonPaginateDisabled: {
    color: '#c3c3c3',
    cursor: 'default',
  },
});

const Paging = props => {
  const {totalItems, limit, page, nextPageAvailable, previousPageAvailable, loadPreviousPage, loadNextPage} = props;

  const s = useStyles();
  const totalDisplay = totalItems ? ('of '+ Math.ceil((totalItems) / limit)) : (nextPageAvailable() ? 'of many' : null);

  return <div className='row'>
    <div className='col-12'>
      <div className={s.buttonWrapper}>
        <div className={s.buttonPaginate + ' ' + (!previousPageAvailable() ? s.buttonPaginateDisabled : '')} onClick={() => {
          if (previousPageAvailable())
            loadPreviousPage();
        }}>
          <p>{'<'}</p>
        </div>
        <p className={s.textCurrentPage}>Page {page} {totalDisplay}</p>
        <div className={s.buttonPaginate + ' ' + (!nextPageAvailable() ? s.buttonPaginateDisabled : '')} onClick={() => {
          if (nextPageAvailable())
            loadNextPage();
        }}>
          <p>{'>'}</p>
        </div>
      </div>
    </div>
  </div>
}

export default Paging;