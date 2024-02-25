import {useEffect, useState} from "react";
import {createFavorite, deleteFavorite, getIsFavorited} from "../../../services/catalog";
import authentication from "../../../stores/authentication";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  wrapper: {
  },
  favoriteStar: {
    display: 'inline-block',
    width: '16px',
    height: '16px',
    background: 'url("/img/FavoriteStar.png")',
    marginBottom: '-2px',
  },
  favoriteCount: {
    textAlign: 'center',
  },
});

const Favorite = props => {
  const {assetId} = props;
  const auth = authentication.useContainer();
  const s = useStyles();

  const [isFavorited, setIsFavorited] = useState(null);
  const [favoriteCount, setFavoriteCount] = useState(0);
  const [locked, setLocked] = useState(false);

  useEffect(() => {
    setIsFavorited(null);
    setFavoriteCount(props.favoriteCount);
    setLocked(false);

    if (auth.userId) {
      getIsFavorited({assetId, userId: auth.userId}).then(data => {
        setIsFavorited(!!data);
      }).catch(e => {
        // undefined/null response causes axios to incorrectly return network error :)
        setIsFavorited(false);
      })
    }
  }, [props.favoriteCount, props.assetId, auth.userId]);

  return <div className={s.wrapper}>
    <div className={s.favoriteCount}><div className={s.favoriteStar}/>
       {favoriteCount.toLocaleString()}
      {
        isFavorited !== null ? <span className='ms-1'>
          <a href="#" onClick={e => {
            e.preventDefault();
            if (locked) return;
            setLocked(true);
            setIsFavorited(!isFavorited);
            setFavoriteCount(isFavorited ? favoriteCount-1 : favoriteCount+1);
            if (isFavorited) {
              deleteFavorite({userId: auth.userId, assetId}).finally(() => {
                setLocked(false);
              })
            }else{
              createFavorite({userId: auth.userId, assetId}).finally(() => {
                setLocked(false);
              })
            }
          }}>{isFavorited ? 'Unfavorite' : 'Favorite'}</a>
        </span> : null
      }
    </div>
  </div>
}

export default Favorite;