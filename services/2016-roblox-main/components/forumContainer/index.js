import UserAdvertisement from "../userAdvertisement";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  forumColumn: {
  },
  forumContainer: {
    minWidth: '900px',
  },
});

const ForumContainer = props => {
  const s = useStyles();
  return <div className={'container ' + s.forumContainer}>
    <div className='row'>
      <div className='col-10'>
        <UserAdvertisement type={1} />
        <div className='mt-4'>
          <div className='bg-white pt-2 pb-2'>
            {props.children}
          </div>
        </div>
      </div>
      <div className='col-2'>
        <UserAdvertisement type={2} />
      </div>
    </div>
  </div>
}

export default ForumContainer;