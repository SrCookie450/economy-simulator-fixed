import {createContainer} from "unstated-next";
import {useState} from "react";

const GroupAdminStore = createContainer(() => {
  const [groupId, setGroupId] = useState(null);
  const [info, setInfo] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [funds, setFunds] = useState(null);

  // UI
  const [tab, setTab] = useState('Members');

  return {
    groupId,
    setGroupId,

    info,
    setInfo,

    feedback,
    setFeedback,

    funds,
    setFunds,

    tab,
    setTab,
  }
});

export default GroupAdminStore;