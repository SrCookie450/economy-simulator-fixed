import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  text: {
    marginBottom: '4px',
    fontWeight: 200,
  },
})
const Subtitle = props => {
  const s = useStyles();
  return <h3 className={s.text}>{props.children}</h3>
}

export default Subtitle;