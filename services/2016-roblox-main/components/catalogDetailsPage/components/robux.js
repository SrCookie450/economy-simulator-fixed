import { createUseStyles } from "react-jss";

const useRobuxStyles = createUseStyles({
  image: {
    background: `url("/img/img-robux.png")`,
    width: '18px',
    height: '12px',
    display: 'inline-block',
  },
  prefix: {
    display: 'inline-block',
    paddingRight: '2px',
  },
  amount: {
    display: 'inline-block',
    paddingLeft: '2px',
    fontSize: '12px',
    color: '#060',
    fontWeight: 700,
  },
});

const Robux = props => {
  const s = useRobuxStyles();
  if (props.inline) {
    return <>
      <div className={s.image}></div>
      <span className={s.amount}>{props.children}</span>
    </>
  }
  return <p className='mb-0'>
    <span className={s.prefix}>{props.prefix}</span>
    <div className={s.image}></div>
    <span className={s.amount}>{props.children}</span>
  </p>
}

export default Robux;