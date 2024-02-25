import SmallGameCard from "../../smallGameCard";
import GameRow, {useStyles as useGameRowStyles} from "./gameRow";
import React from "react";
import GamesPageStore from "../../../stores/gamesPage";

const Games = props => {
  const store = GamesPageStore.useContainer();
  const gameS = useGameRowStyles();
  let existingGames = {}

  if (store.infiniteGamesGrid) {
    if (store.infiniteGamesGrid.games.length === 0) {
      return <p className='mt-4'>No results.</p>
    }
    return <div className='row'>
      {
        store.infiniteGamesGrid.games.map(v => {
          return <SmallGameCard
            key={v.universeId}
            className={gameS.gameCard + ' mb-3'}
            placeId={v.placeId}
            creatorId={v.creatorId}
            creatorType={v.creatorType}
            creatorName={v.creatorName}
            iconUrl={store.icons[v.universeId]}
            likes={v.totalUpVotes}
            dislikes={v.totalDownVotes}
            name={v.name}
            playerCount={v.playerCount}
          />
        })
      }
    </div>
  }
  return <div className='row'>
    {
      store.sorts ? store.sorts.map(v => {
        if (existingGames[v.token]) {
          return null;
        }
        existingGames[v.token] = true;
        let games = store.games && store.games[v.token] || null;
        return <GameRow ads={true} key={'row ' + v.token} title={v.displayName} games={games} icons={store.icons}/>
      }) : null
    }
  </div>
}

export default Games;