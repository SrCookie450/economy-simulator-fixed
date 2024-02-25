import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../stores/catalogPage";

const useStyles = createUseStyles({
  button: {
    color: '#777777',
    fontSize: '15px',
    border: '1px solid #777777',
    background: 'linear-gradient(0deg, rgba(224,224,224,1) 0%, rgba(255,255,255,1) 100%)',
    '&:hover': {
      background: 'linear-gradient(0deg, rgba(203,216,255,1) 0%, rgba(255,255,255,1) 100%)',
    },
    margin: '0 auto',
    display: 'block',
  },
  col: {
    paddingLeft: 0,
    paddingRight: 0,
  },
})

/**
 * Generic pagination component
 * @param {{onClick: (mode: number) => (e: any) => void; pageCount?: number; page: number;}} props
 */
const GenericPagination = props => {
  const { pageCount, page } = props;
  const s = useStyles();

  return <div className='row'>
    <div className='col-12'>
      <div className='row'>
        <div className={`${s.col} col-3`}>
          <button className={s.button} onClick={props.onClick(-1)}>◄</button>
        </div>
        <div className={`${s.col} col-6`}>
          <p className='mb-0 pl-2 pr-2 text-center'>
            Page {page} {typeof pageCount === 'number' && ' of ' + pageCount.toLocaleString() || ''}
          </p>
        </div>
        <div className={`${s.col} col-3`}>
          <button className={s.button} onClick={props.onClick(1)}>►</button>
        </div>
      </div>
    </div>
  </div>
}

export default GenericPagination;