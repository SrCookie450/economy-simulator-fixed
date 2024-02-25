import configureItemStore from "../stores/configureItemStore";

const Comments = props => {
  const store = configureItemStore.useContainer();
  return <div className='row mt-4'>
    <div className='col-12'>
      <h3>Turn comments on/off</h3>
      <hr className='mt-0 mb-2' />
      <p className='ps-2 pe-2'>Choose whether or not this item is open for comments.</p>
    </div>
    <div className='col-12 col-lg-8 offset-lg-1'>
      <div>
        <input disabled={store.locked} type='checkbox' checked={store.commentsEnabled} onChange={e => {
          store.setCommentsEnabled(e.currentTarget.checked);
        }} />
        <p className='d-inline ms-2'>Allow Comments</p>
      </div>
    </div>
  </div>
}

export default Comments;