import { createUseStyles } from "react-jss";
import Link from "../../link";

const useStyles = createUseStyles({
  wrapper: {
    width: '100%',
    color: '#000',
    display: 'block',
    padding: '5px 10px',
    '&:hover': {
      background: '#efefef',
      color: '#000',
    },
  },
  wrapperSelected: {
    borderLeft: '1px solid #ccc',
    borderTop: '1px solid #ccc',
    borderBottom: '1px solid #ccc',

    backgroundColor: '#efefef',
  },
  wrapperDisabled: {
    opacity: 0.25,
  },
  text: {
    fontSize: '16px',
  },
  textSelected: {
    fontWeight: '600',
  },
});

const SelectorOption = props => {
  const s = useStyles();
  const el = <a onClick={props.onClick} className={s.wrapper + (props.selected ? ' ' + s.wrapperSelected : '') + (props.disabled ? ' ' + s.wrapperDisabled : '')}>
    <span className={s.text + (props.selected ? ' ' + s.textSelected : '')}>{props.name}</span>
  </a>

  if (props.url) {
    return <Link href={props.url}>
      {el}
    </Link>
  }

  return el;
}

export default SelectorOption;