import { useState } from "react";
import { createContainer } from "unstated-next";
import {getUserInfo} from "../../../services/users";
import {getFavorites, getInventory} from "../../../services/inventory";
import getFlag from "../../../lib/getFlag";

const UserInventoryStore = createContainer(() => {
  const limit = 24;
  const [userId, setUserId] = useState(null);
  const [userInfo, setUserInfo] = useState(null);
  const [category, setCategory] = useState({name: 'Hats',
   value: 8});
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);
  const [mode, setMode] = useState(null);

  const requestInventory = (mode, userId, category, cursor) => {
    const func = mode === 'Inventory' ? getInventory : getFavorites;
    func({userId, limit, cursor, assetTypeId: category}).then(data => {
      setData(data.Data);
    }).catch(e => {
      setError(e);
    })
  }

  return {
    userId,
    setUserId: (id) => {
      setUserId(id);
      setUserInfo(null);
    },

    userInfo,
    setUserInfo,

    mode,
    setMode,

    error,
    setError,

    data,
    limit,

    category,
    setCategory: (newCategory) => {
      setCategory(newCategory);
      setData(null);
      requestInventory(mode, userId, newCategory.value, '');
    },

    loadNextPage: () => {
      requestInventory(mode, userId, category.value, data.nextPageCursor);
    },
    loadPreviousPage: () => {
      requestInventory(mode, userId, category.value, data.previousPageCursor);
    },
    nextPageAvailable: () => {
      return data && data.nextPageCursor !== null;
    },
    previousPageAvailable: () => {
      return data && data.previousPageCursor !== null;
    },

    requestInventory,
  };
});

export default UserInventoryStore;