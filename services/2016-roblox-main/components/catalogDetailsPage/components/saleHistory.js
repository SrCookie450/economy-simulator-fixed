import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import { getResaleData } from "../../../services/economy";
import CatalogDetailsPage from "../stores/catalogDetailsPage";
import dayjs from "dayjs";

const useSaleHistoryStyles = createUseStyles({
  rap: {
    color: '#060',
    fontWeight: '600',
  },
  rapText: {
    fontSize: '14px',
    marginBottom: 0,
  },
  row: {
    marginTop: '55px',
  },
  daySelectionText: {
    '& .selected-text': {
      // fontWeight: 800,
    },
    textAlign: 'center',
    cursor: 'pointer',
  },
});

const formatGraphTicks = (v, axis) => {
  var result;
  if (v > 1000000000) {
    result = (v / 1000000000).toFixed(axis.tickDecimals) + "B R$";
  } else if (v > 1000000) {
    result = (v / 1000000).toFixed(axis.tickDecimals) + "M R$";
  }
  else {
    result = v.toFixed(axis.tickDecimals);
  }

  return result.toLocaleString() + " R$";
}

const SaleHistory = props => {
  const store = CatalogDetailsPage.useContainer();
  const [rap, setRap] = useState(null);
  const [rapChart, setRapChart] = useState(null);
  const [volumeChart, setVolumeChart] = useState(null);
  const s = useSaleHistoryStyles();

  useEffect(() => {
    if (!store.details) return;
    getResaleData({ assetId: store.details.id }).then(resaleData => {
      setRap(resaleData.recentAveragePrice);
      setRapChart(resaleData.priceDataPoints);
      setVolumeChart(resaleData.volumeDataPoints);
      if (store.saleCount === 0) {
        store.setSaleCount(resaleData.sales);
      }
    });
  }, [store.details]);

  useEffect(() => {
    const el = document.getElementById('placeholder');
    if (!rapChart || !volumeChart) {
      return;
    }
    const rapData = rapChart.map(v => {
      return [
        dayjs(v.date).unix() * 1000,
        v.value,
      ]
    });
    const volumeData = volumeChart.map(v => {
      return [
        dayjs(v.date).unix() * 1000,
        v.value,
      ]
    })
    // @ts-ignore
    if (!window.RobloxItemChartLibrary) {
      const saleChartScript = document.createElement('script');
      saleChartScript.setAttribute('src', '/js/itemSaleChart.js?refresh=1');
      saleChartScript.onload = function () {
        // @ts-ignore
        window.RobloxItemChartLibrary.loadChart(rapData, volumeData);
      }
      document.body.appendChild(saleChartScript);
    } else {
      // @ts-ignore
      window.RobloxItemChartLibrary.loadChart(rapData, volumeData);
    }
  }, [rapChart, volumeChart]);

  if (!rapChart) {
    return null
  }


  return <div className={`row ${s.row}`}>
    <div className='col-12 ps-4 pe-4'>
      <p className={s.rapText}>Recent Average Price: <span className={s.rap}>R$ {rap && rap.toLocaleString()}</span></p>
    </div>
    <div className='col-12 mt-4 ps-4'>
      <p className={s.daySelectionText}>
        <span id="days90" className='pe-1'>30 Days</span>
        <span>|</span>
        <span id="days90" className='pe-1'>90 Days</span>
        <span>|</span>
        <span id="days180">180 Days</span>
      </p>
      <div id='placeholder' style={{ width: '370px', height: '300px' }}></div>
      <div id='volumegraph' style={{ width: '370x', height: '60px' }}></div>
    </div>
  </div>
}

export default SaleHistory;