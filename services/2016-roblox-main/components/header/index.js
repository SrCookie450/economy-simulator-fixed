import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  header: {
    fontSize: '38px',
    marginBottom: 0,
    fontWeight: 400,
  },
});

const Header = props => {
  const s = useStyles();
  return <h1 className={s.header}>{props.children}</h1>
}

export default Header;