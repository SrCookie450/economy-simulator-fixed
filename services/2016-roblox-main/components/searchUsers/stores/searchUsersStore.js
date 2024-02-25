import {createContainer} from "unstated-next";
import {useEffect, useState} from "react";
import {searchUsers} from "../../../services/users";
import {multiGetPresence} from "../../../services/presence";
import getFlag from "../../../lib/getFlag";

const SearchUsersStore = createContainer(() => {
  const [keyword, setKeyword] = useState(null);
  const [data, setData] = useState(null);
  const [locked, setLocked] = useState(true);
  const [presence, setPresence] = useState({});

  useEffect(() => {
    if (!keyword && !getFlag('searchUsersAllowNullKeyword', false))
      return;
    setLocked(true);
    searchUsers({keyword, limit: 12, offset: 0}).then(d => {
      setData(d);
      const ids = d.UserSearchResults.map(v => v.UserId);
      multiGetPresence({userIds: ids}).then(presenceData => {
        let obj = {}
        for (const id of presenceData) {
          obj[id.userId] = id;
        }
        setPresence(obj);
      })
    }).finally(() => {
      setLocked(false);
    })
  }, [keyword]);

  return {
    keyword,
    setKeyword,

    data,
    setData,

    presence,

    locked,
    setLocked,
  };
});

export default SearchUsersStore;