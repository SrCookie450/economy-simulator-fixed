import updatePlaceStore from "../stores/updatePlaceStore";
import {useEffect, useState} from "react";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import {updateAsset} from "../../../services/develop";
/*
    public enum Genre
    {
        All = 0,
        Building = 13,
        Horror = 5,
        TownAndCity = 1,
        Military = 11,
        Comedy = 9,
        Medieval = 2,
        Adventure = 7,
        SciFi = 3,
        Naval = 6,
        FPS = 14,
        RPG = 15,
        Sports = 8,
        Fighting = 4,
        Western = 10,
        Skatepark = 18,
    }
 */
const genres = [
  {
    name: 'All',
    value: 'All',
  },
  {
    name: 'Town and City',
    value: 'TownAndCity',
  },
  {
    name: 'Building',
    value: 'Building',
  },
  {
    name: 'Horror',
    value: 'Horror',
  },
  {
    name: 'Military',
    value: 'Military',
  },
  {
    name: 'Comedy',
    value: 'Comedy',
  },
  {
    name: 'Medieval',
    value: 'Medieval',
  },
  {
    name: 'Adventure',
    value: 'Adventure',
  },
  {
    name: 'Sci-Fi',
    value: 'SciFi',
  },
  {
    name: 'Naval',
    value: 'Naval',
  },
  {
    name: 'FPS',
    value: 'FPS',
  },
  {
    name: 'RPG',
    value: 'RPG',
  },
  {
    name: 'Sports',
    value: 'Sports',
  },
  {
    name: 'Fighting',
    value: 'Fighting',
  },
  {
    name: 'Western',
    value :'Western',
  },
  {
    name: 'Skate park',
    value: 'Skatepark',
  },
]

const BasicSettings = props => {
  const store = updatePlaceStore.useContainer();
  const [name, setName] = useState('');
  const [description, setDescription] = useState(null); // nullable
  const [commentsEnabled, setCommentsEnabled] = useState(false);
  const [genre, setGenre] = useState('All');
  const [feedback, setFeedback] = useState(null);
  const s = useButtonStyles();

  const resetForm = () => {
    if (store.locked) return;
    setName(store.details.name);
    setDescription(store.details.description);
    setGenre(store.details.genre);
  }

  useEffect(() => {
    resetForm();
  }, [store.details]);

  const save = () => {
    if (store.locked) return;
    store.setLocked(true);

    updateAsset({
      assetId: store.placeId,
      name,
      description,
      genres: [genre],
      isCopyingAllowed: false,
      enableComments: commentsEnabled === 'true',
    }).then(() => {
      window.location.reload();
    }).catch(e => {
      // set error
      if (e.response && e.response.data && e.response.data.errors && e.response.data.errors.length) {
        let msg = e.response.data.errors[0];
        setFeedback(msg.message);
      }else{
        setFeedback(e.message);
      }
      store.setLocked(false);
    })
  }

  return <div className='row mt-4'>
    <div className='col-6'>
      <h2 className='fw-bolder mb-4'>Basic Settings</h2>
      {feedback ? <p className='mb-0 text-danger'>{feedback}</p> : null}
      <p className='mb-0 fw-bold'>Name:</p>
      <input type='text' className='w-100' value={name} onChange={(e) => {
        setName(e.currentTarget.value);
      }} />
      <p className='mb-0 fw-bold mt-2'>Description:</p>
      <textarea rows={8} className='w-100' value={description || ''} onChange={(e) => {
        setDescription(e.currentTarget.value);
      }} />
      <p className='mb-0 fw-bold mt-2'>Comments Enabled:</p>
      <select value={commentsEnabled} onChange={e => {
        setCommentsEnabled(e.currentTarget.value);
      }}>
        <option value='true'>Yes</option>
        <option value='false'>No</option>
      </select>
      <p className='mb-0 fw-bold mt-2'>Genre:</p>
      <select value={genre} onChange={(e) => {
        setGenre(e.currentTarget.value);
      }}>
        {
          genres.map(v => {
            return <option key={v.value} value={v.value}>{v.name}</option>
          })
        }
      </select>
      <div className='mt-4'>
        <div className='d-inline-block'>
          <ActionButton disabled={store.locked} className={s.normal + ' ' + s.continueButton} label='Save' onClick={save} />
        </div>
        <div className='d-inline-block ms-4'>

          <ActionButton disabled={store.locked} className={s.normal + ' ' + s.cancelButton} label='Cancel' onClick={() => {
            resetForm();
          }} />
        </div>
      </div>
    </div>
  </div>
}

export default BasicSettings;