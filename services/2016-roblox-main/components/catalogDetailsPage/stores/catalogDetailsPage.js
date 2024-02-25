import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import { getResellableCopies, getResellers } from "../../../services/economy";
import { getOwnedCopies } from "../../../services/inventory";

const subCatIdToName = id => {
  return {
    1: 'Image',
    2: 'T-Shirt',
    3: 'Sound',
    4: 'Mesh',
    5: '',
    8: 'Hat',
    9: 'Place',
    10: 'Model',
    11: 'Shirt',
    12: 'Pants',
    13: 'Decal',
    17: 'Head',
    18: 'Face',
    19: 'Gear',
    21: 'Badge',
    24: 'Animation',
    27: 'Torso',
    28: 'Right Arm',
    29: 'Left Arm',
    30: 'Left Leg',
    31: 'Right Leg',
    32: 'Package',
    34: 'Game Pass',
    38: 'Plugin',
    40: 'Mesh Part',
    41: 'Hat', // Hair Accessory
    42: 'Hat', // Face Accessory
    43: 'Hat', // Neck Accessory
    44: 'Hat', // Shoulder Accessory
    45: 'Hat', // Front Accessory
    46: 'Hat', // Back Accessory
    47: 'Hat', // Waist Accessory
    48: 'Climb Animation',
    49: 'Death Animation',
    50: 'Fall Animation',
    51: 'Idle Animation',
    52: 'Jump Animation',
    53: 'Run Animation',
    54: 'Swim Animation',
    55: 'Walk Animation',
    56: 'Pose Animation',
  }[id];
}

const isLimited = details => {
  if (!details.itemRestrictions.includes('Limited') && !details.itemRestrictions.includes('LimitedUnique')) {
    return false;
  }
  return true;
}

const isResellable = details => {
  return isLimited(details) && !details.isForSale;
}


const CatalogDetailsPage = createContainer(() => {
  /**
   * @type {[AssetDetailsEntry, import('react').Dispatch<AssetDetailsEntry>]}
   */
  const [details, setDetails] = useState(null);
  const [resellers, setResellers] = useState(null);
  const [allResellers, setAllResellers] = useState(null);
  const [resellersPage, setResellersPage] = useState(1);
  const [resellersCount, setResellersCount] = useState(0);
  const [saleCount, setSaleCount] = useState(0);
  const [ownedCopies, setOwnedCopies] = useState(null);
  const [resaleModalOpen, setResaleModalOpen] = useState(false);
  const [unlistModalOpen, setUnlistModalOpen] = useState(false);
  const [inCollection, setInCollection] = useState(false);
  const [offsaleDeadline, setOffsaleDeadline] = useState(null);

  const getPurchaseDetails = (specificUaid = undefined) => {
    if (isResellable(details)) {
      // Get lowest seller
      const seller = specificUaid ? resellers.find(v => v.userAssetId === specificUaid) : resellers && resellers[0];
      if (!seller) return null;
      return {
        assetId: details.id,
        sellerName: seller.seller.name,
        sellerId: seller.seller.id,
        price: seller.price,
        priceTickets: null,
        userAssetId: seller.userAssetId,
        productId: details.productId || details.id,
        currency: details.currency || 1,
      }
    } else if (details.isForSale) {
      return {
        assetId: details.id,
        sellerName: details.creatorName,
        sellerId: details.creatorTargetId,
        price: details.price,
        priceTickets: details.priceTickets,
        userAssetId: null,
        productId: details.productId || details.id,
        currency: details.currency || 1,
      }
    }
    return null;
  }

  useEffect(() => {
    if (!allResellers) return;
    let offset = resellersPage * 6 - 6;
    let limit = 6 + offset;
    setResellers(allResellers.slice(offset, limit));
  }, [resellersPage, allResellers]);

  useEffect(() => {
    if (!allResellers) return;
    setResellersCount(allResellers.length);
  }, [allResellers]);

  return {
    details,
    setDetails,

    saleCount,
    setSaleCount,

    ownedCopies,
    setOwnedCopies,

    inCollection,
    setInCollection,

    isResellable: details && isResellable(details) || false,
    getPurchaseDetails,

    subCategoryDisplayName: details && subCatIdToName(details.assetType),

    offsaleDeadline,
    setOffsaleDeadline,

    unlistModalOpen,
    setUnlistModalOpen,

    resaleModalOpen,
    setResaleModalOpen,

    resellers,
    setResellers,

    resellersCount,
    setResellersCount,

    resellersPage,
    setResellersPage,

    allResellers,
    setAllResellers,

    loadResellers: async () => {
      let data = {}
      let cursor = '';
      let allSellers = [];
      do {
        data = await getResellers({
          assetId: details.id,
          cursor,
          limit: 100,
        });
        cursor = data.data.nextPageCursor;
        data.data.data.forEach(v => allSellers.push(v));
      } while (cursor !== null)
      setAllResellers(allSellers);
      setResellers(allSellers.slice(0, 6));
      setResellersCount(allSellers.length);
    },

    loadOwnedCopies: (userId) => {
      if (!details) return;
      // Get resellers (collectible items)
      if (isResellable(details)) {
        // Get copies (resellable)
        getResellableCopies({
          assetId: details.id,
          userId: userId,
        }).then(({ data }) => {
          setOwnedCopies(data);
        }).catch(e => {
          console.error('[error] could not get owned resellable copies', e);
        })
      } else {
        // Get normal copies
        getOwnedCopies({
          assetId: details.id,
          userId: userId,
        }).then(data => {
          setOwnedCopies(data);
        }).catch(e => {
          console.error('[error] could not get owned copies', e);
        })
      }
    },
  }
})

export default CatalogDetailsPage;