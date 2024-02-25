import dayjs from "dayjs";
import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { sendMessage, toggleReadStatus } from "../../../services/privateMessages";
import AuthenticationStore from "../../../stores/authentication";
import MyMessagesStore from "../../../stores/myMessages";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import Header from "../../header";
import PlayerImage from "../../playerImage"
import Link from "../../link";

const useStyles = createUseStyles({
  username: {
    fontWeight: 700,
    marginBottom: 0,
  },
  created: {
    marginBottom: 0,
    color: '#969696',
    fontWeight: 500,
  },
  subject: {
    fontWeight: 400,
  },
  subjectBodyParagraph: {
    marginBottom: 0,
  },
  body: {
    whiteSpace: 'break-spaces',
  },
  backButtonWrapper: {
    width: '80px',
    float: 'left',
  },
  replyButtonWrapper: {
    width: '110px',
    float: 'left',
    marginLeft: '10px',
  },
  archiveButtonWrapper: {},
  replyText: {
    width: '100%',
    padding: '8px 10px',
    background: '#eaeaea',
    border: '1px solid #666',
  },
  replyWrapper: {
    width: '170px',
    float: 'right',
  },
})

/**
 * Message Entry
 * @param {{fromUserId: number; fromUserName: string; subject: string; body: string; created: string; id: number; read: boolean;}} props
 * @returns 
 */
const LargeMessage = props => {
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const store = MyMessagesStore.useContainer();
  const [reply, setReply] = useState(false);
  const replyInputRef = useRef(null);
  const showReplyButton = props.fromUserId !== 1;
  const auth = AuthenticationStore.useContainer();
  const [feedback, setFeedback] = useState(null);

  useEffect(() => {
    if (props.id !== 1 && props.read !== true && props.fromUserId !== auth.userId) {
      toggleReadStatus({
        messageIds: [props.id],
        isRead: true,
      })
    }
  }, []);

  useEffect(() => {
    setFeedback(null);
  }, [props]);

  return <div className='row pt-2'>
    <div className='col-12'>
      <div className={s.backButtonWrapper}>
        <ActionButton label='Back' className={buttonStyles.cancelButton} onClick={() => {
          store.setHighlightedMessage(null);
        }}/>
      </div>
      {showReplyButton && <div className={s.replyButtonWrapper}>
        <ActionButton label='Reply' className={buttonStyles.continueButton} onClick={() => {
          setReply(!reply);
        }}/>
      </div>
      }
    </div>
    <div className='col-12'>
      <h2 className={s.subject}>{props.subject}</h2>
      {feedback ? <p className='text-danger'>{feedback}</p> : null}
    </div>
    <div className='col-1 pe-0'>
      <PlayerImage id={props.fromUserId}/>
    </div>
    <div className='col-10'>
      <p className={s.username}>
        <Link href={`/users/${props.fromUserId}/profile`}>
          <a className='normal'>
            {props.fromUserName}
          </a>
        </Link>
      </p>
      <p className={s.created}>{dayjs(props.created).format('MMM DD, YYYY | h:mm A')}</p>
    </div>
    <div className='col-12'>
      <p className={s.body + ' mt-2'}>
        {props.body}
      </p>
    </div>
    {
      reply && <div className='col-12'>
        <textarea ref={replyInputRef} maxLength={1000} className={s.replyText} rows={8} placeholder='Reply here...'>

        </textarea>
        <div className={s.replyWrapper}>
          <ActionButton label='Send Reply' className={buttonStyles.continueButton} onClick={() => {
            setFeedback(null);
            sendMessage({
              userId: props.fromUserId,
              subject: 'RE: ' + props.subject,
              body: replyInputRef.current.value,
              includePreviousMessage: true,
              replyMessageId: props.id,
            }).then((res) => {
              if (res.success) {
                store.setHighlightedMessage(null);
              }else{
                setFeedback(res.message || 'Unknown error. Try again.');
              }
            }).catch(e => {
              setFeedback(e.message);
            })
          }}/>
        </div>
      </div>
    }
  </div>
}

export default LargeMessage;