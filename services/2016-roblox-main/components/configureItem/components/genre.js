import configureItemStore from "../stores/configureItemStore";
import assetTypes from "../../characterCustomizerPage/assetTypes";
import {useEffect, useState} from "react";
import {getAllGenres} from "../../../services/develop";

const Genre = props => {
  const store = configureItemStore.useContainer();
  const [genres, setGenres ] = useState([]);
  useEffect(() => {
    getAllGenres().then(data => setGenres(data));
  }, []);
  // TODO: official the genre list should use radio instead of checkbox, but I couldn't get that working
  // see: https://www.youtube.com/watch?v=4Ak48RID580 (around 1:52)

  return <div className='row mt-4'>
    <div className='col-12'>
      <h3>Genre</h3>
      <hr className='mt-0 mb-2' />
      <p className='ps-2 pe-2'>Classify your {assetTypes[store.details.assetType]} to help people find it.</p>
    </div>
    <div className='col-12 col-lg-8 offset-lg-1'>
      <div className='row'>
        {
          genres.map(v => {
            const isChecked = store.genres.find(x => x === v) !== undefined;
            return <div className='col-3'>
              <div className='d-inline'><div className={'GamesInfoIcon ' + v}/></div>
              <input checked={isChecked} disabled={store.locked} type='checkbox' onChange={e => {
                if (isChecked) {
                  // There must always be at least one genre at all times
                  const newGenres = store.genres.filter(x => x !== v);
                  if (newGenres.length === 0) {
                    newGenres.push('All');
                  }
                  store.setGenres(newGenres);
                }else{
                  store.setGenres([...store.genres, v]);
                }
              }} />
              <p className='d-inline ms-2'>{v}</p>
            </div>
          })
        }

      </div>
    </div>
  </div>
}

export default Genre;