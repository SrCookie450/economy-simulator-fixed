import React from "react";
import { createUseStyles } from "react-jss";
import UserAdvertisement from "../userAdvertisement";

const useStyles = createUseStyles({
  image: {
    display: 'block',
    margin: '0 auto',
    width: '100%',
    maxWidth: '728px',
    height: 'auto',
  }
})

const AdBanner = props => {
  const s = useStyles();
  return <div className='row'>
    <div className='col-12'>
      <UserAdvertisement type={1}></UserAdvertisement>
    </div>
  </div>
}

export default AdBanner;