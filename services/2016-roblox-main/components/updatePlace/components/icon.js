import updatePlaceStore from "../stores/updatePlaceStore";
import {useEffect, useState} from "react";
import {multiGetUniverseIcons} from "../../../services/thumbnails";

const Icon = props => {
  // TODO: add ability to update game icon.
  // I'm just not sure which endpoints I have to use (I don't want to use webforms).

  const store = updatePlaceStore.useContainer();
  const [icon, setIcon] = useState(null);
  useEffect(() => {
    multiGetUniverseIcons({universeIds: [store.details.universeId], size: '420x420'}).then(img => {
      if (img.length && img[0].imageUrl) {
        setIcon(img[0].imageUrl);
      }
    })
  }, [store.details]);

  return <div className='row mt-4'>
    <div className='col-12'>
      <h2 className='fw-bolder mb-4'>Game Icon</h2>
    </div>
    <div className='col-6'>
      <img className='w-100 mx-auto d-block' src={icon || '/img/placeholder.png'} alt='Your game icon' />
    </div>
  </div>
}

export default Icon;