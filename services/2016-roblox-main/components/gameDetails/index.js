// reference: https://web.archive.org/web/20150228101148mp_/http://www.roblox.com/Work-at-a-Pizza-Place-place?id=192800
import React, { useEffect } from "react";
import { createUseStyles } from "react-jss";
import AdBanner from "../ad/adBanner";
import AdSkyscraper from "../ad/adSkyscraper";
import Comments from "../catalogDetailsPage/components/comments";
import Recommendations from "../catalogDetailsPage/components/recommendations";
import OldVerticalTabs from "../oldVerticalTabs";
import GameOverview from "./components/gameOverview";
import GameServers from "./components/gameServers";
import GameDetailsStore from "./stores/gameDetailsStore"

const useStyles = createUseStyles({
  gameContainer: {
    backgroundColor: '#fff',
    padding: '4px 8px',
    overflow: 'hidden',
  }
})

const GameDetails = props => {
  const { details } = props;
  const s = useStyles();

  const store = GameDetailsStore.useContainer();
  useEffect(() => {
    store.setDetails(details);
  }, [props]);

  if (!store.details) return null;
  return <div className='container'>
    <AdBanner></AdBanner>
    <div className={s.gameContainer}>
      <div className='row mt-2'>
        <div className='col-12 col-lg-10'>
          <GameOverview></GameOverview>
          <div className='row mt-4'>
            <div className='col-12'>
              <OldVerticalTabs options={[
                {
                  name: 'Recommendations',
                  element: <Recommendations assetId={details.id} assetType={9}></Recommendations>
                },
                {
                  name: 'Games',
                  element: <GameServers></GameServers>
                },
                {
                  name: 'Commentary',
                  element: <Comments assetId={details.id}></Comments>
                },
              ]}></OldVerticalTabs>
            </div>
          </div>
        </div>
        <div className='d-none d-lg-flex col-2'>
          <AdSkyscraper></AdSkyscraper>
        </div>
      </div>
    </div>
  </div>
}

export default GameDetails;