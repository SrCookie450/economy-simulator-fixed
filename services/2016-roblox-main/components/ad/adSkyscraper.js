import { createUseStyles } from "react-jss";
import UserAdvertisement from "../userAdvertisement";

const useStyles = createUseStyles({
  image: {
    display: 'block',
    margin: '0 auto',
    width: '100%',
    maxWidth: '160px',
    height: 'auto',
  }
})

const AdSkyscraper = props => {
  const s = useStyles();
  return <div className='row'>
    <div className='col-12'>
      <UserAdvertisement type={2}></UserAdvertisement>
    </div>
  </div>
}

export default AdSkyscraper;