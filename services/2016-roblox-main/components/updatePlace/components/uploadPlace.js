import updatePlaceStore from "../stores/updatePlaceStore";
import {useRef, useState} from "react";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import {uploadAssetVersion} from "../../../services/develop";

const UploadPlace = props => {
  const store = updatePlaceStore.useContainer();
  const fileRef = useRef(null);
  const s = useButtonStyles();
  const [feedback, setFeedback] = useState(null);

  const uploadPlace = () => {
    uploadAssetVersion({
      assetId: store.placeId,
      file: fileRef.current.files[0],
    }).then(() => {
      window.location.reload();
    }).catch(e => {
      setFeedback(e.response?.data?.errors?.[0]?.message || e.message);
    })
  }

  return <div className='row mt-4'>
    <div className='col-12'>
      <h2 className='fw-bolder mb-4'>Upload Place</h2>
      {feedback ? <p className='mb-0 text-danger'>{feedback}</p> : null}
    </div>
    <div className='col-6'>
      <p className='mb-0 fw-bold mt-2'>RBXM File:</p>
      <input ref={fileRef} type='file' />
      <div className='mt-4'>
        <div className='d-inline-block'>
          <ActionButton disabled={store.locked} className={s.normal + ' ' + s.continueButton} label='Upload' onClick={uploadPlace} />
        </div>
      </div>
    </div>
  </div>
}

export default UploadPlace;