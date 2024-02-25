import React, { useState } from "react";
import { createUseStyles } from "react-jss"
import {getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import GearDropdown from "../../gearDropdown";
import AssetListAdEntry from "./assetListAdEntry";
import AssetListCatalogEntry from "./assetListCatalogEntry";
import AssetListGameEntry from "./assetListGameEntry";
import thumbnailStore from "../../../stores/thumbnailStore";
import Link from "../../link";
import getFlag from "../../../lib/getFlag";
import {getGameUrl} from "../../../services/games";

const useStyles = createUseStyles({
  image: {
    margin: '0 auto',
    display: 'block',
    height: '70px',
    width: '70px',
    objectFit: 'cover',
  },
  row: {
    borderBottom: '1px solid #f2f2f2',
    paddingBottom: '4px',
  },
  gearDropdownWrapper: {
    marginBottom: '-1rem',
  },
});

const AssetEntry = props => {
  const s = useStyles();
  const thumbs = thumbnailStore.useContainer();
  const isPlace = props.assetType === 9;
  const isAd = props.ad !== undefined && props.target !== undefined;

  const assetUrl = isPlace ? getGameUrl({placeId: props.assetId, name: props.name}) : getItemUrl({assetId: props.assetId, name: props.name})
  const url = isPlace ? `/universes/configure?id=${props.universeId}` : assetUrl;

  const imageAssetId = isAd ? props.ad.advertisementAssetId : props.assetId;
  // todo: figure out better way to do this
  const [runMenuOpen, setRunMenuOpen] = useState(false);

  const gearOptions = [
    isPlace && {
      url: '/universes/configure?id=' + props.universeId,
      name: 'Configure Game',
    },
    isPlace && {
      url: '/places/' + props.assetId + '/update',
      name: 'Configure Start Place',
    },
    // localization skipped
    isPlace && {
      name: 'separator',
    },
    isPlace && {
      name: 'Create Badge',
      url: `/develop?selectedPlaceId=${props.assetId}&View=21`,
    },
    isPlace && {
      name: 'Create Pass',
      url: `/develop?selectedplaceId=${props.assetId}&View=34`,
    },
    isPlace && {
      name: 'Developer Stats',
      url: `/creations/games/${props.universeId}/stats`,
    },
    isPlace && {
      name: 'separator',
    },
    !isAd && !isPlace && {
      name: 'Configure',
      url: `/My/Item.aspx?id=${props.assetId}`,
    },
    !isAd && {
      name: 'Advertise',
      url: `/My/CreateUserAd.aspx?targetId=${props.assetId}&targetType=asset`,
    },
    isAd && {
      name: 'Run',
      onClick: e => {
        e.preventDefault();
        console.log('run ad');
        setRunMenuOpen(!runMenuOpen);
      },
    },
    isPlace && {
      name: 'separator',
    },
    isPlace && {
      name: 'Shut Down All Servers',
      url: '#',
      onClick: e => {
        e.preventDefault();
        // TODO
      },
    },
  ];

  return <div className={'row ' + s.row}>
    <div className='col-2'>
      <img className={s.image} src={thumbs.getAssetThumbnail(imageAssetId)}/>
    </div>
    <div className='col-9 ps-0'>
      <p className='mb-0'>
        <Link href={url}>
          <a>
            {props.name}
          </a>
        </Link>
      </p>
      {
        isAd ? <AssetListAdEntry ad={props.ad} target={props.target} runMenuOpen={runMenuOpen} setRunMenuOpen={setRunMenuOpen}/>
          : props.assetType === 9 ?
            <AssetListGameEntry url={assetUrl} startPlaceName={props.name}/>
            : <AssetListCatalogEntry created={props.created}/>
      }
    </div>
    <div className='col-1'>
      <GearDropdown boxDropdownRightAmount={0} options={gearOptions.filter(v => !!v)}/>
    </div>
  </div>
}

const AssetList = props => {
  return <div className='row'>
    <div className='col-12'>
      {
        props.assets.map(v => {
          return <AssetEntry key={v.assetId || v.ad.id} {...v}/>
        })
      }
    </div>
  </div>
}

export default AssetList;