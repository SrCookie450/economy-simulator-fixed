import styles from "./chatMenu.module.css";
import {sendMessage, startOneToOneConversation, updateTypingStatus} from "../../../services/chat";
import {useRef} from "react";
import chatStore from "../chatStore";
import authentication from "../../../stores/authentication";

const ChatInput = props => {
  const store = chatStore.useContainer();
  const auth = authentication.useContainer();
  const {locked, setLocked, conversationId, addCreatedMessage, user} = props;
  const inputRef = useRef(null);
  const typingState = useRef({
    sentAt: 0,
    timer: null,
    broadcastTypingTimer: null,
  });

  return <input autoFocus={true} ref={inputRef} type='text' className={styles.chatMessageBox} placeholder='Send a message' onKeyDown={e => {
    if (locked) return;

    if (conversationId) {
      if (typingState.current.timer) {
        clearTimeout(typingState.current.timer);
      }
      typingState.current.timer = setTimeout(() => {
        clearInterval(typingState.current.broadcastTypingTimer);
        typingState.current.broadcastTypingTimer = null;
      }, 3000);

      if (!typingState.current.broadcastTypingTimer) {
        const updateStatus = () => {
          updateTypingStatus({
            conversationId,
            isTyping: true,
          });
        }
        typingState.current.broadcastTypingTimer = setInterval(() => {
          updateStatus();
        }, 1000);
        updateStatus();
      }
    }

    if (e.key === 'Enter') {
      if (typingState.current.broadcastTypingTimer) {
        clearInterval(typingState.current.broadcastTypingTimer);
        typingState.current.broadcastTypingTimer = null;
      }
      if (typingState.current.timer) {
        clearTimeout(typingState.current.timer);
        typingState.current.timer = null;
      }

      setLocked(true);
      const message = e.currentTarget.value;
      if (conversationId === null) {
        if (!message || message.length < 3 || message.length > 255)
          return; // Invalid message

        // We have to create a conversation, THEN send the message
        startOneToOneConversation({
          userId: user.id,
        }).then(d => {
          store.dispatchConversations({
            action: 'MULTI_ADD',
            data: [
              {
                id: d.conversation.id,
                hasUnreadMessages: false,
                participants: [
                  {
                    type: 'User',
                    targetId: auth.userId,
                    name: auth.username,
                  },
                  {
                    type: 'User',
                    targetId: user.id,
                    name: user.username,
                  }
                ],
                conversationType: 'OneToOneConversation',
                conversationTitle: {
                  titleForViewer: null,
                  isDefaultTitle: true,
                },
                conversationUniverse: null,
              },
            ],
          });
          sendMessage({
            conversationId: d.conversation.id,
            message: message,
          }).then((msg) => {
            addCreatedMessage(msg, d.conversation.id);
            // Update the ID
            store.selectedConversation.find(a => a.user.id === user.id).conversationId = d.conversation.id;
            store.setSelectedConversation([...store.selectedConversation]);
          }).catch(e => {
            setLocked(false);
            // todo: feedback
            console.error('[error] could not send message',e);
          })
        }).catch(e => {
          // todo: feedback
          setLocked(false);
          console.error('[error] could not create conversation',e);
        })
      }else{
        // Just send the message like normal
        sendMessage({
          conversationId: conversationId,
          message: message,
        }).then((msg) => {
          addCreatedMessage(msg);
          inputRef.current.value = '';
          setLocked(false);
        }).catch(e => {
          //todo: feedback
          setLocked(false);
          console.error('[error] could not send message', e);
        })
      }
    }
  }} />
}

export default ChatInput;