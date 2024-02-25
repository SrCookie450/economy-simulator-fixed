import {useEffect, useRef} from "react";
import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../stores/catalogPage";
import buttonStyles from '../../styles/buttons.module.css';

const useInputStyles = createUseStyles({
  input: {
    width: '100%',
    padding: '2px',
    border: '1px solid #a7a7a7',
  },
  select: {
    width: '100%',
    padding: '2px',
    border: '1px solid #a7a7a7',
    paddingLeft: '2px',
    appearance: 'none',
  },
  col: {
    padding: 0,
    paddingLeft: '2px',
  },
  caret: {
    position: 'absolute',
    marginLeft: '-21px',
    fontSize: '8px',
    transform: 'rotate(90deg)',
    marginTop: '6px',
    border: '1px solid black',
    paddingLeft: '3px',
    paddingRight: '1px',
    background: 'linear-gradient(90deg, rgba(224,224,224,1) 0%, rgba(255,255,255,1) 100%)',
  },
})

/**
 * Catalog page search input
 * @param {*} props 
 */
const CatalogPageInput = props => {
  const s = useInputStyles();
  const store = CatalogPageStore.useContainer();

  const input = useRef(null);
  const category = useRef(null);

  useEffect(() => {
    // for keyword updates from url param
    if (input.current)
      input.current.value = store.query;
  }, [store.query]);

  return <div className='row'>
    <div className='col-12 col-lg-8 offset-lg-4'>
      <div className='row'>
        <div className={`col-12 col-md-6 ${s.col}`}>
          <input type='text' className={`${s.input}`} ref={input}/>
        </div>
        <div className={`col-6 col-md-3 ${s.col}`}>
          <select disabled={store.locked} className={s.select} ref={category}>
            <option value='all'>All Categories</option>
          </select>
          <span className={s.caret}>►</span>
        </div>
        <div className={`col-6 col-md-2 ${s.col}`}>
          <button disabled={store.locked} className={`${buttonStyles.legacyButton} w-100`} onClick={(e) => {
            e.preventDefault();
            store.setQuery(input.current.value);
          }}>Search</button>
        </div>
      </div>
    </div>
  </div>
}

export default CatalogPageInput;