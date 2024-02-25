import {useEffect, useState} from "react";
import { createUseStyles } from "react-jss";
import { getBaseUrl } from "../../../lib/request";
import GameDetailsStore from "../stores/gameDetailsStore";
import {getGameMedia} from "../../../services/games";
import {multiGetAssetThumbnails, multiGetUserThumbnails} from "../../../services/thumbnails";

const useStyles = createUseStyles({
  image: {
    width: '100%',
    height: 'auto',
    maxWidth: '800px',
    margin: '0 auto',
    display: 'block',
  },
  pageButtons: {
    marginTop: '-165px',
    marginLeft: '10px',
    paddingRight: '30px',
    color: 'rgba(255,255,255,0.5)',
  },
  pageButton: {
    textShadow: '0px 0px 8px rgba(0,0,0,0.5)',
    '&:hover': {
      color: 'rgba(255,255,255,1)',
    }
  },
})

const GameThumbnails = props => {
  const s = useStyles();
  const store = GameDetailsStore.useContainer();
  const [imageUrl, setImageUrl] = useState(null);
  const [images, setImages] = useState(null);

  useEffect(() => {
    if (!store.universeDetails || !store.universeDetails.id)
      return;

    getGameMedia({universeId: store.universeDetails.id}).then(media => {
      const images = media.filter(v => v.assetType === 'Image');
      if (images.length) {
        multiGetAssetThumbnails({assetIds: images.map(v => v.imageId)}).then(thumb => {
          // Default to first thumbnail
          setImageUrl(thumb[0].imageUrl);
          setImages(thumb.map(v => v.imageUrl));
        })
      }else{
        multiGetAssetThumbnails({assetIds: [store.universeDetails.rootPlaceId]}).then(thumb => {
          if (thumb.length) {
            setImageUrl(thumb[0].imageUrl);
            setImages([thumb[0].imageUrl]);
          }
        })
      }
    })
  }, [store.universeDetails, store.details]);

  const loadNextImage = () => {
    let newImage = '';
    for (let i = 0; i < images.length; i++) {
      if (imageUrl === images[i]) {
        newImage = images[i+1];
      }
    }
    if (!newImage) {
      newImage = images[0];
    }
    setImageUrl(newImage);
  }

  const loadPrevImage = () => {
    let newImage = '';
    for (let i = 0; i < images.length; i++) {
      if (imageUrl === images[i]) {
        newImage = images[i-1];
      }
    }
    if (!newImage) {
      newImage = images[images.length-1];
    }
    setImageUrl(newImage);
  }

  useEffect(() => {
    if (!images || !images.length) return;
    let timer = setTimeout(() => {
      loadNextImage();
    }, 10 * 1000);
    return () => {
      clearTimeout(timer);
    }
  }, [imageUrl, images]);

  return <div className='row'>
    <div className='col-12'>
      {imageUrl ? <img className={s.image} src={imageUrl}/> : null}
    </div>
    <div className='col-12'>
      <p className={'fw-bolder font-size-30 user-select-none mb-0 ' + s.pageButtons}>
        {images && images.length > 1 ? <>
          <span className={'me-4 cursor-pointer float-left ' + s.pageButton} onClick={() => {
            loadPrevImage();
          }}>{'<'}</span>
          <span className={'ms-4 cursor-pointer float-right ' + s.pageButton} onClick={() => {
            loadNextImage();
          }}>{'>'}</span>
        </> : null}
      </p>
    </div>
  </div>
}

export default GameThumbnails;