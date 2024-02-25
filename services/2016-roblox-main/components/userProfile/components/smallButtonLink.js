import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  button: {
    color: 'white',
    textAlign: 'center',
  },
  buttonWrapper: {
    background: '#00A2FF',
    width: '100%',
    color: 'white',
    textAlign: 'center',
    padding: '5px 10px',
    borderRadius: '4px',
    '&:hover': {
      background: '#32B5FF',
      boxShadow: '0 1px 3px rgb(150 150 150 / 74%)',
    },
  },
})

const SmallButtonLink = (props) => {
  const s = useStyles();
  return <a className={s.button} href={props.href} onClick={props.onClick}><div className={s.buttonWrapper + ' ' + (props.className || '')}>{props.children}</div></a>
}

export default SmallButtonLink;