import styles from './permissions.module.css';

const Permissions = props => {
  const {tab, options, openTab, setOpenTab} = props;
  const isOpen = tab === openTab;

  return <div className='me-4 ms-4 mb-1'>
    <div className={styles.accordian}>
      <p className='fw-bold mb-0 cursor-pointer' onClick={() => {
        setOpenTab(openTab === tab ? null : tab);
      }}>{props.tab}</p>
    </div>
    {isOpen ? <div className='ms-4'>
      {
        options.map(v => {
          return <div>
            <input type='checkbox' checked={v.get()} onChange={e => {
              v.set(e.currentTarget.checked);
            }} />
            <span>{v.name}</span>
          </div>
        })
      }
    </div> : null}
  </div>
}

export default Permissions;