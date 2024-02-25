import GameDetailsStore from "../stores/gameDetailsStore";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  descriptionText: {
    whiteSpace: 'break-spaces',
  },
})

const Description = props => {
  const store = GameDetailsStore.useContainer();
  const s = useStyles();
  return <div className='row'>
    <div className='col-12'>
      <p className={'mb-0 mt-4 ' + s.descriptionText}>{store.details.description?.trim() || 'No description available'}</p>
    </div>
  </div>
}

export default Description;