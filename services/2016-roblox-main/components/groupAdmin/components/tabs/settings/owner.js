import {useState} from "react";
import btnStyles from '../../../../../styles/buttons.module.css';
import {getPreviousUsernames, getUserIdByUsername} from "../../../../../services/users";
import {changeGroupOwner} from "../../../../../services/groups";
import groupAdminStore from "../../../stores/groupAdminStore";

const Owner = props => {
  const store=  groupAdminStore.useContainer();
  const [name, setName] = useState('');

  return <div className='col-12 mt-4'>
    <h3>Change Group Owner</h3>
    <div className='d-inline-block me-1'>Username:</div>
    <div className='d-inline-block me-1'>
      <input type='text' value={name} onChange={e => {
        setName(e.currentTarget.value);
      }} />
    </div>
    <div className='d-inline-block'>
      <button className={btnStyles.legacyButton} onClick={() => {
        getUserIdByUsername(name).then(id => {
          changeGroupOwner({
            groupId: store.groupId,
            userId: id,
          }).then(() => {
            window.location.href = `/My/Groups.aspx?gid=${store.groupId}`;
          }).catch(e => {
            store.setFeedback('Error changing owner: ' + e.message);
          })
        }).catch(e => {
          store.setFeedback('Error getting user: ' + e.message);
        })
      }}>Make Owner</button>
    </div>
  </div>
}

export default Owner;