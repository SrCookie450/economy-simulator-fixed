import { createUseStyles } from "react-jss";
import { getBaseUrl } from "../../lib/request";
import ThumbnailStore from "../../stores/thumbnailStore";
import {reportImageFail} from "../../services/metrics";
import {useEffect, useState} from "react";

const useStyles = createUseStyles({
  image: {
    maxWidth: '400px',
    width: '100%',
    margin: '0 auto',
    height: 'auto',
    paddingTop: '20px',
  },
})

const ItemImage = (props) => {
  const s = useStyles();
  const store = ThumbnailStore.useContainer();
  const [retryCount, setRetryCount] = useState(0);

  const [image, setImage] = useState(store.getPlaceholder());
  useEffect(() => {
    setImage(store.getAssetThumbnail(props.id, '420x420'));
  }, [props, store.thumbnails]);

  return <img className={s.image + ' ' + (props.className || '')} src={image} alt={props.name} onError={(e) => {
    if (retryCount >= 3) return;
    reportImageFail({
      errorEvent: e,
      type: 'assetThumbnail',
      src: image,
    })
    setRetryCount(retryCount + 1);
    setImage(store.getPlaceholder())
  }}/>
}

export default ItemImage;