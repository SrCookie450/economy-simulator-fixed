import { useRouter } from "next/dist/client/router";
import React, { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import ActionButton from "../../components/actionButton";
import Header from "../../components/header";
import { sendMessage } from "../../services/privateMessages";
import { getUserInfo } from "../../services/users";
import useButtonStyles from "../../styles/buttonStyles";

const useStyles = createUseStyles({
  box: {
    border: '1px solid #c3c3c3',
  },
  input: {
    width: '100%',
    fontSize: '16px',
    border: '1px solid #c3c3c3',
    padding: '5px 10px',
  },
  textArea: {
    background: '#eaeaea',
  },
  sendWrapper: {
    width: '100px',
    float: 'right',
  },
  messagesContainer: {
    backgroundColor: '#fff',
  },
});

const ComposeMessagePage = props => {
  const buttonStyles = useButtonStyles();
  const s = useStyles();

  const router = useRouter();
  const userIdStr = router.query['recipientId'];
  const [error, setError] = useState(null);
  const [userInfo, setUserInfo] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    if (typeof userIdStr !== 'string') return
    let userId = parseInt(userIdStr, 10);
    getUserInfo({ userId }).then(result => {
      setUserInfo(result)
    })
  }, [userIdStr]);

  const subjectRef = useRef(null);
  const bodyRef = useRef(null);

  if (!userInfo) return null;
  return <div className={'container ' + s.messagesContainer}>
    <div className='row'>
      <div className='col-12'>
        <Header>New Message</Header>
        <p className={error ? 'mb-0' : 'mb-4'}><a href='#' onClick={(e) => {
          e.preventDefault();
          window.history.back();
        }}>Back</a></p>
        {error && <p className='mb-1 text-danger'>{error}</p>}
      </div>
    </div>
    <div className='row'>
      <div className='col-12 mt-4'>
        <input className={s.input} readOnly={true} value={'To: ' + userInfo.name}></input>
        <input ref={subjectRef} maxLength={256} className={s.input + ' mt-2'} placeholder='Subject:'></input>
        <textarea ref={bodyRef} maxLength={1000} className={s.input + ' mt-2 ' + s.textArea} rows={8} placeholder='Write your message...'></textarea>
        <div className={s.sendWrapper}>
          <ActionButton disabled={locked} label='Send' className={buttonStyles.continueButton} onClick={() => {
            if (locked) return;

            setError(null);
            setLocked(true);
            sendMessage({
              userId: userInfo.id,
              subject: subjectRef.current.value,
              body: bodyRef.current.value,
              replyMessageId: undefined,
              includePreviousMessage: undefined,
            }).then(() => {
              router.push('/My/Messages');
            }).catch(e => {
              setLocked(false);
              setError(e.message);
            })
          }}></ActionButton>
        </div>
      </div>
    </div>
  </div>
}

export default ComposeMessagePage;