import { createUseStyles } from "react-jss";
import Link from "../../link";

const useStyles = createUseStyles({
  startPlaceLabel: {
    color: '#999',
    paddingRight: '8px',
    fontSize: '13px',
  },
  visibilityButton: {

  },
});
const AssetListGameEntry = props => {
  const s = useStyles();
  return <div>
    <p className='mb-0'><span className={s.startPlaceLabel}>Start Place: </span> <Link href={props.url}><a>{props.startPlaceName}</a></Link></p>
    <p className={s.visibilityButton + ' mb-0 mt-1'}>
      <Link href={props.url + '#/#basicSettings'}>
        <a>
          Public
        </a>
      </Link>
    </p>
  </div>
}

export default AssetListGameEntry;