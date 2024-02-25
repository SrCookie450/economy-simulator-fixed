import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../stores/catalogPage";

const useStyles = createUseStyles({
  label: {
    fontSize: '13px',
    paddingLeft: '4px',
  },
  header: {
    fontWeight: 600,
    fontSize: '16px',
  },
  allGenres: {
    paddingLeft: '5px',
    fontWeight: 600,
    marginBottom: 0,
    paddingBottom: 0,
  },
})

const GenreFilter = props => {
  const store = CatalogPageStore.useContainer();
  const s = useStyles();
  const genres = [
    { genre: 13, name: 'Building' },
    { genre: 5, name: 'Horror' },
    { genre: 1, name: 'Town and City' },
    { genre: 11, name: 'Military' },
    { genre: 9, name: 'Comedy' },
    { genre: 2, name: 'Medieval' },
    { genre: 7, name: 'Adventure' },
    { genre: 3, name: 'Sci-Fi' },
    { genre: 6, name: 'Naval' },
    { genre: 14, name: 'FPS' },
    { genre: 15, name: 'RPG' },
    { genre: 8, name: 'Sports' },
    { genre: 4, name: 'Fighting' },
    { genre: 10, name: 'Western' },
  ];
  return <div>
    <p className={s.header}>Genre</p>
    <p className={s.allGenres + ' cursor-pointer'} onClick={() => {
      store.setGenres([]);
    }}>All Genres</p>
    {
      genres.map(v => {
        const id = 'catalog_genre_fitler_' + v.genre;
        return <p className='mb-0' key={id}>
          <input id={id} type='checkbox' checked={store.genres.includes(v.genre)} onChange={(c) => {
            if (c.currentTarget.checked === false) {
              store.setGenres(store.genres.filter(x => x !== v.genre));
            } else {
              store.setGenres([...store.genres, v.genre]);
            }
          }}></input>
          <label htmlFor={id} className={s.label}>{v.name}</label>
        </p>
      })
    }
  </div>
}

export default GenreFilter;