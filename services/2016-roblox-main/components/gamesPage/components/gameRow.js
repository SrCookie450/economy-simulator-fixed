import {useEffect, useRef, useState} from "react";
import { createUseStyles } from "react-jss";
import SmallGameCard from "../../smallGameCard";
import UserAdvertisement from "../../userAdvertisement";
import useDimensions from "../../../lib/useDimensions";

export const useStyles = createUseStyles({
  title: {
    fontWeight: 300,
    marginBottom: '10px',
    marginTop: '10px',
    color: 'rgb(33, 37, 41)',
    marginLeft: '10px',
  },
  gameRow: {
    display: 'flex',
    flexWrap: 'nowrap',
    overflowX: 'hidden',
    marginLeft: '-4px',
    '&>div': {
      flex: '0 0 auto',
    }
  },
  gameCard: {
    width: '170px',
    paddingLeft: '5px',
    paddingRight: '5px',
  },
  pagerButton: {
    border: '1px solid #c3c3c3',
    width: '40px',
    height: 'calc(100% - 34px)',
    background: 'rgba(255,255,255,1)',
    position: 'relative',
    cursor: 'pointer',
    color: '#666',
    boxShadow: '0 0 3px 0 #ccc',
    '&:hover': {
      color: 'black',
    },
  },
  goBack: {
    float: 'left',
    marginLeft: '10px',
  },
  goForward: {
    float: 'right',
    marginLeft: '10px',
  },
  pagerCaret: {
    textAlign: 'center',
    marginTop: '240%',
    userSelect: 'none',
    fontSize: '40px',
  },
  caretLeft: {
    display: 'block',
    transform: 'rotate(90deg)',
    marginRight: '10px',
  },
  caretRight: {
    display: 'block',
    transform: 'rotate(-90deg)',
    marginLeft: '10px',
  },
});

/**
 * A game row
 * @param {{title: string; games: any[]; icons: any; ads?: boolean;}} props
 */
const GameRow = props => {
  const s = useStyles();
  const [offset, setOffset] = useState(0);
  const [limit, setLimit] = useState(1);
  const [offsetComp, setOffsetComp] = useState(0);
  const rowRef = useRef(null);
  const gameRowRef = useRef(null);
  const [rowHeight, setRowHeight] = useState(0);

  const [dimensions] = useDimensions();
  useEffect(() => {
    if (!rowRef.current) {
      return
    }
    // width = 170px
    // sub 80 for pagination buttons
    let windowWidth = rowRef.current.clientWidth;
    // breakpoints: 992, 1300 for side nav
    let offsetNotRounded = (windowWidth - 80) / 170;
    let newLimit = Math.floor(offsetNotRounded);

    setLimit(newLimit);
    if (offsetNotRounded !== newLimit) {
      setOffsetComp(1);
    }else{
      setOffsetComp(0);
    }
  }, [dimensions, props.games, props.icons]);

  useEffect(() => {
    if (!gameRowRef.current)
      return;
    const newHeight = Math.max(236, gameRowRef.current.clientHeight);
    if (newHeight === rowHeight) return;

    setRowHeight(newHeight);
  });
  if (!props.games) return null;

  const remainingGames = props.games.length - (offset-offsetComp);
  const showForward = remainingGames >= limit;
  return <div className='row'>
    <div className='col-12'>
      <h3 className={s.title}>{props.title.toUpperCase()}</h3>
    </div>
    <div className={props.ads ? 'col-12 col-lg-9' : 'col-12'} ref={rowRef}>
      <div className={s.goBack + ' ' + s.pagerButton + ' ' + (offset === 0 ? 'opacity-25' : '')} onClick={() => {
        if (offset === 0)
          return;

        setOffset((offset - limit));
      }} style={{height: rowHeight}}>
        <p className={s.pagerCaret}><span className={s.caretRight}>^</span></p>
      </div>

      {showForward ? <div className={s.goForward + ' ' + s.pagerButton} onClick={() => {
        let newOffset = ((offset) + (limit));
        setOffset(newOffset);
      }} style={{height: rowHeight}}>
        <p className={s.pagerCaret}><span className={s.caretLeft}>^</span></p>
      </div> : null
      }
      <div className={'row ' + s.gameRow} ref={gameRowRef}>
        {
          props.games.slice(offset, offset+100).map((v, i) => {
            return <SmallGameCard
              key={i}
              className={s.gameCard}
              placeId={v.placeId}
              creatorId={v.creatorId}
              creatorType={v.creatorType}
              creatorName={v.creatorName}
              iconUrl={props.icons[v.universeId]}
              likes={v.totalUpVotes}
              dislikes={v.totalDownVotes}
              name={v.name}
              playerCount={v.playerCount}
            />
          })
        }
      </div>
    </div>
    {props.ads ? <div className='col-12 col-lg-3'><UserAdvertisement type={3} /></div> : null}
  </div>
}

export default GameRow;