import CreatorLink from "../../creatorLink";
import PlayerImage from "../../playerImage";

const BuilderDetails = props => {
  const { creatorId, creatorType, creatorName } = props;

  return <div className='row'>
    <div className='col-3 ps-0 pe-0'>
      {
        creatorType === 'User' ? <PlayerImage name={creatorName} id={creatorId}></PlayerImage> : null
      }
    </div>
    <div className='col-9'>
      <p className='mb-0 fw-600 lighten-2 font-size-15'>Builder:</p>
      <p className='mb-0 fw-500 font-size-18'><CreatorLink id={creatorId} name={creatorName} type={creatorType}></CreatorLink></p>
      {/* TODO: Creator join date (if user), or group creation date (if group) */}
    </div>
  </div>
}

export default BuilderDetails;