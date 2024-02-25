import { createUseStyles } from "react-jss";

const useSaleStyles = createUseStyles({
  overlay: {
    marginBottom: '-77px',
    float: 'right',
    width: '72px',
    height: '72px',
    zIndex: 2,
    position: 'relative',
  },
});

const SaleOverlay = props => {
  const s = useSaleStyles();
  return <img src='/img/CatalogOverlays/Sale.png' className={s.overlay} alt='Sale'></img>
}

export default SaleOverlay;