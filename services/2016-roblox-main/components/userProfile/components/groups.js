import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../../lib/numberUtils";
import { multiGetGroupIcons } from "../../../services/thumbnails";
import UserProfileStore from "../stores/UserProfileStore";
import SmallButtonLink from "./smallButtonLink";
import Subtitle from "./subtitle";
import Link from "../../link";

const useGroupGridEntryStyles = createUseStyles({
  groupImage: {
    width: '100%',
    height: 'auto',
    margin: '0 auto',
    display: 'block',
  },
  name: {
    fontWeight: 300,
    marginBottom: 0,
    fontSize: '16px',
    color: '#757575',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  memberCount: {
    fontWeight: 400,
    fontSize: '12px',
    marginTop: '2px',
    marginBottom: 0,
    color: '#757575',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
})

const GroupGridEntry = props => {
  const s = useGroupGridEntryStyles();
  return <div className='col-6 col-lg-2 pe-0 ps-0'>
    <Link href={`/My/Groups.aspx?gid=${props.group.group.id}`}>
      <a>
        <div className='card pt-1 pb-1 pe-1 ps-1'>
          <img className={s.groupImage} src={props.icon}/>
          <div className='pe-1 ps-1'>
            <p className={s.name}>{props.group.group.name}</p>
            <p className={s.memberCount}>{abbreviateNumber(props.group.group.memberCount)} Members</p>
            <p className={s.memberCount}>{props.group.role.name}</p>
          </div>
        </div>
      </a>
    </Link>
  </div>
}

const GroupGrid = (props) => {
  const store = UserProfileStore.useContainer();
  return <div className='row ps-3 pe-3'>
    {
      store.groups.map(v => {
        return <GroupGridEntry key={v.group.id} group={v} icon={props.icons[v.group.id]}></GroupGridEntry>
      })
    }
  </div>
}


const useGroupSquareStatStyles = createUseStyles({
  header: {
    color: '#c3c3c3',
    fontSize: '16px',
    fontWeight: 400,
    marginBottom: 0,
  },
  value: {
    fontSize: '16px',
    fontWeight: 400,
  },
});

const GroupSquareStat = props => {
  const s = useGroupSquareStatStyles();
  return <div className='row'>
    <div className='col-12'>
      <p className={s.header}>{props.header}</p>
      <p className={s.value}>{props.value}</p>
    </div>
  </div>
}

const useGroupSquareStyles = createUseStyles({
  iconCard: {
    background: '#0074bd',
    borderRadius: 0,
    height: '100%',
  },
  groupName: {
    fontWeight: 400,
    margin: 0,
    fontSize: '30px',
    borderBottom: '1px solid #c3c3c3',
  },
  description: {
    fontWeight: 300,
    marginBottom: 0,
    height: '150px',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    marginTop: '4px',
    fontSize: '16px',
  },
  cursor: {
    height: '50px',
    width: '50px',
    background: 'rgba(0,0,0,0.5)',
    position: 'relative',
    borderRadius: '100%',
    cursor: 'pointer',
    marginTop: '-50px',
    '&:hover': {
      background: 'rgba(0,0,0,0.75)',
    },
  },
  cursorChevron: {
    color: 'white',
    fontSize: '40px',
    paddingLeft: '18px',
    marginTop: '-10px',
    position: 'relative',
    top: '-4px',
    userSelect: 'none',
  },
  cursorBack: {
    border: '2px solid white',
    marginLeft: '20px',
    top: '160px',
  },
  cursorForward: {
    float: 'right',
    top: '-120px',
    marginRight: '20px',
  },
  cursorShevronForward: {
    paddingLeft: '20px',
  },
  image: {
    width: '240px',
    height: '240px',
    display: 'block',
    margin: '0 auto',
    borderRadius: '12px',
    boxShadow: '0 0 6px 0 rgb(0 0 0 / 49%)',
  },
  imageWrapper: {
    paddingTop: '20px',
  },
});

const GroupSquareEntry = props => {
  const s = useGroupSquareStyles();
  const [offset, setOffset] = useState(0);
  const [showCursor, setShowCursor] = useState(false);
  const store = UserProfileStore.useContainer();
  const group = store.groups[offset];

  return <div className='row' onMouseEnter={() => {
    if (store.groups.length === 1) {
      return
    }
    setShowCursor(true);
  }} onMouseLeave={() => {
    setShowCursor(false);
  }}>
    <div className='col-12 col-lg-6 pe-lg-0'>
      {showCursor && <div onClick={() => {
        if (offset === 0) {
          setOffset(store.groups.length - 1);
        } else {
          setOffset(offset - 1);
        }
      }} className={s.cursor + ' ' + s.cursorBack}>
        <span className={s.cursorChevron}>‹</span></div>}
      <div className={s.iconCard}>
        <div className={s.imageWrapper}>
          <Link href={`/My/Groups.aspx?gid=${group.group.id}`}>
            <a>
              <img className={s.image} src={props.icons[group.group.id]}/>
            </a>
          </Link>
        </div>
      </div>
    </div>
    <div className='col-12 col-lg-6 ps-lg-0'>
      <div className='card pt-4 pb-4 ps-4 pe-4'>
        <h3 className={s.groupName}>
          {group.group.name}
        </h3>
        <p className={s.description}>
          {group.group.description}
        </p>
        <div className='row'>
          <div className='col-6'>
            <GroupSquareStat header='Members' value={abbreviateNumber(group.group.memberCount)}></GroupSquareStat>
          </div>
          <div className='col-6'>
            <GroupSquareStat header='Rank' value={group.role.name}></GroupSquareStat>
          </div>
        </div>
      </div>
      {showCursor && <div onClick={() => {
        if (offset >= (store.groups.length - 1)) {
          setOffset(0);
        } else {
          setOffset(offset + 1);
        }
      }} className={s.cursor + ' ' + s.cursorForward}>
        <span className={s.cursorChevron + ' ' + s.cursorShevronForward}>›</span></div>}
    </div>
  </div>
}

const useGroupStyles = createUseStyles({
  buttonsGroup: {
    paddingTop: '10px',
  },
  buttonInUse: {
    paddingTop: 0,
    paddingBottom: 0,
  },
  buttonNotInUse: {
    background: 'white !important',
    border: '1px solid #c3c3c3',
    paddingTop: 0,
    paddingBottom: 0,
  },
})

const Groups = props => {
  const store = UserProfileStore.useContainer();
  const s = useGroupStyles();
  const [mode, setMode] = useState('Square');
  const [icons, setIcons] = useState({});
  useEffect(() => {
    if (!store.groups || !store.groups.length) return;
    multiGetGroupIcons({
      groupIds: store.groups.map(v => v.group.id),
    }).then(newIcons => {
      let obj = {};
      for (const item of newIcons) {
        obj[item.targetId] = item.imageUrl;
      }
      console.log(obj);
      setIcons(obj);
    })
  }, [store.groups]);

  if (!store.groups || !store.groups.length) {
    return null;
  }
  return <div className='row'>
    <div className='col-6'>
      <Subtitle>Groups</Subtitle>
    </div>
    <div className={'col-3 col-lg-1 offset-lg-4 pe-1 ' + s.buttonsGroup}>
      <SmallButtonLink onClick={(e) => {
        e.preventDefault();
        setMode('Square');
      }} className={mode === 'Square' ? s.buttonInUse : s.buttonNotInUse}>
        <span className='container-buttons'>
          <span className='icon-slideshow'>

          </span>
        </span>
      </SmallButtonLink>
    </div>
    <div className={'col-3 col-lg-1 ps-1 ' + s.buttonsGroup}>
      <SmallButtonLink onClick={(e) => {
        e.preventDefault();
        setMode('Grid');
      }} className={mode === 'Grid' ? s.buttonInUse : s.buttonNotInUse}>
        <span className='container-buttons'>
          <span className='icon-grid'></span>
        </span>
      </SmallButtonLink>
    </div>
    <div className='col-12'>
      {mode === 'Square' ? <GroupSquareEntry icons={icons}></GroupSquareEntry> : <GroupGrid icons={icons}></GroupGrid>}
    </div>
  </div>
}

export default Groups;