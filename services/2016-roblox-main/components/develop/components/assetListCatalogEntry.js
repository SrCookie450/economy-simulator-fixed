import dayjs from "dayjs";
import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  createdLabel: {
    color: '#999',
  },
})

const AssetListCatalogEntry = props => {
  const s = useStyles();
  return <div>
    <p className='mb-0'><span className={s.createdLabel}>Created: </span> {dayjs(props.created).format('M/d/YYYY')}</p>
  </div>
}

export default AssetListCatalogEntry;