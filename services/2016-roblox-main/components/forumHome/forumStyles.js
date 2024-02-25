import {createUseStyles} from "react-jss";

const useForumStyles = createUseStyles({
  header: {

  },
  headerRow: {
    background: '#29508d',
    '& th': {
      fontSize: '1.25rem',
      color: '#fff',
      fontWeight: 500,
      paddingLeft: '0.25rem',
      paddingTop: '0.5rem',
      paddingBottom: '0.5rem',
    }
  },
  bodyRow: {
    background: '#f9f9f9',
    '&:hover': {
      background: '#e9e9e9',
    },
  },
  lastPostHeader: {
    minWidth: '110px',
  },
})

export default useForumStyles;