import React from "react";
import MyMessages from "../../components/myMessages";
import MyMessagesStore from "../../stores/myMessages";

const MyMessagesPage = props => {
  return <MyMessagesStore.Provider>
    <MyMessages></MyMessages>
  </MyMessagesStore.Provider>
}

export default MyMessagesPage;