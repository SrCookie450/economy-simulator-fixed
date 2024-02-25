import {useState} from "react";
import Mode from "./mode";
import Summary from "./summary";
import LineItem from "./lineItem";

const GroupRevenue = props => {
  const [mode, setMode] = useState('Summary');

  return <div className='row'>
    <div className='col-12'>
      <Mode mode={mode} setMode={setMode} />
      {
        mode === 'Summary' ? <Summary {...props} /> : null
      }
      {
        mode === 'Line Item' ? <LineItem {...props} /> : null
      }
    </div>
  </div>
}

export default GroupRevenue;