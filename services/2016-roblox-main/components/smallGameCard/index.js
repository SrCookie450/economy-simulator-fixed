import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../lib/numberUtils";
import { itemNameToEncodedName } from "../../services/catalog";
import { getGameUrl } from "../../services/games";
import CreatorLink from "../creatorLink";
import useCardStyles from "../userProfile/styles/card";
import Link from "../link";

const useStyles = createUseStyles({
  label: {
    fontWeight: 300,
    fontSize: '16px',
    marginBottom: 0,
  },
  labelPlaying: {
    fontSize: '12px',
    marginBottom: 0,
    color: '#757575',
  },
  imageWrapper: {
    padding: '8px',
  },
  image: {
    width: '100%',
    margin: '0 auto',
    display: 'block',
  },
  thumbsUp: {
    marginBottom: 0,
  },
  creatorDetailsCard: {
    position: 'absolute',
    background: 'white',
    marginLeft: '-7px',
    boxShadow: '0 3px 4px 0 rgb(25 25 25 / 30%)',
  },
  creatorText: {
    color: '#c3c3c3',
    fontSize: '13px',
    '&>a': {
      color: '#00a2ff',
      '&:hover': {
        textDecoration: 'underline!important',
      },
    },
  },
  floatRight: {
    float: 'right',
  },

  ratioBox: {
    width: '20px',
    height: '5px',
    background: '#c3c3c3',
    display: 'inline-block',
    marginLeft: '3px',
  },
  ratioBoxPart: {
    height: '5px',
    display: 'inline-block',
  },
  radioBoxPartContainer: {
    display: 'inline-block',
    marginLeft: '3px',
  },
  ratioContainer: {
    display: 'inline-block',
  },
  solidGreen: {
    background: '#757575',
  },
  solidRed: {
    background: '#c3c3c3',
  },
  solidGreenColor: {
    background: '#02b757',
  },
  solidRedColor: {
    background: '#E27676',
  },
});

/**
 * SmallGameCard
 * @param {{name: string; playerCount: number; likes: number; dislikes: number; creatorId: number; creatorType: string | number; creatorName: string; iconUrl: string; placeId: number; className?: string; hideVoting?: boolean;}} props
 * @returns 
 */
const SmallGameCard = props => {
  const {hideVoting} = props;

  const [dimensions, setDimensions] = useState({
    height: window.innerHeight,
    width: window.innerWidth
  });
  useEffect(() => {
    window.addEventListener('resize', () => {
      setDimensions({
        height: window.innerHeight,
        width: window.innerWidth
      });
    });
  }, []);

  const s = useStyles();
  const cardStyles = useCardStyles();
  const [showCreator, setShowCreator] = useState(false);
  const [iconUrl, setIconUrl] = useState('/img/empty.png');
  const [boxWidth, setBoxWidth] = useState(0);
  const cardRef = useRef(null);

  let likePercent = (props.likes / (props.likes+props.dislikes));
  if (isNaN(likePercent) || likePercent < 0) {
    likePercent = 0;
  }
  let numSolidGreen = Math.trunc(likePercent * 5);
  let numSolidRed = Math.max(0, 5 - numSolidGreen);
  const squares = [];
  for (let i = 0; i < numSolidGreen; i++) {
    squares.push({
      solidGreen: true,
    });
  }
  if (numSolidGreen !== likePercent * 5) {
    let remainder = (likePercent * 5) - numSolidGreen;
    numSolidRed--;
    squares.push({
      percentGreen: remainder,
    });
  }
  for (let i = 0; i < numSolidRed; i++) {
    squares.push({
      solidRed: true,
    });
  }

  useEffect(() => {
    if (cardRef.current) {
      const width = cardRef.current.clientWidth;
      setBoxWidth((width - 75) / 5);
    }
  }, [dimensions]);

  useEffect(() => {
    if (!props.iconUrl) {
      setIconUrl('/img/empty.png');
      return
    }
    setIconUrl(props.iconUrl);
  }, [props.iconUrl]);

  const colRef = useRef(null);
  const url = getGameUrl({
    placeId: props.placeId,
    name: props.name,
  });

  const Voting = (props) => {
    const {color} = props;
    const sGreen = color ? s.solidGreenColor : s.solidGreen;
    const sRed = color ? s.solidRedColor : s.solidRed;

    return <div className={s.ratioContainer + ' '}>
      {squares.map((v, i) => {
        if (v.percentGreen) {
          const widthGreen = boxWidth * v.percentGreen;
          const widthRed = boxWidth - widthGreen;
          return <div key={i} className={s.radioBoxPartContainer}>
            <div className={s.ratioBoxPart + ' ' + sGreen} style={{width: widthGreen}} />
            <div className={s.ratioBoxPart + ' ' + sRed} style={{width: widthRed}} />
          </div>
        }
        return <div key={i} className={s.ratioBox + ' ' + (v.solidGreen ? sGreen : v.solidRed ? sRed : '')} style={{width: boxWidth}} />
      })}
    </div>
  }

  return <div ref={cardRef} className={props.className || 'col-6 col-lg-2 ps-1 pe-1'} onMouseEnter={() => {
    setShowCreator(true);
  }} onMouseLeave={() => {
    setShowCreator(false);
  }}>
    <div className={cardStyles.card + ' '} ref={colRef}>
      <Link href={url}>
        <a>
          <div className={s.imageWrapper}>
            <img className={s.image} src={iconUrl} alt={props.name} onLoad={(e) => {
            }} onError={(e) => {
              if (!iconUrl || iconUrl.indexOf('empty.png') !== -1) return;
              setIconUrl('/img/empty.png');
              setTimeout(() => {
                setIconUrl(props.iconUrl);
              }, 1000);
            }}/>
          </div>
        </a>
      </Link>
      <div className='pe-2 pb-2 pt-2 ps-2'>
        <p className={s.label + ' truncate'}>{props.name}</p>
        <p className={s.labelPlaying + ' truncate'}>{abbreviateNumber(props.playerCount)} Playing</p>
        {
          !showCreator && !hideVoting && <p className={s.thumbsUp + ' mt-2 d-inline-block'}>
            <span className='icon-thumbs-up'/>
          </p> || null
        }

        {
          !showCreator && !hideVoting ?  <Voting /> : null
        }

        {
          showCreator && <div className={s.creatorDetailsCard + ' ' + cardStyles.card} style={colRef ? { width: colRef.current.clientWidth + 'px' } : undefined}>
            {!hideVoting ?
            <>
              <p className={s.thumbsUp + ' ps-2 pe-2 mt-2'}>
                <span className='icon-thumbs-up colored'/>
                <Voting color={true} />
                <span className={'icon-thumbs-down colored ' + s.floatRight}/>
              </p>
              <div className='ps-1 pt-2 pe-1'>
                <div className='divider-top'/>
              </div>
            </> : null}
            <p className={'ps-2 pt-2 pb-0 ' + s.creatorText}>By <CreatorLink type={props.creatorType} name={props.creatorName} id={props.creatorId}/></p>
          </div>
        }
      </div>
    </div>
  </div>
}

export default SmallGameCard;