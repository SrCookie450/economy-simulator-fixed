import {createContainer} from "unstated-next";
import {useReducer, useState} from "react";

const conversationReducer = (state, action) => {
  if (action.action === 'MULTI_ADD') {
    let newState = state ? [... state] : [];
    action.data.forEach(v => {
      if (!newState.find(x => x.id === v.id)) {
        newState.push(v);
      }
    });
    return newState;
  }
  if (action.action === 'MARK_AS_READ') {
    let newState = [...state];
    const convo = newState.find(a => a.id === action.conversationId);
    convo.hasUnreadMessages = false;
    return newState;
  }
  if (action.action === 'MULTI_ADD_LATEST_MESSAGES') {
    let newState = [...state];
    for (const item of action.data) {
      const convo = newState.find(x => x.id === item.conversationId);
      let msg = item.chatMessages[0] || null;
      if (convo) {
        convo.latest = msg;
        if (msg && msg.read === false) {
          convo.hasUnreadMessages = true;
        }
      }
    }
    return newState;
  }

  if (action.action === 'SET_TYPING_STATUS') {
    let newState = [...state];
    const convo = newState.find(a => a.id === action.conversationId);
    for (const participant of convo.participants) {
      if (participant.targetId === action.userId) {
        participant.isTyping = action.isTyping;
      }
    }
    return newState;
  }
  return state;
}

const ChatStore = createContainer(() => {
  const [conversations, dispatchConversations] = useReducer(conversationReducer, null);
  const [friends, setFriends] = useState(null);
  const [selectedConversation, setSelectedConversation] = useState([]);

  const unreadCount = (() => {
    if (!conversations) return null;
    let total = 0;
    for (const item of conversations){
      if (item.hasUnreadMessages)
        total++;
    }
    return total;
  })();

  return {
    unreadCount,

    conversations,
    dispatchConversations,

    selectedConversation,
    setSelectedConversation,

    friends,
    setFriends,
  }
});

export default ChatStore;