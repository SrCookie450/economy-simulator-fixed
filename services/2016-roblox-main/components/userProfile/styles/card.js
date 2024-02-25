import { createUseStyles } from "react-jss";

const useCardStyles = createUseStyles({
  card: {
    borderRadius: 0,
    boxShadow: '0 1px 4px 0 rgb(25 25 25 / 30%)',
    background: 'white',
  },
});

export default useCardStyles;