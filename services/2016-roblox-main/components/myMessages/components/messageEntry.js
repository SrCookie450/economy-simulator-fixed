import { createUseStyles } from "react-jss";
import MyMessages from "..";
import MyMessagesStore from "../../../stores/myMessages";
import PlayerImage from "../../playerImage"

const useStyles = createUseStyles({
  username: {
    fontWeight: 700,
    color: '#828282',
    marginBottom: 0,
  },
  subject: {
    fontWeight: 700,
    color: '#828282',
  },
  subjectUnread: {
    color: '#000',
  },
  subjectBodyParagraph: {
    marginBottom: 0,
    whiteSpace: 'nowrap',
    textOverflow: 'ellipsis',
    overflow: 'hidden',
  },
  body: {
    color: '#969696',
    fontWeight: 500,
  },
  messageRow: {
    cursor: 'pointer',
  },
  userImage: {
    maxWidth: '100%',
    display: 'inline-block',
    position: 'relative',
    marginTop: '-15px',
    paddingLeft: '15px',
  },
  markRead: {
    zIndex: 99,
    cursor: 'pointer',
  },
  markReadWrapper: {
    display: 'inline-block',
    width: '10px',
    position: 'relative',
    top: '20px',
    zIndex: 99,
  },

  userCheckAndImage: {
    display: 'inline-block',
    width: '65px',
  },
  subjectAndContent: {
    display: 'inline-block',
    width: 'calc(100% - 65px)',
    verticalAlign: 'super',
  },
})

/**
 * Message Entry
 * @param {{fromUserId: number; fromUserName: string; subject: string; body: string; created: string; id:number; read: boolean; archived: boolean;}} props
 * @returns 
 */
const MessageEntry = props => {
  const s = useStyles();
  const store = MyMessagesStore.useContainer();
  const isChecked = store.checked.find(v => v.id === props.id) !== undefined;
  return <div className={`pt-2 ${s.messageRow}`} onClick={(e) => {
    e.preventDefault();
    store.setHighlightedMessage(props);
  }}>
    <div className={s.userCheckAndImage}>
      <div className={s.markReadWrapper}>
        <input type='checkbox' checked={isChecked} className={s.markRead} onChange={() => {}} onClick={(e) => {
          e.stopPropagation();
          if (isChecked) {
            store.setChecked(store.checked.filter(v => {
              return v.id !== props.id;
            }));
          } else {
            store.setChecked([...store.checked, props]);
          }
        }}/>
      </div>
      <div className={s.userImage}>
        <PlayerImage id={props.fromUserId}/>
      </div>
    </div>
    <div className={s.subjectAndContent}>
      <p className={s.username}>{props.fromUserName}</p>
      <p className={s.subjectBodyParagraph}><span className={s.subject + ' ' + (props.read ? '' : s.subjectUnread)}>{props.subject}</span> - <span className={s.body}>{props.body}</span></p>
    </div>
    <div >
      <div className='divider-top'/>
    </div>
  </div>
}

export default MessageEntry;