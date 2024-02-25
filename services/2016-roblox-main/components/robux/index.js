import { createUseStyles } from "react-jss";

const useRobuxStyles = createUseStyles({
  text: {
    color: '#060',
    fontWeight: 'bold',
    fontSize: '12px',
  },
  image: {
    background: `url("/img/img-robux.png")`,
    width: '18px',
    height: '12px',
    float: 'left',
    marginLeft: '0',
    marginTop: '4px',
    marginRight: '2px',
  },
  prefix: {
    float: 'left',
    marginRight: '4px',
  },
});

const Robux = props => {
  const s = useRobuxStyles();
  return <>
    {props.prefix ? <span className={s.text + ' ' + s.prefix}>{props.prefix}</span> : null}
    <span className={s.image}></span>
    <span className={s.text}>{props.children}</span>
  </>
}

export default Robux;