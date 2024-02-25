import React from "react";
import { createUseStyles } from "react-jss";
import AdSkyscraper from "../../ad/adSkyscraper";

const useStyles = createUseStyles({
  tradeTypeActions: {
    display: 'inline-block',
  },
  table: {
    width: '100%',
  },
  tableHead: {
    borderTop: '1px solid #b9b9b9',
    borderBottom: '2px solid #e3e3e3',
    background: '#f1f1f1',
  },
  tableHeadLabel: {
    paddingTop: '5px',
    paddingBottom: '5px',
    paddingLeft: '5px',
    color: '#5d5a5b',
  },
  tableHeadBorder: {
    borderLeft: '2px solid #e1e1e1',
  },
  tableAction: {
    width: '60px',
  },
  feedbackWrapper: {
    border: '2px solid #839ec3',
    background: '#e6eefa',
    padding: '8px',
    marginBottom: '10px',
  },
  row: {},
  td: {
    paddingTop: '6px',
    paddingBottom: '4px',
    paddingLeft: '5px',
    borderBottom: '1px solid #e3e3e3',
  },
});


/**
 * Money page table
 * @param {{keys: (string | JSX.Element)[]; entries: (JSX.Element | string)[][]; children?: JSX.Element; ad: boolean}} props
 */
const Table = props => {
  const s = useStyles();
  return <div className='row'>
    <div className={props.ad === false ? 'col-12' : 'col-12 col-lg-10'}>
      <div className='row'>
        <div className='col-12'>
          <table className={s.table}>
            <thead className={s.tableHead}>
              <tr>
                {
                  props.keys.map((v, i) => {
                    const additionalClass = i !== 0 ? ' ' + s.tableHeadBorder : '';
                    return <th key={i} className={s.tableHeadLabel + additionalClass}>{v}</th>
                  })
                }
              </tr>
            </thead>
            <tbody>
              {props.entries && props.entries.map((values, i) => {
                return <tr className={s.row} key={i}>
                  {values.map((v, i) => {
                    return <td className={s.td} key={i}>
                      {v}
                    </td>
                  })}
                </tr>
              })}
            </tbody>
          </table>
          {
            props.entries && props.entries.length === 0 ? <p className='mt-4 text-center'>No entries available</p> : null
          }
          {props.children}
        </div>
      </div>
    </div>
    {props.ad !== false ? <div className='d-none d-lg-flex col-2'>
      <AdSkyscraper context='MyTransactionsTable'/>
    </div> : null}
  </div>
}

export default Table;