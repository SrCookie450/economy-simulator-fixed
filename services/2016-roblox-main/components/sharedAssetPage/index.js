import React, { useEffect, useState } from "react";
import CatalogDetails from "../catalogDetailsPage";
import CatalogDetailsPage from "../catalogDetailsPage/stores/catalogDetailsPage";
import CatalogDetailsPageModal from "../catalogDetailsPage/stores/catalogDetailsPageModal";
import GameDetails from "../gameDetails";
import GameDetailsStore from "../gameDetails/stores/gameDetailsStore";
import getFlag from "../../lib/getFlag";
import { logger } from "../../lib/logger";
import redirectIfNotEqual from "../../lib/redirectIfNotEqual";
import {getItemDetails, getItemUrl, getProductInfoLegacy, itemNameToEncodedName} from "../../services/catalog";
import { getGameUrl, multiGetPlaceDetails, multiGetUniverseDetails } from "../../services/games";
import {useRouter} from "next/dist/client/router";

const getUrlForAssetType = ({ assetTypeId, assetId, name }) => {
  if (assetTypeId === 9) {
    // Place
    return getGameUrl({
      placeId: assetId,
      name,
    });
  }
  // Anything else
  return getItemUrl({assetId: assetId, name: name});
}

const AssetPage = props => {
  const router = useRouter();
  const assetId = router.query[props.idParamName];
  const name = router.query[props.nameParamName];

  // TODO: all this asset details crap needs to be done in getInitialProps()
  // The only reason it's not in there now is because I don't have a solution to the server-side CSRF issue yet
  /**
   * @type {[AssetDetailsEntry, import('react').Dispatch<AssetDetailsEntry>]}
   */
  const [details, setDetails] = useState(null);
  const [error, setError] = useState(null);

  const redirectIfBadUrl = ({assetTypeId, name}) => {
    const expectedUrl = getUrlForAssetType({
      assetTypeId: assetTypeId,
      assetId: assetId,
      name: name,
    });
    if (typeof window !== 'undefined' && window.location.href !== expectedUrl) {
      router.push(expectedUrl);
      return true;
    }
    return false;
  }
  useEffect(() => {
    if (!assetId) return;

    getItemDetails([assetId]).then(result => {
      const newDetails = result.data.data[0];
      if (newDetails === undefined) {
        throw new Error('NotFound');
      }
      setDetails(newDetails);
      redirectIfBadUrl({assetTypeId: newDetails.assetType, name: newDetails.name})
    }).catch(e => {
      if (e.response && e.response.status === 406) {
        const isBadAssetType = e.response.data.errors.find(v => v.code === 11);
        if (isBadAssetType) {
          // Get from place details endpoint
          multiGetPlaceDetails({placeIds: [assetId]}).then(resp => {
            const place = resp[0];
            return multiGetUniverseDetails({universeIds: [place.universeId]}).then(data => {
              const uni = data[0];
              setDetails({
                name: uni.name,
                description: uni.description,
                creatorTargetId: uni.creator.id,
                creatorType: uni.creator.type,
                creatorName: uni.creator.name,
                assetType: 9,
                id: assetId,
                createdAt: uni.created,
                updatedAt: uni.updated,
                genres: [uni.genre],
                favoriteCount: uni.favoritedCount,
                isForSale: uni.price !== null,
                price: uni.price,
                itemRestrictions: [],
                productId: assetId,
                itemType: 'Asset',
                lowestSellerData: null,
                offsaleDeadline: null,
                currency: 1,
              })
            })
          }).catch(e => {
            console.error('could not get place details',e);
            setError(e);
          });
          return;
        }else{
          setError(e);
        }
      }
      setError(e);
    })
  }, [assetId]);

  if (error) {
    // todo: better error page would be nice
    return <div className='container'>
      <div className='row'>
        <div className='col-12'>
          <div className='card card-body'>
            <p className='fw-bold'>Error Loading Item</p>
            <p>{error.message ? error.message : error.toString()}</p>
          </div>
        </div>
      </div>
    </div>
  }

  if (!details) return null;
  if (!assetId) return null;

  if (details.assetType === 9) {
    // Place
    return <GameDetailsStore.Provider>
      <GameDetails details={details}/>
    </GameDetailsStore.Provider>;
  }
  // Anything else (e.g. hat, shirt, model)
  return <CatalogDetailsPage.Provider>
    <CatalogDetailsPageModal.Provider>
      <CatalogDetails details={details}/>
    </CatalogDetailsPageModal.Provider>
  </CatalogDetailsPage.Provider>;
}
/*
export async function getServerSideProps({ query, res, req }) {
  const assetId = query['id'];
  const name = query['asset'] || query['url'];
  if (!assetId || !name) {
    return {
      notFound: true,
    }
  }
  let info;
  try {
    info = await getProductInfoLegacy(assetId);
    // Redirection seems to break every few nextjs updates but I can't figure out why.
    if (getFlag('assetRedirectsEnabled', true)) {
      const expectedUrl = getUrlForAssetType({
        assetId: info.AssetId,
        name: info.Name,
        assetTypeId: info.AssetTypeId,
      });
      if (req.url !== expectedUrl) {
        logger.info('redirects', 'asset redirect from', req.url, 'to', expectedUrl);
        return {
          redirect: {
            destination: expectedUrl,
          },
          props: {},
        };
      }
    }
  } catch (e) {
    if (e.response && (e.response.status === 404 || e.response.status === 400)) {
      return {
        notFound: true,
      }
    }
    // todo: we need a better error handling mechanism...
    throw e;
  }
  return {
    props: {
      assetId,
      name,
      title: info.Name + ' - ROBLOX'
    },
  };
}
*/


export default AssetPage;