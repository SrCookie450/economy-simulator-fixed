import styles from './chatMenu.module.css';
import {useEffect, useState} from "react";
import chatStore from "../chatStore";
import ChatEntry from "./chatEntry";
import {getUserConversations} from "../../../services/chat";
import authentication from "../../../stores/authentication";
import dayjs from "dayjs";

const ChatMenuClosed = props => {
  const store = chatStore.useContainer();
  return <div className={styles.chatMenu}>
    <div className={styles.chatMenuHeader}>
      <p className={styles.chatLabel} onClick={() => {
        props.setMenuOpen(true);
      }}>Chat {
        store.unreadCount ? <span className={styles.unreadBubble}>{store.unreadCount}</span> : null
      }</p>
    </div>
  </div>
}

const ChatMenuOpen = props => {
  const store = chatStore.useContainer();
  const auth = authentication.useContainer();

  return <div className={styles.chatMenu}>
    <div className={styles.chatMenuHeader}>
      <p className={styles.chatLabel} onClick={() => {
        props.setMenuOpen(false);
      }}>Chat {
        store.unreadCount ? <span className={styles.unreadBubble}>{store.unreadCount}</span> : null
      }</p>
    </div>
    <div className={styles.chatMenuBody}>
      {
        store.conversations ? store.conversations.sort((a,b) => {
          return (a.latest && b.latest) ? (dayjs(a.latest.sent).isAfter(dayjs(b.latest.sent)) ? -1 : 1) : 0;
        }).filter(v => v.conversationType === 'OneToOneConversation').map(v => {
          const other = v.participants.find(o => o.targetId !== auth.userId);
          return <ChatEntry key={v.id} user={{
            id: other.targetId,
            username: other.name,
            isTyping: other.isTyping||false,
          }} conversationId={v.id} latestMessage={v.latest || null} hasUnread={v.hasUnreadMessages} />
        }) : null
      }
      {
        store.friends ? store.friends.filter(v => {
          return store.conversations.find(x => x.conversationType === 'OneToOneConversation' && x.participants.find(o => o.targetId === v.id)) === undefined;
          // return true;
        }).map(v => {
          return <ChatEntry user={{
            id: v.id,
            username: v.name,
            isTyping: false,
          }} conversationId={null} latestMessage={null} key={v.id} hasUnread={false} />
        }) : null
      }
    </div>
  </div>
}

const ChatMenu = props => {
  const [menuOpen, setMenuOpen] = useState(null);
  if (menuOpen) {
    return <ChatMenuOpen setMenuOpen={setMenuOpen} />
  }

  return <ChatMenuClosed setMenuOpen={setMenuOpen} />
}

export default ChatMenu;