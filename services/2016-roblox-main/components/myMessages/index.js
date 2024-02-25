import React, {useState} from "react";
import { createUseStyles } from "react-jss";
import MyMessagesStore from "../../stores/myMessages";
import AdBanner from "../ad/adBanner";
import OldVerticalTabs from "../oldVerticalTabs";
import LargeMessage from "./components/largeMessage";
import MessageRow from "./components/messageRow";

const useStyles = createUseStyles({
  messagesContainer: {
    background: '#fff',
    padding: '4px 8px',
    overflow: 'hidden',
    minHeight: '100vh',
  }
})

const MyMessages = props => {
  const s = useStyles();
  const store = MyMessagesStore.useContainer();
  const [tab, setTab] = useState('Inbox');

  return <div className='container'>
    <AdBanner/>
    <div className={s.messagesContainer}>
      <div className='row mt-2'>
        <div className='col-12'>
          {
            store.highlightedMessage ? <LargeMessage {...store.highlightedMessage}/> :

              <OldVerticalTabs onChange={(v) => {
                setTab(v.name);
              }} default={tab} options={[
                {
                  name: 'Inbox',
                  element: <MessageRow tab='inbox'/>,
                },
                {
                  name: 'Sent',
                  element: <MessageRow tab='sent'/>,
                },
                {
                  name: 'Notifications',
                  element: <MessageRow tab='notifications'/>,
                  count: store.notifications ? store.notifications.collection.length : undefined,
                },
                {
                  name: 'Archive',
                  element: <MessageRow tab='archive'/>,
                },
              ]}/>
          }
        </div>
      </div>
    </div>
  </div>
}

export default MyMessages;