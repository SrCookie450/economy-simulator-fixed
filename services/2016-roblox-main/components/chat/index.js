import {useEffect, useState} from "react";
import ChatContainer from "./container";
import {getChatSettings} from "../../services/chat";
import ChatStore from "./chatStore";

const Chat = props => {
  const [enabled, setEnabled] = useState(null);
  useEffect(() => {
    getChatSettings().then(d => {
      if (d.chatEnabled) {
        setEnabled(true);
      }
    }).catch(e => {
      console.error('[error] error fetching chat settings:',e);
    })
  }, []);

  if (!enabled)
    return null;

  return <ChatStore.Provider>
    <ChatContainer />
  </ChatStore.Provider>
}

export default Chat;