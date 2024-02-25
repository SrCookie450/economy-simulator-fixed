import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  modalBg: {
    background: 'rgba(0,0,0,0.8)',
    position: 'fixed',
    top: 0,
    width: '100%',
    height: '100%',
    left: 0,
    zIndex: 9999,
  },
  modalWrapper: {
    width: '400px',
    height: '250px',
    backgroundColor: '#e1e1e1',
    margin: '0 auto',
    border: '1px solid #a3a3a3',
    marginTop: 'calc(50vh - 125px)',
  },
  title: {
    textAlign: 'center',
    fontWeight: 700,
    fontSize: '24px',
    marginTop: '10px',
  },
  innerSection: {
    padding: '4px 8px',
    background: 'white',
    width: '100%',
    height: '205px',
    border: '4px solid #e1e1e1',
  },
  footerText: {
    textAlign: 'center',
    marginBottom: '0',
    marginTop: '14px',
    fontSize: '12px',
    fontWeight: 600,
    color: 'grey',
  },
  closeButtonWrapper: {
    float: 'right',
  },
  closeButton: {
    position: 'relative',
    top: '-30px',
    right: '10px',
    height: '20px',
    width: '20px',
    background: '#666',
    borderRadius: '100%',
    textAlign: 'center',
    color: '#FFFFFF',
    paddingTop: '2px',
    paddingLeft: '1px',
    fontWeight: 700,
    fontFamily: 'sans-serif',
    cursor: 'pointer',
  }
});
const OldModal = props => {
  const showClose = props.onClose || false;
  const s = useStyles();
  const outerStyles = {}
  const innerStyles = {}
  if (props.height) {
    outerStyles.height = props.height + 50;
    innerStyles.height = props.height;
    outerStyles.marginTop = `calc(50vh - ${Math.trunc(innerStyles.height / 2)}px)`;
  }
  if (props.width) {
    outerStyles.width = props.width + 2;
    innerStyles.width = props.width;
  }

  return <div className={s.modalBg}>
    <div className={s.modalWrapper} style={outerStyles}>
      <h3 className={s.title}>
        {props.title}
      </h3>
      {showClose && <div className={s.closeButtonWrapper}>
        <div className={s.closeButton} onClick={(e) => {
          e.preventDefault();
          props.onClose();
        }}>X</div>
      </div>}
      <div className={s.innerSection} style={innerStyles}>
        <div className='row'>
          {props.children}
        </div>
      </div>
    </div>
  </div>
}

export default OldModal;