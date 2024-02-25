import styles from './mode.module.css';

const Button = props => {
  return <button className={styles.modeButton + ' ' + (props.mode === props.me ? styles.modeButtonSelected : '')} onClick={() => {props.setMode(props.me)}}>{props.me}</button>

}

const Mode = props => {
  return <div className='text-center'>
    <Button {...props} me='Summary'/>
    <Button {...props} me='Line Item'/>
  </div>
}

export default Mode;