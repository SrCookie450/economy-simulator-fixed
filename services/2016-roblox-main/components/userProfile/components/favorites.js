import {useEffect, useState} from "react";
import {getFavorites} from "../../../services/inventory";
import Subtitle from "./subtitle";
import SmallButtonLink from "./smallButtonLink";
import {createUseStyles} from "react-jss";
import GameRow from "../../gamesPage/components/gameRow";
import SmallGameCard from "../../smallGameCard";
import {multiGetUniverseIcons} from "../../../services/thumbnails";

const useStyles = createUseStyles({
  buttonWrapper: {
    width: '100px',
    float: 'right',
    marginTop: '10px',
  },
});

const Favorites = props => {
  const {userId} = props;
  const [favorites, setFavorites] = useState(null);
  const [icons, setIcons] = useState({});
  useEffect(() => {
    setFavorites(null);
    getFavorites({
      userId: userId,
      limit: 6,
      assetTypeId: 9,
    }).then(data => {
      setFavorites(data.Data.Items);
      multiGetUniverseIcons({
        universeIds: data.Data.Items.map(v => v.Item.UniverseId),
      }).then(d => {
        let newObj = {}
        for (const item of d) {
          newObj[item.targetId] = item.imageUrl;
        }
        setIcons(newObj);
      })
    })
  }, [userId]);
  const s = useStyles();

  if (favorites === null || favorites.length === 0) return null;
  return <div className='row'>
    <div className='col-10'>
      <Subtitle>Favorites</Subtitle>
    </div>
    <div className='col-2'>
      <div className={s.buttonWrapper}>
        <SmallButtonLink href={`/users/${userId}/favorites`}>Favorites</SmallButtonLink>
      </div>
    </div>
    <div className='col-12'>
      <div className='row ms-0 me-0'>
        {
          favorites.slice(0,6).map(v => {
            return <SmallGameCard
              key={v.Item.AssetId}
              name={v.Item.Name}
              creatorId={v.Creator.Id}
              creatorName={v.Creator.Name}
              creatorType={v.Creator.Type}
              placeId={v.Item.AssetId}
              iconUrl={icons[v.Item.UniverseId]}
              hideVoting={true}
              playerCount={0}
            />
          })
        }
      </div>
    </div>
  </div>
}

export default Favorites;