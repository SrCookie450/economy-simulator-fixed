import ChatMenu from "./components/chatMenu";
import styles from './container.module.css';
import chatStore from "./chatStore";
import {useEffect, useRef} from "react";
import {getUserConversations, multiGetLatestMessages} from "../../services/chat";
import authentication from "../../stores/authentication";
import {getFriends} from "../../services/friends";
import ConversationEntry from "./components/conversationEntry";
import * as signalR from "@microsoft/signalr";
import getFlag from "../../lib/getFlag";
const useSignalCoreForRealTimeChat = getFlag('useSignalCoreForRealTimeChat', false);

const ConversationContainer = props => {
  const store = chatStore.useContainer();
  if (store.selectedConversation === null || store.selectedConversation.length === 0) return null;
  return <>
    {
      store.selectedConversation.map((v, i) => {
        return <ConversationEntry index={i} key={v.conversationId || v.user.userId} {...v} />
      })
    }
  </>
}

const ChatContainer = props => {
  const store = chatStore.useContainer();
  const auth = authentication.useContainer();
  const conversationUpdate = useRef(null);
  const typingEndRefs = useRef({});

  const setTyping = (conversationId, userId, isTyping) => {
    store.dispatchConversations({
      action: 'SET_TYPING_STATUS',
      conversationId: conversationId,
      userId: userId,
      isTyping: isTyping,
    });
  }
  const getTypingKey = (msg) => `${msg.conversationId}_${msg.userId}`;
  const removeTypingEndRefIfExists = (k) => {
    if (typingEndRefs.current[k]) {
      clearTimeout(typingEndRefs.current[k]);
      typingEndRefs.current[k] = null;
    }
  }

  const onChatTyping = msg => {
    console.log('[info] typing for',msg);
    setTyping(msg.conversationId, msg.userId, true);
    const k = getTypingKey(msg);
    removeTypingEndRefIfExists(k);
    typingEndRefs.current[k] = setTimeout(() => {
      typingEndRefs.current[k] = null;
      setTyping(msg.conversationId, msg.userId, false);
    }, msg.endsAt - Date.now());
  }

  const onChatConversationAdded = msg => {
    store.dispatchConversations({
      action: 'MULTI_ADD',
      data: [data.conversation],
    });
    multiGetLatestMessages({
      conversationIds: [msg.id],
    }).then(data => {
      store.dispatchConversations({
        action: 'MULTI_ADD_LATEST_MESSAGES',
        data: data,
      });
    })
  }

  const onChatMessageReceived = msg => {
    console.log('[info] signalr ChatMessageReceived', msg);
    const k = getTypingKey(msg);
    removeTypingEndRefIfExists(k);
    setTyping(msg.conversationId, msg.userId, false);
    store.dispatchConversations({
      action: 'MULTI_ADD_LATEST_MESSAGES',
      data: [
        {
          conversationId: msg.conversationId,
          chatMessages: [
            {
              id: msg.id,
              content: msg.message,
              sent: msg.sent,
              senderTargetId: msg.userId,
              read : false, // todo: ?
            }
          ],
        },
      ],
    });
  }

  useEffect(() => {
    if (useSignalCoreForRealTimeChat) {
      if (conversationUpdate.current) {
        console.log('[info] signalr closing existing connection');
        conversationUpdate.current.connection.stop();
        if (conversationUpdate.current.pingTimer) {
          clearInterval(conversationUpdate.current.pingTimer);
        }
      }
      console.log('[info] signalr creating connection');
      const connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();
      connection.on('ChatTyping', data => onChatTyping(JSON.parse(data)));
      connection.on('ChatConversationAdded', data => onChatConversationAdded(JSON.parse(data)));
      connection.on("ChatMessageReceived", data => onChatMessageReceived(JSON.parse(data)));

      let pingTimer;
      connection.start().then(() => {
          console.log('[info] signalr connection ready');
          pingTimer = setInterval(() => {
            connection.invoke("ListenForMessages").then((r) => {
              console.log('[info] signalr ListenForMessages response', r);
            }).catch(e => {
              console.error('[err] signalr ListenForMessages error', e);
            })
          }, 5000);
        })
        .catch(e => {
          console.error('[error] signalr connection error:', e);
        });
      conversationUpdate.current = {
        connection,
        pingTimer,
      };
    }

    const updateConversations = () => {
      // hard coded since this is mostly a POC right now. we should probably add paging eventually
      getUserConversations({
        pageNumber: 1,
        pageSize: 100,
      }).then(conv => {
        store.dispatchConversations({
          action: 'MULTI_ADD',
          data: conv,
        });

        if (conv.length === 0) return;

        multiGetLatestMessages({
          conversationIds: conv.map(v => v.id),
        }).then(data => {
          store.dispatchConversations({
            action: 'MULTI_ADD_LATEST_MESSAGES',
            data: data,
          });
        })
      })
    }

    updateConversations();
    if (!useSignalCoreForRealTimeChat) {
      conversationUpdate.current = setInterval(() => {
        updateConversations();
      }, 5 * 1000);
    }

    return () => {
      if (!useSignalCoreForRealTimeChat) {
        clearInterval(conversationUpdate.current);
      }else{
        if (conversationUpdate.current)
          conversationUpdate.current.connection.stop();
      }
      conversationUpdate.current = null;
    }
  }, []);

  useEffect(() => {
    if (auth.userId) {
      getFriends({userId: auth.userId}).then(d => {
        store.setFriends(d);
      })
    }
  }, [auth.userId]);

  return <div className={styles.container}>
    <ChatMenu />
    <ConversationContainer />
  </div>
}

export default ChatContainer;