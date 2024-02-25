import { createUseStyles } from "react-jss";
import TradeItemRow from "./tradeItemRow";

const useStyles = createUseStyles({
  value: {
    float: 'right',
    fontSize: '12px',
  },
  valueLabel: {
    fontWeight: 700,
    paddingRight: '4px',
  },
  robuxIconLg: {
    height: '10px',
    width: '14px',
    display: 'inline-block',
    verticalAlign: 'middle',
    margin: '0 2px 0 0',
  },
  robuxLabel: {
    fontWeight: 700,
    color: '#1a931a',
  },
});

const TradeOfferEntry = (props) => {
  const s = useStyles();
  const { label, offer } = props;
  const valueInRobux = offer && offer.userAssets.map(v => v.recentAveragePrice).reduce((a, b) => a + b, 0) || 0;

  return <>
    <div className='row'>
      <div className='col-8'>
        <p className='fw-700 font-size-15'>{label}</p>
      </div>
      <div className='col-4'>
        <p className={s.value}>
          <span className={s.valueLabel}>Value:</span> <img className={s.robuxIconLg} src='/img/img-robux.png' />
          <span className={s.robuxLabel}>{valueInRobux}</span>
        </p>
      </div>
    </div>
    {offer && <TradeItemRow items={offer.userAssets} robux={offer.robux}></TradeItemRow>}
  </>
}

export default TradeOfferEntry;