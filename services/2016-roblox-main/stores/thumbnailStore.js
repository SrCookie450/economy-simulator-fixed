import {createContainer} from "unstated-next";
import {useReducer, useRef, useState} from "react";
import {multiGetAssetThumbnails, multiGetGroupIcons, multiGetUserThumbnails} from "../services/thumbnails";

const getKey = (id, type, size) => {
  return type + '_' + id + '_' + size;
}

const thumbnailReducer = (prev, action) => {
  let newData = {...prev};
  if (action.event === 'MULTI_ADD') {
    for (const item of action.thumbnails) {
      newData[getKey(item.targetId, action.type, action.size)] = item.imageUrl;
    }
  }

  return newData;
}

const ThumbnailStore = createContainer(() => {
  const [thumbnails, dispatchThumbnails] = useReducer(thumbnailReducer, {});

  const pendingState = useRef({
    pending: false,
    pendingCount: 0,
    pendingTimer: 0,
    pendingItems: [],
  });

  const doWithRetry = (cb) => {
    (async () => {
      while (true) {
        try {
          await cb();
          return;
        }catch(e) {
          if (e.response && e.response.status === 400)
            return;

          await new Promise((res) => setTimeout(res, 1000));
        }
      }
    })();
  }

  const fetchThumbnails = () => {
    const copy = pendingState.current;
    pendingState.current = {
      pending: false,
      pendingCount: 0,
      pendingTimer: 0,
      pendingItems: copy.pendingItems,
    };
    const getAndProcessThumbnails = (type, cb) => {
      // todo: size support?
      const assets = copy[type];
      if (assets && assets.length) {
        for (const t of assets) {
          pendingState.current.pendingItems.push(getKey(t.id, type, '420x420'));
        }
        doWithRetry(async () => {
          const data = await cb(assets);
          dispatchThumbnails({
            event: 'MULTI_ADD',
            type: type,
            size: '420x420',
            thumbnails: data,
          });
          for (const item of data) {
            pendingState.current.pendingItems = pendingState.current.pendingItems.filter(v => v !== getKey(item.targetId, type, '420x420'));
          }
        })
      }
    }
    getAndProcessThumbnails('asset', (items) => {
      return multiGetAssetThumbnails({
        assetIds: items.map(v => v.id),
      });
    });
    getAndProcessThumbnails('userThumbnail', (items) => {
      return multiGetUserThumbnails({
        userIds: items.map(v => v.id),
        size: '420x420',
        format: 'png',
      });
    });
    getAndProcessThumbnails('groupIcon', (items) => {
      return multiGetGroupIcons({
        groupIds: items.map(v => v.id),
      });
    });
  }
  const requestThumbnail = (id, type, size) => {
    if (!pendingState.current[type]) {
      pendingState.current[type] = []
    }
    let exists = pendingState.current[type].find(v => v.id === id);
    if (exists)
      return;

    if (pendingState.current.pendingItems.includes(getKey(id, type, size)))
      return;

    pendingState.current[type].push({
      id: id,
      size: size,
    });
    pendingState.current.pendingCount++;
    if (!pendingState.current.pending) {
      pendingState.current.pending = true;
      pendingState.current.pendingTimer = setTimeout(() => {
        fetchThumbnails();
      }, 10);
    }else if (pendingState.current.pendingCount >= 50) {
      clearTimeout(pendingState.current.pendingTimer);
      fetchThumbnails();
    }
  }

  const getPlaceholder = () => {
    return '/img/placeholder.png';
  };

  const getThumbnailHandler = (type) => {
    return (id, size = '420x420') => {
      if (!['420x420'].includes(size)) {
        throw new Error('Invalid size');
      }

      const t = thumbnails[getKey(id, type, size)];
      // if t is null, the image is pending/blocked/not available, so don't try to get it again.
      if (t === null || (typeof t === 'string' && t.length === 0)) {
        return getPlaceholder();
      }
      if (t === undefined) {
        requestThumbnail(id, type, size);
        return '/img/placeholder.png';
      }
      return t;
    }
  }

  const getThumbnailRemovalHandler = (type) => {
    return (id, size='420x420') => {
      delete thumbnails[getKey(id, type, size)];
    }
  }

  return {
    thumbnails,

    getUserThumbnail: getThumbnailHandler('userThumbnail'),
    getAssetThumbnail: getThumbnailHandler('asset'),
    getGroupIcon: getThumbnailHandler('groupIcon'),
    removeUserThumbnail: getThumbnailRemovalHandler('userThumbnail'),

    getPlaceholder,
  }
});

export default ThumbnailStore;