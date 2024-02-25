import { useEffect } from "react";
import { createUseStyles } from "react-jss";
import { toggleArchiveStatus, toggleReadStatus } from "../../../services/privateMessages";
import MyMessagesStore from "../../../stores/myMessages";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import MessageEntry from "./messageEntry";
import MessagePagination from "./messagePagination";

const useStyles = createUseStyles({
  row: {
    border: '1px solid #c3c3c3',
  },
  buttonWrapper: {
    marginTop: '3px',
    float: 'left',
  },
  button: {
    fontSize: '16px',
  },
  buttonMarkUnread: {
    width: '175px',
  },
  buttonArchive: {
    width: '100px',
  },
  buttonMarkRead: {
    width: '125px',
  },
})

const MessageRow = props => {
  const s = useStyles();
  const buttonStyles = useButtonStyles();
  const store = MyMessagesStore.useContainer();

  useEffect(() => {
    if (store.tab !== props.tab) {
      store.setOffset(0);
      store.setTotal(null);
      store.setTab(props.tab);
    }
  });
  const archiveOrUnarchive = props.tab === 'archive' ? 'Unarchive' : 'Archive';
  const showButtons = store.tab !== 'sent' && store.tab !== 'notifications';
  const reverseSender = store.tab === 'sent';

  const toggleReadStatusClick = readStatus => {
    return (e) => {
      e.preventDefault();
      toggleReadStatus({
        messageIds: store.checked.map(v => v.id),
        isRead: readStatus,
      }).then(() => {
        let newMessages = store.messages.map(v => {
          if (store.checked.find(x => x.id === v.id)) {
            v.isRead = readStatus;
          }
          return v;
        })
        store.setMessages(newMessages);
        store.setChecked([]);
      })
    }
  }

  if (!store.messages) return null;
  return <div className='row'>
    <div className='col-12 col-lg-6 mt-2'>
      {showButtons &&
        <>
          <div className={s.buttonWrapper}>
            <input type='checkbox' className='mt-2 me-2' checked={store.messages && store.messages.length !== 0 && store.checked.length === store.messages.length || false} onChange={e => {
              let checked = e.currentTarget.checked;
              if (!checked) {
                return store.setChecked([]);
              }
              let newArr = [...store.checked];
              store.messages.forEach(v => {
                let exists = store.checked.find(c => { return c.id === v.id });
                if (!exists) {
                  newArr.push({
                    id: v.id,
                    read: v.read,
                    archived: store.tab === 'archive',
                  });
                }
              })
              store.setChecked(newArr);
            }}></input>
          </div>
          <div className={s.buttonWrapper + ' ms-1 ' + s.buttonArchive}>
            <ActionButton disabled={store.checked.length === 0} label={archiveOrUnarchive} className={buttonStyles.continueButton + ' ' + s.button} onClick={(e) => {
              toggleArchiveStatus({
                messageIds: store.checked.map(v => v.id),
                isArchived: archiveOrUnarchive === 'Archive',
              }).then(() => {
                store.getMessagesForTab(store.tab);
                store.setChecked([]);
              });
            }}></ActionButton>
          </div>
          <div className={s.buttonWrapper + ' ms-1 ' + s.buttonMarkRead}>
            <ActionButton disabled={store.checked.length === 0} label='Mark as Read' className={buttonStyles.continueButton + ' ' + s.button} onClick={toggleReadStatusClick(true)}></ActionButton>
          </div>
          <div className={s.buttonWrapper + ' ms-1 ' + s.buttonMarkUnread}>
            <ActionButton disabled={store.checked.length === 0} label='Mark as Unread' className={buttonStyles.continueButton + ' ' + s.button} onClick={toggleReadStatusClick(false)}></ActionButton>
          </div>
        </>
      }
    </div>
    <div className='col-12 col-lg-6'>
      <MessagePagination></MessagePagination>
    </div>
    <div className={`col-12 ${s.row}`}>
      {
        store.messages.map(v => {
          const userData = reverseSender ? v.recipient : v.sender;
          if (!userData) return null; // can be undefined when switch from Notifications tab to Sent tab

          return <MessageEntry key={v.id}
            fromUserId={userData.id}
            fromUserName={userData.name}
            body={v.body}
            subject={v.subject}
            created={v.updated}
            read={reverseSender ? true : v.isRead} // prevent "is read" request from being sent, which will always fail
            archived={store.tab === 'archive'}
            id={v.id}
          ></MessageEntry>
        })
      }
    </div>
  </div>
}

export default MessageRow;
