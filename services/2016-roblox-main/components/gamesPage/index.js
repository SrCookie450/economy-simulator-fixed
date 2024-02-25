import React, {useEffect} from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../lib/getFlag";
import AuthenticationStore from "../../stores/authentication";
import GamesPageStore from "../../stores/gamesPage";
import AdBanner from "../ad/adBanner";
import Selector from "../selector";
import {getQueryParams} from "../../lib/getQueryParams";
import GamesList from './components/games';

const useStyles = createUseStyles({
  authContainer: {
    '@media(min-width: 1300px)': {
      marginLeft: '180px',
    }
  },
  selectorSort: {
    width: '200px',
    float: 'left',
  },
  gamesContainer: {
    backgroundColor: '#e3e3e3',
    paddingTop: '8px',
    marginLeft: '15px',
    marginRight: '15px',
  },
})

const Games = props => {
  const query = getQueryParams();
  const store = GamesPageStore.useContainer();
  const auth = AuthenticationStore.useContainer();
  const s = useStyles();
  const showGenre = getFlag('gameGenreFilterSupported', false) && !query.keyword;
  const showSortDropdown = getFlag('gameCustomSortDropdown', false) && !query.keyword;

  useEffect(() => {
    if (query.keyword)
      store.setQuery(query.keyword);

    store.loadGames({
      query: query.keyword,
      genreFilter: store.genreFilter,
    });
  }, [store.genreFilter]);

  // if (!store.sorts || !store.games || !store.icons) return null;
  return <div className={'row ' + (auth.isAuthenticated ? s.authContainer : '')}>
    <div className='col-12'>
      <AdBanner context='gamesPage'/>
    </div>
    <div className='col-12 ps-0 pb-0'>
      <div className={'row pb-2 ' + s.gamesContainer}>
        <div className='col-12'>
          {showSortDropdown &&
            <div className={s.selectorSort}>
              <Selector
                onChange={(newValue) => {
                  // TODO
                  console.log('[info] use sort', newValue);
                }}
                options={[
                  {
                    name: 'Default',
                    value: 'default',
                  },
                  {
                    name: 'Popular',
                    value: 'popular',
                  },
                  {
                    name: 'Top Earning',
                    value: 'top-earning',
                  },
                  {
                    name: 'Top Rated',
                    value: 'top-rated',
                  },
                  {
                    name: 'Recommended',
                    value: 'recommended',
                  },
                  {
                    name: 'Top Favorite',
                    value: 'top-favorite',
                  },
                  {
                    name: 'Top Paid',
                    value: 'top-paid',
                  },
                  {
                    name: 'Builders Club',
                    value: 'builders-club',
                  },
                ]}/>
            </div>
          }
          {showGenre &&
            <div className={s.selectorSort + ' ms-2'}>
              <Selector
                onChange={(newValue) => {
                  // TODO
                  console.log('[info] use genre', newValue);
                  store.setGenreFilter(newValue.value);
                }}
                options={store.selectorSorts}/>
            </div>
          }
        </div>
        <div className='col-12'>
          <GamesList />
        </div>
      </div>
    </div>
  </div>
}

export default Games;