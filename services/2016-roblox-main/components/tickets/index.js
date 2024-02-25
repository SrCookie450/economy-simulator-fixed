import { createUseStyles } from "react-jss";

const useTicketStyles = createUseStyles({
  text: {
    color: '#A61',
    fontWeight: 'bold',
    fontSize: '12px',
  },
  image: {
    background: `url("/img/img-tickets.png")`,
    width: '16px',
    height: '16px',
    marginLeft: '0',
    marginTop: '4px',
    marginRight: '2px',
    display: 'inline-block',
  },
  prefix: {
    float: 'left',
    marginRight: '4px',
  },
});

const Robux = props => {
  const s = useTicketStyles();
  return <>
    {props.prefix ? <span className={s.text + ' ' + s.prefix}>{props.prefix}</span> : null}
    <span className={s.image}></span>
    <span className={s.text}>{props.children}</span>
  </>
}

export default Robux;