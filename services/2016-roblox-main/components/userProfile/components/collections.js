import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getBaseUrl } from "../../../lib/request";
import {getItemUrl} from "../../../services/catalog";
import { getCollections } from "../../../services/inventory";
import useCardStyles from "../styles/card";
import SmallButtonLink from "./smallButtonLink";
import Subtitle from "./subtitle";

const useCollectionStyles = createUseStyles({
  imageWrapper: {
    border: '1px solid #c3c3c3',
    borderRadius: '4px',
  },
  image: {
    width: '100%',
    margin: '0 auto',
    display: 'block',
  },
  itemLabel: {
    fontWeight: 300,
    fontSize: '16px',
    color: '#666',
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
  },
  buttonWrapper: {
    width: '100px',
    float: 'right',
    marginTop: '10px',
  },
  labelWrapper: {
    width: '100%',
    marginTop: '-27px',
    overflow: 'hidden',
  },
  overlayLimited: {
    height: '28px',
    marginLeft: '-14px',
  },
  overlayLimitedUnique: {
    height: '28px',
    marginLeft: '-14px',
  },
});

const Collections = props => {
  const { userId } = props;
  const s = useCollectionStyles();
  const cardStyles = useCardStyles();
  const [collections, setCollections] = useState(null);
  useEffect(() => {
    getCollections({ userId }).then(setCollections)
  }, [userId]);

  if (!collections || collections.length === 0) {
    return null;
  }
  return <div className='row'>
    <div className='col-10'>
      <Subtitle>Collections</Subtitle>
    </div>
    <div className='col-2'>
      <div className={s.buttonWrapper}>
        <SmallButtonLink href={`/users/${userId}/inventory`}>Inventory</SmallButtonLink>
      </div>
    </div>
    <div className='col-12'>
      <div className={cardStyles.card}>
        <div className='row ps-4 pe-4 pt-4 pb-4'>
          {
            collections.map((v, i) => {
              const assetId = v.Id;
              const url = assetId && getItemUrl({assetId: assetId, name: v.Name}) || v.AssetSeoUrl;
              const isLimited = v.AssetRestrictionIcon && v.AssetRestrictionIcon.CssTag === "limited";
              const isLimitedUnique = v.AssetRestrictionIcon && v.AssetRestrictionIcon.CssTag === "limited-unique";
              const hasOverlay = isLimited || isLimitedUnique;

              return <div className='col-4 col-lg-2' key={i}>
                <a href={url}>
                  <div className={s.imageWrapper}>
                    <img src={v.Thumbnail.Url.startsWith('http') ? v.Thumbnail.Url : getBaseUrl() + v.Thumbnail.Url} className={s.image}/>
                    {hasOverlay ? <div className={s.labelWrapper}>
                      {
                        isLimited ? <img className={s.overlayLimited} src='/img/limitedOverlay_itemPage.png' />
                          : isLimitedUnique ? <img className={s.overlayLimitedUnique} src='/img/limitedUniqueOverlay_itemPage.png' /> : null
                      }
                    </div> : null}
                  </div>
                  <p className={`mb-0 ${s.itemLabel}`}>{v.Name}</p>
                </a>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}

export default Collections;