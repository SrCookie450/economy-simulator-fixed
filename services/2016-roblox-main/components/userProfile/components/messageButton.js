import React from "react";
import AuthenticationStore from "../../../stores/authentication";
import UserProfileStore from "../stores/UserProfileStore";
import Button from "./button";

const MessageButton = props => {
  const store = UserProfileStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  // TODO: check if we can message user, disable if user is not messageable
  return <Button>
    <a className='text-dark' href={'/messages/compose?recipientId=' + store.userId}>Message</a>
  </Button>
}

export default MessageButton;