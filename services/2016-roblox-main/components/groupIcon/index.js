import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getBaseUrl } from "../../lib/request";
import { reportImageFail } from "../../services/metrics";
import thumbnailStore from "../../stores/thumbnailStore";

const useStyles = createUseStyles({
  image: {
    maxWidth: '400px',
    width: '100%',
    margin: '0 auto',
    height: 'auto',
    display: 'block',
  },
})

const GroupIcon = (props) => {
  const s = useStyles();
  const size = props.size || 420;
  const [retryCount, setRetryCount] = useState(0);
  const thumbs = thumbnailStore.useContainer();
  const [image, setImage] = useState(props.url ? props.url : thumbs.getGroupIcon(props.id, '420x420'));

  useEffect(() => {
    if (props.url) {
      setImage(props.url)
      return
    }
    setRetryCount(0);
    setImage(thumbs.getGroupIcon(props.id, '420x420'));
  }, [props]);

  useEffect(() => {
    if (props.url) {
      return
    }
    setImage(thumbs.getGroupIcon(props.id, '420x420'));
  }, [thumbs.thumbnails]);

  return <img className={s.image} src={image} alt={props.name} onError={(e) => {
    if (retryCount >= 3) return;
    reportImageFail({
      errorEvent: e,
      type: 'groupIcon',
      src: image,
    })
    setRetryCount(retryCount + 1);
    setImage('/img/placeholder.png')
  }} />
}

export default GroupIcon;