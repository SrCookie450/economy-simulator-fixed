import ConfigureItemStore from "../stores/configureItemStore";
import {useEffect} from "react";
import {getItemDetails} from "../../../services/catalog";
import assetTypes from "../../characterCustomizerPage/assetTypes";
import ConfigureHeader from "./configureHeader";
import ConfigureRow from "./configureRow";
import SellItem from "./sellItem";
import Comments from "./comments";
import Genre from "./genre";

const Container = props => {
  const {assetId} = props;
  const store = ConfigureItemStore.useContainer();
  useEffect(( ) => {
    if (!assetId)
      return;

    store.setError(null);
    store.setDetails(null);
    store.setAssetId(assetId);
    getItemDetails([assetId]).then(data => {
      store.setDetails(data.data.data[0]);
    }).catch(e => {
      store.setError(e.message);
    })
  }, [assetId]);

  if (store.error) {
    return <div className='row'>
      <div className='col-12'>
        <p className='text-center mt-4 mb-4 text-danger'>{store.error}</p>
      </div>
    </div>
  }
  if (!store.assetId || !store.details)
    return null;

  return <div className='row'>
    <div className='col-12'>
      <ConfigureHeader />
      <ConfigureRow />
      <SellItem />
      <Comments />
      <Genre />
    </div>
  </div>
}

export default Container;