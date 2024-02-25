import { createUseStyles } from "react-jss";

const useSubtitleStyles = createUseStyles({
  header: {
    fontWeight: 300,
    fontSize: '26px',
    margin: 0,
    marginBottom: '5px',
    marginTop: '10px',
  },
});

const Subtitle = props => {
  const s = useSubtitleStyles();
  return <h3 className={s.header}>{props.children}</h3>
}

export default Subtitle;