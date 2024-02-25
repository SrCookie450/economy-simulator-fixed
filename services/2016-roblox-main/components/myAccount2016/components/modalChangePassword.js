import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { changePassword } from "../../../services/auth";
import ActionButton from "../../actionButton";
import MyAccountStore from "../stores/myAccountStore";
import useModalStyles from "../styles/modal";

const ModalChangePassword = props => {
  const store = MyAccountStore.useContainer();
  const [newPass, setNewPass] = useState('');
  const [oldPass, setOldPass] = useState('');
  const [newPassConfirm, setNewPassConfirm] = useState('');
  const [feedback, setFeedback] = useState(null);
  const [locked, setLocked] = useState(null);

  useEffect(() => {
    if (feedback) {
      setFeedback(null);
    }
  }, [newPass, oldPass, newPassConfirm]);

  const s = useModalStyles();
  return <div className='row ps-4'>
    <div className='col-12'>
      {feedback && <p className='text-danger mb-0'>{feedback}</p>}
      <input disabled={locked} type='password' className={s.input} placeholder='Current Password' value={oldPass} onChange={(e) => setOldPass(e.currentTarget.value)}></input>
      <input disabled={locked} type='password' className={s.input} placeholder='New Password' value={newPass} onChange={(e) => setNewPass(e.currentTarget.value)}></input>
      <input disabled={locked} type='password' className={s.input} placeholder='Confirm Password' value={newPassConfirm} onChange={(e) => setNewPassConfirm(e.currentTarget.value)}></input>
      <div className={s.confirmWrapper}>
        <ActionButton disabled={locked || !newPass || !newPassConfirm || !oldPass} label='Update' onClick={() => {
          setFeedback(null);
          if (newPass !== newPassConfirm) {
            setFeedback('Your new and confirm passwords do not match.');
            return
          }
          setLocked(true);
          changePassword({
            newPassword: newPass,
            existingPassword: oldPass,
          }).then(() => {
            store.setModalMessage({
              title: 'Password Updated',
              message: 'Your password has been updated.',
            });
            store.setModal('MODAL_OK');
          }).catch(e => {
            setFeedback(e.response?.data?.errors[0]?.message || e.message);
            console.error(e.response.data);
          }).finally(() => {
            setLocked(false);
          })
        }}></ActionButton>
      </div>
    </div>
  </div>
}

export default ModalChangePassword;