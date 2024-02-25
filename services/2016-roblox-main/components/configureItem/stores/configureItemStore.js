import {createContainer} from "unstated-next";
import {useState} from "react";
import {setAssetPrice, updateAsset} from "../../../services/develop";
import getFlag from "../../../lib/getFlag";

const ConfigureItemStore = createContainer(() => {
  const [assetId, setAssetId] = useState(null);
  const [details,setDetails] = useState(null);
  const [error, setError] = useState(null);
  const [locked, setLocked] = useState(false);

  const [name, setName] = useState(null);
  const [description, setDescription] = useState(null);
  // 0 = free, null = unset
  const [price, setPrice] = useState(null);
  const [priceTickets, setPriceTickets] = useState(null);
  const [isForSale, setIsForSale] = useState(false);
  const [commentsEnabled, setCommentsEnabled] = useState(false);
  const [genres, setGenres] = useState(null);

  return {
    assetId,
    setAssetId,

    details,
    setDetails: (newDetails) => {
      setDetails(newDetails);
      if (!newDetails)
        return;
      setName(newDetails.name);
      setDescription(newDetails.description);
      setIsForSale(newDetails.isForSale);
      setPrice(newDetails.price);
      setCommentsEnabled(newDetails.commentsEnabled);
      setGenres(newDetails.genres);
      if (getFlag('sellItemForTickets', true)) {
        setPriceTickets(newDetails.priceTickets);
      }
    },

    error,
    setError,

    name,
    setName,

    description,
    setDescription,

    price,
    setPrice,

    priceTickets,
    setPriceTickets,

    isForSale,
    setIsForSale,

    commentsEnabled,
    setCommentsEnabled,

    locked,
    setLocked,

    genres,
    setGenres,

    save: () => {
      if (locked)
        return;
      setLocked(true);
      Promise.all([
        setAssetPrice({assetId,
          priceInRobux: Number.isSafeInteger(price) ? price : null,
          priceInTickets: Number.isSafeInteger(parseInt(priceTickets,10)) ? priceTickets : null,
        }),
        updateAsset({
          assetId,
          name,
          description,
          enableComments: commentsEnabled,
          genres,
          // TODO: everything below this comment
          isCopyingAllowed: false,
        }),
      ]).then(() => {
        setLocked(false);
      }).catch(e => {
        setError(e.message);
      })
    },
  }
});

export default ConfigureItemStore;