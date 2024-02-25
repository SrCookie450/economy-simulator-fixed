import React, {useEffect, useState} from "react";
import AuthenticationStore from "../../../../../stores/authentication";
import {
  formatSummaryResponse,
  getGroupTransactionSummary,
  getTransactionSummary
} from "../../../../../services/economy";
import Robux from "../../../../catalogDetailsPage/components/robux";
import Table from "../../../../myMoney/components/table";

const Summary = props => {
  const [period, setPeriod] = useState('day');
  const [entries, setEntries] = useState(null);
  const [response, setResponse] = useState(null);
  useEffect(() => {
    getGroupTransactionSummary({
      timePeriod: period,
      groupId: props.groupId,
    }).then(values => {
      setEntries(formatSummaryResponse(values, 'Group'));
      setResponse(values);
    })
  }, [period]);

  return <div className='row'>
    <div className='col-12 mb-3 mt-3'>
      <div className='d-inline-block'>
        <p className='mb-0 fw-700 lighten-1 pe-2'>Time Period: </p>
      </div>
      <div className='d-inline-block'>
        <select value={period} onChange={(e) => {
          setPeriod(e.currentTarget.value);
          setEntries(null);
        }}>
          <option value='day'>Past Day</option>
          <option value='week'>Past Week</option>
          <option value='month'>Past Month</option>
          <option value='year'>Past Year</option>
        </select>
      </div>
    </div>
    <div className='col-12'>
      <p className='mb-0 fw-600'><Robux inline={true}></Robux> Robux</p>
    </div>
    <div className='col-12'>
      <Table
        keys={
          [
            'Categories',
            'Credit',
          ]
        }
        entries={entries}>
        <p className={'text-end fw-600 mt-2'}>TOTAL: <Robux inline={true}>{response && response.incomingRobuxTotal.toLocaleString()}</Robux></p>
      </Table>
    </div>
  </div>
}

export default Summary;