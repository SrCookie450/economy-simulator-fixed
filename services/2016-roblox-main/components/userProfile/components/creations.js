import { useEffect, useState } from "react";
import { multiGetUniverseIcons } from "../../../services/thumbnails";
import SmallGameCard from "../../smallGameCard";
import UserProfileStore from "../stores/UserProfileStore";
import Subtitle from "./subtitle";

const Creations = props => {
  const store = UserProfileStore.useContainer();
  const [icons, setIcons] = useState({});
  useEffect(() => {
    if (!store.createdGames || store.createdGames.length === 0) return
    multiGetUniverseIcons({
      universeIds: store.createdGames.map(v => v.id),
      size: '150x150',
    }).then(data => {
      let obj = {};
      data.forEach(v => {
        obj[v.targetId] = v.imageUrl;
      });
      setIcons(obj);
    })
  }, []);
  if (!store.createdGames || store.createdGames.length === 0) {
    return null;
  }

  return <div className='row'>
    <div className='col-12'>
      <Subtitle>Games</Subtitle>
    </div>
    <div className='col-12 ps-4 pe-4'>
      <div className='row'>
        {
          store.createdGames.map(v => {
            return <SmallGameCard key={v.id}
              name={v.name}
              likes={0}
              dislikes={0}
              playerCount={0}
              placeId={v.rootPlace.id}
              iconUrl={icons[v.id]}
              creatorType='User'
              creatorId={store.userId}
              creatorName={store.username}
            ></SmallGameCard>
          })
        }
      </div>
    </div>
  </div>
}

export default Creations;