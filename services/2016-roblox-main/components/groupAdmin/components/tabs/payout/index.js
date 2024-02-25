import {useState} from "react";
import btnStyles from '../../../../../styles/buttons.module.css';
import {getUserIdByUsername} from "../../../../../services/users";
import {oneTimePayout} from "../../../../../services/groups";
import groupAdminStore from "../../../stores/groupAdminStore";

const Payout = props => {
  const store = groupAdminStore.useContainer();
  const [name, setName] = useState('');
  const [amount, setAmount] = useState('');

  return <div className='col-12 mt-4'>
    <h3>One-Time Payout</h3>
    <div className='row'>
      <div className='col-12 col-lg-6'>
        <div className='row'>
          <div className='col-3 pe-0'>
            <p className='text-end fw-bold'>Username:</p>
          </div>
          <div className='col-9 ps-1'>
            <input type='text' value={name} onChange={e => {
              setName(e.currentTarget.value);
            }} />
          </div>
        </div>


        <div className='row'>
          <div className='col-3 pe-0'>
            <p className='text-end fw-bold'>Amount:</p>
          </div>
          <div className='col-9 ps-1'>
            <input type='text' value={amount} onChange={e => {
              setAmount(e.currentTarget.value);
            }} />
          </div>
        </div>


        <button className={btnStyles.legacyButton} onClick={() => {
          getUserIdByUsername(name).then(id => {
            oneTimePayout({
              groupId: props.groupId,
              userId: id,
              amount: parseInt(amount, 10),
            }).then(() => {
              window.location.reload();
            }).catch(e => {
              store.setFeedback(e.message);
            })
          }).catch(e => {
            store.setFeedback('Error getting user: ' + e.message);
          })
        }}>Pay</button>

      </div>
    </div>

  </div>
}

export default Payout;