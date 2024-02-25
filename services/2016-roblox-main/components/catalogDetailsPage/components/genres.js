import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  header: {
    color: '#999',
    fontSize: '12px',
    marginBottom: 0,
  },
  genre: {
    fontSize: '12px',
    color: '#0055B3',
  },
});

const genreToHuman = str => {
  switch (str) {
    case 'TownAndCity':
      return 'Town and City';
    default:
      return str;
  }
}

const Genres = props => {
  const s = useStyles();
  return <div className='mt-3'>
    <p className={s.header}>Genres:</p>
    {props.genres.map(v => {
      return <p className={s.genre} key={v}>
        <div className={`GamesInfoIcon ${v}`}></div>
        {genreToHuman(v)}
      </p>
    })}
  </div>
}

export default Genres;