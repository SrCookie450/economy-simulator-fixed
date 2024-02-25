import styles from './chatEntry.module.css';
import PlayerHeadshot from "../../playerHeadshot";
import chatStore from "../chatStore";

const ChatEntry = props => {
  const store = chatStore.useContainer();
  const {user, conversationId, latestMessage, hasUnread} = props;
  const isUnread = hasUnread;

  const onClick = e => {
    e.preventDefault();
    let existing = [...store.selectedConversation.filter(v => {
      if (v.conversationId === null) {
        return v.user.id !== user.id;
      }
      return v.conversationId !== conversationId;
    })];
    if (existing.length >= 3) {
      existing = existing.slice(0,2);
    }
    existing.unshift({user, conversationId, latestMessage});
    store.setSelectedConversation(existing);
  }

  return <div className={styles.chatEntry} onClick={onClick}>
    <div className={styles.chatHeadshotColumn}>
      <div className={styles.chatHeadshot}>
        <div className={styles.chatHeadshotImage}>
          <PlayerHeadshot id={user.id} name={user.username} />
        </div>
      </div>
    </div>
    <div className={styles.chatLatestMessage}>
      <p className={styles.chatUsername}>{user.username}</p>
      <p className={styles.chatMessage + ' text-truncate ' + (isUnread ? styles.chatMessageUnread : '')}>
        {
          user.isTyping ? <span className='fst-italic'>Typing...</span> : (
            latestMessage ? latestMessage.content : <span>&emsp;</span>
          )
        }
      </p>
    </div>
  </div>
}

export default ChatEntry;