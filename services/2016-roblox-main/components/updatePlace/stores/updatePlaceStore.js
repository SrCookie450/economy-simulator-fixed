import {createContainer} from "unstated-next";
import {useState} from "react";

const UpdatePlaceStore = createContainer(() => {
  const [tab, setTab] = useState(null);
  const [details, setDetails] = useState(null);
  const [locked, setLocked] = useState(false);
  const [placeId, setPlaceId] = useState(null);

  return {
    locked,
    setLocked,

    tab,
    setTab,

    details,
    setDetails,

    placeId,
    setPlaceId,
  }
});

export default UpdatePlaceStore;