import styles from "./chatMenu.module.css";
import chatStore from "../chatStore";
import {memo, useEffect, useReducer, useRef, useState} from "react";
import {getMessages, markAsRead, sendMessage, startOneToOneConversation} from "../../../services/chat";
import dayjs from "dayjs";
import authentication from "../../../stores/authentication";
const useSignalCoreForRealTimeChat = getFlag('useSignalCoreForRealTimeChat', false);
const ChatInputMemo = memo(props => <ChatInput {...props} />);
/**
 *
 * @param {{id: string, sent: string, read: boolean, messageType: string, senderTargetId: number, content: string}[]} chat
 * @returns {*}
 */
const GenerateLayoutFromMessageHistory = (chat) => {
  /**
   * @type {{type: 'message' | 'date'}[]}
   */
  const final = [];

  chat.forEach((v, i, arr) => {
    let previous = i > 0 ? arr[i-1] : undefined;
    let next = i < arr.length ? arr[i+1] : undefined;
    const previousInFinal = final.length > 0 ? final[final.length - 1] : undefined;

    if (previousInFinal && previousInFinal.type === 'message') {
      if (previousInFinal.data.senderTargetId === v.senderTargetId) {
        const currentTime = dayjs(v.sent);
        const lastSendTime = dayjs(previousInFinal.data.messages[previousInFinal.data.messages.length - 1]);
        // if (currentTime.day() !== lastSendTime.day() && currentTime.year() === lastSendTime.year())
        previousInFinal.data.messages.unshift(v);
        return
      }
    }

    // If there is over "1 day" of difference between each message, add a date separator
    if (previousInFinal && previousInFinal.type === 'message') {
      const lastMessageSentAt = previousInFinal.data.messages[0].sent;
      const currentTime = dayjs(v.sent);
      const lastSendTime = dayjs(lastMessageSentAt);
      const now = dayjs();

      if (currentTime.day() !== lastSendTime.day()) {
        let doAddFull = true;
        if (currentTime.month() === lastSendTime.month() && currentTime.year() === lastSendTime.year()) {
          const difference = lastSendTime.day() - currentTime.day();
          const differenceToNow = currentTime.day() - now.day();
          if (differenceToNow >= 7) {
            doAddFull = false;
            // e.g. "Nov 12, 2015"
            final.push({
              type: 'date',
              data: {
                label: currentTime.format('MMM DD, YYYY'),
              },
            });
          }
        }
        if (doAddFull) {
          final.push({
            type: 'date',
            data: {
              label: currentTime.format('ddd | h:mm A'),
            },
          });
        }
      }
    }

    final.push({
      type: 'message',
      data: {
        senderTargetId: v.senderTargetId,
        messages: [v],
      },
    });
  });

  if (chat.length < 100) {
    final.push({
      type: 'date',
      data: {
        label: dayjs(chat[0].sent).format('MMM DD, YYYY'),
      },
    });
  }

  return final.reverse();
}

import historyStyles from './conversationEntry.module.css';
import entryStyles from './chatEntry.module.css';
import PlayerHeadshot from "../../playerHeadshot";
import getFlag from "../../../lib/getFlag";
import ChatInput from "./chatInput";
const ChatHistory = props => {
  const auth = authentication.useContainer();
  if (props.messageLayout) {
    return <>
      {
        props.messageLayout.map(v => {
          if (v.type === 'message') {
            const isMe = auth.userId === v.data.senderTargetId;
            return <div key={v.data.messages[0].id} className={historyStyles.box + ' ' + (isMe ? historyStyles.boxSelf : historyStyles.boxOther)}>
              {
                isMe ? <>
                  <div className={historyStyles.messageBoxSelf}>
                    {
                      v.data.messages.map(v => {
                        return <p key={v.id} className={historyStyles.message}>{v.content}</p>
                      })
                    }
                  </div>
                </> : <>
                  <div className={historyStyles.messageOtherHeadshot}>
                    <div className={entryStyles.chatHeadshot}>
                      <div className={entryStyles.chatHeadshotImage}>
                        <PlayerHeadshot id={v.data.senderTargetId} name={'Roblox User'} />
                      </div>
                    </div>
                  </div>

                  <div className={historyStyles.messageOther}>
                    <div className={historyStyles.messageBoxOther}>
                      {
                        v.data.messages.map(v => {
                          return <p key={v.id} className={historyStyles.message}>{v.content}</p>
                        })
                      }
                    </div>
                  </div>
                </>
              }
            </div>
          }else if (v.type === 'date') {
            return <div key={'date ' + v.data.label} className={historyStyles.date}>
              <p className={historyStyles.dateLabel}>
                <span className={historyStyles.dateSpan}>
                  {v.data.label}
                </span>
              </p>
            </div>
          }
        })
      }
    </>
  }

  return null;
}

