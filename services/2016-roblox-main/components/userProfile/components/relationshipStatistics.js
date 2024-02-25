import { createUseStyles } from "react-jss";
import { abbreviateNumber } from "../../../lib/numberUtils";
import Link from "../../link";

const useStyles = createUseStyles({
  statHeader: {
    color: '#c3c3c3',
    fontWeight: 400,
    marginBottom: 0,
    textAlign: 'center',
    fontSize: '18px',
  },
  statValue: {
    fontWeight: 300,
    marginBottom: 0,
    textAlign: 'center',
    fontSize: '20px',
    '&> a': {
      color: '#00A2FF',
      '&:hover': {
        textDecoration: 'underline!important',
      }
    }
  },
});

const RelationshipStatistics = props => {
  const { id, label, value, userId } = props;
  const s = useStyles();

  return <div className='col-12 col-lg-2'>
    <p className={s.statHeader}>{label}</p>
    <p className={s.statValue}>
      <Link href={`/users/${userId}/friends#!${id}`}>
        <a>
          {Number.isSafeInteger(value) ? abbreviateNumber(value) : '...'}
        </a>
      </Link>
    </p>
  </div>
}

export default RelationshipStatistics;