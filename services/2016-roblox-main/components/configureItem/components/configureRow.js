import configureItemStore from "../stores/configureItemStore";
import {createUseStyles} from "react-jss";
import ItemImage from "../../itemImage";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";

const useStyles = createUseStyles({
  itemImage: {
    width: '100%',
    border: '1px solid #a8a8a8',

  }
});

const ConfigureRow = props => {
  const store = configureItemStore.useContainer();
  const s = useStyles();
  const btnStyles = useButtonStyles();

  return <div className='row'>
    <div className='col-12 col-lg-6 offset-lg-3'>
      <p className='fw-bolder ms-0 mb-1'>Name:</p>
      <input disabled={store.locked} type='text' className='ps-2 w-100' value={store.name || ''} onChange={(e) => {
        store.setName(e.currentTarget.value)
      }} />

      <div className={s.itemImage + ' mt-4'}>
        <ItemImage className='mx-auto d-block' id={store.assetId} />
      </div>

      <p className='fw-bolder ms-0 mb-1 mt-4'>Description:</p>
      <textarea disabled={store.locked} className='ps-2 w-100' rows={6} value={store.description || ''} onChange={e => {
        store.setDescription(e.currentTarget.value);
      }} />

      <div className='row mt-4'>
        <div className='col-6'>
          <ActionButton disabled={store.locked} className={'float-right ' + btnStyles.continueButton + ' ' + btnStyles.normal} label='Save' onClick={e => {
            e.preventDefault();
            store.save();
          }}/>
        </div>
        <div className='col-6'>
          <ActionButton disabled={store.locked} className={'float-left ' + btnStyles.cancelButton  + ' ' + btnStyles.normal} label='Cancel' onClick={e => {
            // todo: what did the cancel button even do?
            window.location.reload();
          }} />
        </div>
      </div>
    </div>
  </div>
}

export default ConfigureRow;