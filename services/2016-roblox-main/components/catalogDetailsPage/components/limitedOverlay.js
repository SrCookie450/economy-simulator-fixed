import { createUseStyles } from "react-jss"

const useStyles = createUseStyles({
  wrapper: {
    marginTop: '-60px',
    overflowX: 'hidden',
  },
  img: {
    marginLeft: '-26px',
  },
});

const LimitedOverlay = props => {
  const s = useStyles();
  return <div className={s.wrapper}>
    <img className={s.img} src='/img/limitedOverlay_itemPage.png'>

    </img>
  </div>
}

const LimitedUniqueOverlay = props => {
  const s = useStyles();
  return <div className={s.wrapper}>
    <img className={s.img} src='/img/limitedUniqueOverlay_itemPage.png'>

    </img>
  </div>
}

export {
  LimitedOverlay,
  LimitedUniqueOverlay,
};