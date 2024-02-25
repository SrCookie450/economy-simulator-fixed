import ExchangeStore from "../stores/exchangeStore";
import {createUseStyles} from "react-jss";
import {useEffect, useRef, useState} from "react";
import {closePosition, countOpenPositions, getOpenPositions} from "../../../../../services/economy";
import PositionsPaging from "./positionsPaging";
import Table from "../../table";

const Side = ({children, title}) => {
  return <div className='w-100'>
    <p className='fw-bolder mb-0'>{title}</p>
    {children}
  </div>
}

const useStyles = createUseStyles({
  main: {
    borderTop: '1px solid #ccc',
    borderRight: '1px solid #ccc',
    marginTop: '52px',
  }
});

const PositionsTab = props => {
  const {currency} = props;
  const store = ExchangeStore.useContainer();
  const s = useStyles();
  const [count, setCount] = useState(null);
  const [positions, setPositions] = useState(null);
  const [startId, setStartId] = useState(0);
  const [page, setPage] = useState(1);
  const startIdHistory = useRef({}); // map of page number to start id
  useEffect(() => {
    setCount(null);
    countOpenPositions({currency}).then(c => {
      setCount(c);
    })
  }, [currency]);

  useEffect(() => {
    if (count === null || count <= 0) return;
    startIdHistory.current[page] = page === 1 ? 0 : positions[0].id;
    getOpenPositions({
      startId: startIdHistory.current[page],
      limit: 1,
      currency,
    }).then(data => {
      if (data.data.length)
        startIdHistory.current[page+1] = data.data[0].id;
      setPositions(data.data);
    })
  }, [page, count]);

  if (!store.statistics)
    return null;

  return <div className='col-8'>
    <div className='row'>
      <div className={'col-8 ' + s.main}>
        {count === 0 ? <p className='text-center mt-4'>You do not have any open {currency === 1 ? 'ROBUX' : 'TIX'} trades.</p> :

        positions ? <Table ad={false} keys={['Offer', 'Remainder', 'Action']} entries={positions.map(v => {
          let rate = (v.exchangeRate / 1000);
          let rateStr = '@ 1:' + (v.exchangeRate / 1000);
          if (rate < 1) {
            let reversedRate = (1 / rate).toFixed(3);
            rateStr = '@ ' + reversedRate + ':1';
          }
          return [
            v.startAmount + ' ' + (v.sourceCurrency === 'Robux' ? 'RS' : 'TX') + ' ' + rateStr,
            v.balance.toLocaleString(),
            <span>
              <a href='#' onClick={() => {
                closePosition({
                  orderId: v.id,
                }).then(() => {
                  setPositions(positions.filter(c => c.id !== v.id));
                }).catch(e => {
                  //todo: feedback?
                })
              }}>Cancel</a>
            </span>
          ];
        })}/>
           : null
        }

        <PositionsPaging positions={positions} count={count} setStartId={setStartId} startIdHistory={startIdHistory} page={page} setPage={setPage} />
      </div>
      <div className='col-4'>
        <div className='row'>
          <div className='col-6'>
            <Side title='Pair'>
              <p>BUX/TIX</p>
            </Side>
            <Side title='Spread'>
              <p>
                {(store.statistics.average.robuxToTickets - store.statistics.average.ticketsToRobux) / 1000}
              </p>
            </Side>
            <Side title='Available Tickets'>
              {
                store.statistics.positions.tickets.map(v => {
                  let rate = v.rate / 1000;
                  let str = '@ ' + (rate).toFixed(3) + ':1';
                  if (rate < 1) {
                    str = '@ 1:' + (1/rate).toFixed(3);
                  }
                  return <p key={v.rate + ' '  +v.amount} className='mb-0'>{v.amount} {str}</p>
                })
              }
            </Side>
          </div>
          <div className='col-6'>
            <Side title='Rate'>
              <p>
                {store.statistics.average.robuxToTickets / 1000}/{store.statistics.average.ticketsToRobux / 1000}
              </p>
            </Side>
            <Side title='High/Low'>
              <p>
                {store.statistics.high.robuxToTickets / 1000}/{store.statistics.low.robuxToTickets / 1000}
              </p>
            </Side>
            <Side title='Available Robux'>
              {
                store.statistics.positions.robux.map(v => {
                  let rate = v.rate / 1000;
                  let str = '@ ' + (rate).toFixed(3) + ':1';
                  if (rate < 0) {
                    rate = 1 / rate;
                    str = '@ 1:' + (rate).toFixed(3);
                  }
                  return <p key={v.rate + ' '  +v.amount} className='mb-0'>{v.amount} {str}</p>
                })
              }
            </Side>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default PositionsTab;