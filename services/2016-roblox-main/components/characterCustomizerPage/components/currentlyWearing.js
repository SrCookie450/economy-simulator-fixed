import React from "react";
import { createUseStyles } from "react-jss";
import CharacterCustomizationStore from "../../../stores/characterPage";
import WardrobeEntry from "./wardrobeEntry";

const useCurrentlyWearingStyles = createUseStyles({
  subtitle: {
    fontSize: '24px',
    margin: 0,
    fontWeight: 400,
  },
});

const CurrentlyWearing = props => {
  const s = useCurrentlyWearingStyles();
  const characterStore = CharacterCustomizationStore.useContainer();
  if (characterStore.wearingAssets === null) {
    return null;
  }

  return <div className='row'>
    <div className='col-12'>
      <div className='divider-top divider-thick divider-light mt-4 mb-4'></div>
    </div>
    <div className='col-12'>
      <h2 className={s.subtitle}>Currently Wearing</h2>
      <div className='row'>
        {characterStore.wearingAssets.length === 0 ? <p>You aren't wearing anything</p> : characterStore.wearingAssets.map((v, i) => {
          return <WardrobeEntry key={v.assetId} assetId={v.assetId} name={v.name} assetTypeId={v.assetType.id} assetTypeName={v.assetType.name} showAssetType={true}></WardrobeEntry>
        })}
      </div>
    </div>
  </div>
}

export default CurrentlyWearing;