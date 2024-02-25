import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { changePassword, changeUsername, validateUsername } from "../../../services/auth";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import MyAccountStore from "../stores/myAccountStore";
import useModalStyles from "../styles/modal";

const ModalChangeUsername = props => {
  const store = MyAccountStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const [pass, setPass] = useState('');
  const [username, setUsername] = useState('');
  const [feedback, setFeedback] = useState(null);
  const [locked, setLocked] = useState(null);

  useEffect(() => {
    if (feedback) {
      setFeedback(null);
    }
  }, [username, pass]);

  const s = useModalStyles();
  return <div className='row ps-4'>
    <div className='col-12'>
      {feedback && <p className='text-danger mb-0'>{feedback}</p>}
      <input disabled={locked} type='text' className={s.input} placeholder='Desired Username (3-20 characters)' value={username} onChange={(e) => setUsername(e.currentTarget.value)}></input>
      <input disabled={locked} type='password' className={s.input} placeholder='Current Password' value={pass} onChange={(e) => setPass(e.currentTarget.value)}></input>
      <div className={s.confirmWrapper + ' mt-4 ' + s.confirmWrapperWide}>
        <ActionButton disabled={locked || !username || !pass} label='Buy for R$1,000' onClick={() => {
          if (username === auth.username) {
            setFeedback('Username is already in use')
            return
          }
          setFeedback(null);
          setLocked(true);
          validateUsername({
            username,
            context: 'UsernameChange',
          }).then(d => {
            if (d.code !== 0) {
              setLocked(false);
              return setFeedback(d.message);
            }
            changeUsername({
              username,
              password: pass,
            })
              .then(() => {
                store.setModalMessage({
                  title: 'Success',
                  message: 'Successfully changed username.',
                });
                store.setModal('MODAL_OK');
              })
              .catch(e => {
                setFeedback(e.response?.data?.errors[0]?.message || e.message);
              })
              .finally(() => {
                setLocked(false)
              })
          }).catch((e) => {
            setLocked(false);
          })
        }}></ActionButton>
      </div>
      <div className='mt-4'>
        <p><span className='fw-700'>Important: </span> Original account creation date and forum post count will carry over to your new username. Previous forum posts will appear under your old username and will NOT carry over to your new username.</p>
      </div>
    </div>
  </div>
}

export default ModalChangeUsername;