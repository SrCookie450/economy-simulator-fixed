import AccountInfoStore from '../stores/myAccountStore';
import useFormStyles from '../styles/forms';

const GenderSelection = props => {
  const formStyles = useFormStyles();
  const store = AccountInfoStore.useContainer();
  const { id, displayName } = props;
  return <div className='col ps-0 pe-0'>
    <div className={'card ' + formStyles.fakeInput}>
      <p className={'text-center mb-2 mt-2 ' + (store.gender !== id ? formStyles.genderUnselected : '')} onClick={() => {
        store.setGender(id);
      }}>{displayName}</p>
    </div>
  </div>
}

export default GenderSelection;