const Conversation = props => {
  const store = chatStore.useContainer();
  const auth = authentication.useContainer();
  const myIndex = props.index;
  const {user, conversationId} = props;
  const [locked, setLocked] = useState(false);

  const [messages, setMessages] = useReducer((prev,action) => {
    if (action.action === 'MULTI_ADD') {
      let previous = prev ? [...prev] : [];
      let toAdd = action.data.filter(v => {
        return !previous.find(x => x.id === v.id);
      });
      if (toAdd.length === 0) return prev;
      for (const item of previous) {
        toAdd.push(item);
      }
      return toAdd;
    }
    if (action.action === 'ADD_ONE') {
      if (!prev)
        return [action.data];
      return [action.data, ...prev];
    }
    if (action.action === 'RESET') return null;
    return prev;
  }, null);
  useEffect(() => {
    const myConvo = store.conversations.filter(v => v.id === conversationId)[0];
    if (!myConvo || !conversationId)
      return;

    if (myConvo.hasUnreadMessages) {
      store.dispatchConversations({
        action: 'MARK_AS_READ',
        conversationId,
      });
    }
    if (messages && myConvo.latest && useSignalCoreForRealTimeChat) {
      const messageExists = messages.find(a => a.id === myConvo.latest.id);
      if (!messageExists) {
        setMessages({
          action: 'MULTI_ADD',
          data: [myConvo.latest],
        });
          markAsRead({
            conversationId: conversationId,
            endMessageId: myConvo.latest.id,
          }).then().catch(e => {
            // uh-oh
          })

      }
    }
  }, [store.conversations, messages]);
  const timer = useRef(null);

  useEffect(() => {
    if (timer.current) {
      clearInterval(timer.current);
      timer.current = null;
    }
    setMessages({action: 'RESET'});

    if (conversationId === null) {
      return;
    }
    const loadMessages = () => {
      // TODO: paging
      getMessages({
        conversationId,
        pageSize: 100,
        startId: '',
      }).then(data => {
        setMessages({
          action: 'MULTI_ADD',
          data: data,
        });
        if (data.length) {
          markAsRead({
            conversationId: conversationId,
            endMessageId: data[data.length-1].id,
          }).then().catch(e => {
            // uh-oh
          })
        }
      });
    }
    if (!useSignalCoreForRealTimeChat) {
      timer.current = setInterval(() => {
        loadMessages();
      }, 2 * 1000);
    }
    loadMessages();
    return () => {
      if (!useSignalCoreForRealTimeChat) {
        clearInterval(timer.current);
      }
      timer.current = null;
    }
  }, [user, conversationId]);

  const addCreatedMessage = (msg, conversationIdOverride) => {
    const newMsg = {
      id: msg.messageId,
      sent: msg.sent,
      content: msg.content,
      messageType: 'PlainText',
      senderTargetId: auth.userId,
    };
    store.dispatchConversations({
      action: 'MULTI_ADD_LATEST_MESSAGES',
      data: [
        {
          conversationId: conversationIdOverride || conversationId,
          chatMessages: [
            {
              ...newMsg,
              read: true,
            }
          ],
        }
      ],
    });
    setMessages({
      action: 'ADD_ONE',
      data: newMsg,
    });
  }
  const chatHistoryRef = useRef(null);
  useEffect(() => {
    chatHistoryRef.current.scrollTop =  chatHistoryRef.current.scrollHeight;
    chatHistoryRef.current.focus();
  });

  return <div className={styles.chatMenu} style={{right: 30 + ((myIndex+1) * 260)}}>
    <div className={styles.chatMenuHeader}>
      <div className='d-inline-block'>
        <p className={styles.chatLabel}>{user.username}</p>
      </div>
      <div className={styles.chatClose} onClick={e => {
        e.preventDefault();
        store.setSelectedConversation(store.selectedConversation.filter(v => {
          if (conversationId)
            return v.conversationId !== conversationId;
          return v.user.id !== user.id;
        }))
      }}>
        X
      </div>
    </div>
    <div className={styles.chatMenuBody}>
      <div className={styles.chatMessageHistory} ref={chatHistoryRef}>
        {messages ? <ChatHistory messageLayout={GenerateLayoutFromMessageHistory(messages)} /> : null}
      </div>
      <ChatInputMemo conversationId={conversationId} locked={locked} setLocked={setLocked} addCreatedMessage={addCreatedMessage} user={user} />
    </div>
  </div>
}

export default Conversation;