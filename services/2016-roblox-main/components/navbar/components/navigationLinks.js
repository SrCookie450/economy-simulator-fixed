import { createUseStyles } from "react-jss";
import Link from "../../link";

const useStyles = createUseStyles({
  container: {
    marginTop: '3px',
    marginBottom: 0,
    paddingBottom: 0,
    paddingLeft: '0',
  },
  linkEntry: {
    color: 'white',
    fontWeight: 400,
    marginBottom: 0,
    paddingBottom: 0,
    textAlign: 'center',
    fontSize: '16px',

    textDecoration: 'none',
    padding: '4px 8px',
    transition: 'none',
    '&:hover': {
      color: 'white',
      background: 'rgba(25,25,25,0.1)',
      cursor: 'pointer',
      borderRadius: '4px',
      transition: 'none',
    },
  },
  navItem: {
    paddingRight: '2rem',
    '@media(max-width: 1300px)': {
      paddingRight: '1.75rem',
    },
    '@media(max-width: 1250px)': {
      paddingRight: '1.5rem',
    },
    '@media(max-width: 1175px)': {
      paddingRight: '1rem',
    },
  },
  col: {
    paddingLeft: 0,
    marginLeft: 0,
  }
})

const LinkEntry = props => {
  const s = useStyles();
  return <div className='col-3'>
    <Link href={`/${props.url}`}>
      <a className={`${s.linkEntry} nav-link active pt-0`}>
        {props.children}
      </a>
    </Link>
  </div>
}

const NavigationLinks = props => {
  const s = useStyles();
  return <div className={`${s.col} col-10 col-lg-5`}>
    <div className={s.container}>
      <div className='row'>
        <LinkEntry url='games'>Games</LinkEntry>
        <LinkEntry url='catalog'>Catalog</LinkEntry>
        <LinkEntry url='develop'>Develop</LinkEntry>
        <LinkEntry url='robux.aspx'>ROBUX</LinkEntry>
      </div>
    </div>
  </div>
}

export default NavigationLinks;