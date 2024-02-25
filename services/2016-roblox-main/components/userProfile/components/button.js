import { createUseStyles } from "react-jss";

const useButtonStyles = createUseStyles({
  btn: {
    textAlign: 'center',
    border: '1px solid #B8B8B8',
    borderRadius: '2px',
    background: 'white',
    fontSize: '18px',
    width: '100%',
    paddingTop: '6px',
    paddingBottom: '6px',
    '&:hover': {
      boxShadow: '0 1px 3px rgb(150 150 150 / 74%)',
    },
  }
});

const Button = props => {
  const s = useButtonStyles()
  return <button className={s.btn} onClick={props.onClick} disabled={props.disabled} style={props.style}>{props.children}</button>
}

export default Button;