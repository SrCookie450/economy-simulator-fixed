import { useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { createOutfit } from "../../../services/avatar";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import OldModal from "../../oldModal";

const useStyles = createUseStyles({
  inputRow: {
    display: 'inline-block',
  },
  buttonsWrapper: {
    marginTop: '20px',
  },
});

const CreateOutfitModal = props => {
  const inputRef = useRef(null);
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const [disabled, setDisabled] = useState(false);
  const [error, setError] = useState(null);

  return <OldModal title="Create Outfit" height={error ? 120 : 110}>
    <div className={'col-12 ' + error ? 'mt-0' : 'mt-2'}>
      {error && <p className='mb-0 text-danger'>{error}</p>}
      <div className={s.inputRow}>
        <p className='mb-0'>Outfit Name:</p>
      </div>
      <div className={s.inputRow + ' ps-1'}>
        <input type='text' ref={inputRef} disabled={disabled}></input>
      </div>
    </div>
    <div className='col-8 offset-2'>
      <div className={s.buttonsWrapper}>
        <div className='row'>
          <div className='col-8 pe-1'>
            <ActionButton disabled={disabled} className={buttonStyles.buyButton} label="Create Outfit" onClick={() => {
              setDisabled(true);
              createOutfit({
                name: inputRef.current.value,
              }).then(() => {
                window.location.reload();
              }).catch((e) => {
                setError(e.message);
                setDisabled(false);
              })
            }}></ActionButton>
          </div>
          <div className='col-4 ps-1 pe-0'>
            <ActionButton disabled={disabled} className={buttonStyles.cancelButton} onClick={() => {
              props.onClose();
            }} label='Cancel'></ActionButton>
          </div>
        </div>
      </div>
    </div>
  </OldModal>
}

export default CreateOutfitModal;