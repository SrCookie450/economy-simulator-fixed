import React, { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { getAds } from "../../../../services/ads";
import AuthenticationStore from "../../../../stores/authentication";
import AssetList from "../assetList";

const Clothing = props => {
  const [ads, setAds] = useState(null);
  const auth = AuthenticationStore.useContainer();
  useEffect(() => {
    if (!auth.userId && !props.groupId) return;
    setAds(null);
    getAds({
      creatorId: props.groupId || auth.userId,
      creatorType: props.groupId ? 'Group' : 'User',
    }).then(d => {
      setAds(d);
    })
  }, [auth.userId, props.groupId]);

  if (!ads) return null;
  return <div className='row'>
    <div className='col-12'>
      <h2>User Ads</h2>
    </div>
    <div className='col-12 mt-4'>
      {
        ads.data.length === 0 ? <p>You haven't created any User Ads.</p> : <AssetList assets={ads.data}/>
      }
    </div>
  </div>
}

export default Clothing;