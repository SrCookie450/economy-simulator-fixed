import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss"
import { getAvatar } from "../../../services/avatar";
import {getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import ItemImage from "../../itemImage";
import PlayerImage from "../../playerImage"
import Subtitle from "./subtitle"
import Link from "../../link";

const useAvatarStyles = createUseStyles({
  avatarImageWrapper: {
    maxWidth: '300px',
    margin: '0 auto',
    display: 'block',
  },
  assetContainerCard: {
    background: '#3b7599',
    height: '100%',
    borderRadius: 0,
  },
  avatarImageCard: {
    borderRadius: 0,
  },
  pagination: {
    textAlign: 'center',
    marginBottom: 0,
    color: 'white',
    fontSize: '28px',
    fontFamily: 'serif',
    '&>span': {
      cursor: 'pointer',
    }
  },
  disabledPagination: {

  },
});

const Avatar = props => {
  const s = useAvatarStyles();
  const { userId } = props;
  const assetsLimit = 8;
  const [assets, setAssets] = useState(null);
  const [selectedAssets, setSelectedAssets] = useState(null);
  const [assetPages, setAssetPages] = useState(1);
  const [assetPage, setAssetPage] = useState(1);
  useEffect(() => {
    getAvatar({ userId }).then(d => {
      setAssets(d.assets);
      setSelectedAssets(d.assets.slice(0, assetsLimit));
      setAssetPage(1);
      setAssetPages(Math.ceil(d.assets.length / assetsLimit));
    })
  }, [userId]);

  return <div className='row'>
    <div className='col-12'>
      <Subtitle>Currently Wearing</Subtitle>
    </div>
    <div className='col-12 col-lg-6 pe-0'>
      <div className={'card ' + s.avatarImageCard}>
        <div className={s.avatarImageWrapper}>
          <PlayerImage id={userId}/>
        </div>
      </div>
    </div>
    <div className='col-12 col-lg-6 ps-0'>
      <div className={'card ' + s.assetContainerCard}>
        <div className='row ps-4 pe-4 pt-4 pb-4'>
          {selectedAssets && selectedAssets.map(v => {
            return <div className='col-3 pt-2 ps-1 pe-1' key={v.id}>
              <div className='card' title={v.name}>
                <Link href={getItemUrl({name: v.name, assetId: v.id})}>
                  <a title={v.name}>
                    <ItemImage id={v.id} className='pt-0'/>
                  </a>
                </Link>
              </div>
            </div>
          })}
        </div>
        <div className='row'>
          <div className='col-12'>
            {
              assetPages > 1 && <p className={s.pagination}>
                {
                  [...new Array(assetPages)].map((_, v) => {
                    const disabled = (v + 1) === assetPage;
                    if (disabled) {
                      return <span className={s.disabledPagination}>●</span>
                    }
                    return <span onClick={() => {
                      setAssetPage(v + 1);
                      let offset = (v + 1) * assetsLimit - assetsLimit;
                      setSelectedAssets(assets.slice(offset, offset + assetsLimit));
                    }}>○</span>
                  })
                }
              </p>
            }
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default Avatar;