import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  buttonWrapper: {},
  button: {
    border: '1px solid #666',
    background: 'linear-gradient(0deg, rgba(197,197,197,1) 0%, rgba(255,255,255,1) 100%)',
    display: 'block',
    textAlign: 'center',
    color: '#000',
    '&:hover': {
      color: '#000',
      background: 'rgba(197,197,197,1)',
    }
  },
});

const Button = props => {
  const s = useStyles();
  return <div className={s.buttonWrapper}>
    {props.href ? <a {...props} className={s.button + ' ' + (props.className || '')}>{props.children}</a> :
      <button {...props} className={s.button + ' ' + (props.className || '')}>{props.children}</button>
    }
  </div>
}

export default Button;