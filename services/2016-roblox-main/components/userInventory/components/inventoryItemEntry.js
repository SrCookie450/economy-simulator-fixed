import {createUseStyles} from "react-jss";
import ItemImage from "../../itemImage";
import {getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import Link from "../../link";

const useStyles = createUseStyles({
  itemCard: {
    background: '#fff',
    padding: '4px',
    borderRadius: '4px',
    boxShadow: '0 1px 3px rgba(150,150,150,0.75)',
    marginBottom: '1rem',
    '&:hover': {
      boxShadow: '0 1px 6px 0 #757575',
    },
    cursor: 'pointer',
  },
  serial: {
    background: '#000',
    color: '#fff',
    padding: '4px 8px',
    borderRadius: '4px',
    display: 'inline-block',
    float: 'right',
    fontSize: '12px',
    marginTop: '10px',
    marginRight: '10px',
    marginBottom: '-34px',
    zIndex: '2',
    position: 'relative',
  },
  fakeLimitedLabel: {
    width: '100%',
    height: '16px',
    display: 'inline-block',
  },
  itemImage: {
    marginTop: '-20px',
    marginBottom: '-16px',
  },
  itemLabel: {
    fontWeight: '400',
    fontSize: '12px',
    color: 'rgb(25, 25, 25)',
    borderTop: '1px solid #f2f2f2',
    marginTop: '2px',
  },
  creatorLabel: {
    fontWeight: '400',
    fontSize: '12px',
    color: '#757575',
    marginTop: '2px',
  },
  creatorUrl: {
    color: 'rgb(25, 25, 25)',
  },
  column: {
    paddingLeft: '4px',
    paddingRight: '4px',
  },
});

const InventoryItemEntry = props => {
  const s = useStyles();
  const {isLimited, isLimitedUnique, serialNumber} = props;

  return <div className={'col-4 col-md-3 col-lg-2 ps-1 pe-1'}>
    <Link href={getItemUrl({name: props.name, assetId: props.id})}>
      <a>
        <div className={s.itemCard}>
          {typeof serialNumber === 'number' ? <p className={s.serial}>#{serialNumber}</p> : null }
          <div className={s.itemImage}>
            <ItemImage id={props.id} />
          </div>
          <span className={isLimitedUnique ? "icon-limited-unique-label" : isLimited ? "icon-limited-label" : s.fakeLimitedLabel}/>
          <p className={s.itemLabel + ' text-truncate'}>{props.name}</p>
          <p className={s.creatorLabel + ' text-truncate'}>By <a className={s.creatorUrl} href={props.creatorType === 1 ? `/users/${props.creatorId}/profile` : `/My/Groups.aspx?gid=${props.creatorId}`}>{props.creatorName}</a> </p>
        </div>
      </a>
    </Link>
  </div>
}

export default InventoryItemEntry;