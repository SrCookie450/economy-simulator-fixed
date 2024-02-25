import {useEffect, useState} from "react";
import {getGroupSettings, setGroupSettings} from "../../../../../services/groups";
import ActionButton from "../../../../actionButton";
import useButtonStyles from "../../../../../styles/buttonStyles";
import groupAdminStore from "../../../stores/groupAdminStore";
import Owner from "./owner";

const GroupSettings = props => {
  const store = groupAdminStore.useContainer();
  const btn = useButtonStyles();

  const [isManualApproval, setIsManualApproval] = useState(false);
  const [bcRequired, setBcRequired]   = useState(false);

  const [enemiesAllowed, setEnemiesAllowed]  = useState(false);
  const [groupFundsPublic ,setGroupFundsPublic] = useState(false);
  const [groupGamesVisible, setGroupGamesVisible] = useState(false);

  useEffect(() => {
    getGroupSettings({groupId: props.groupId}).then(d => {
      setIsManualApproval(d.isApprovalRequired);
      setEnemiesAllowed(d.areEnemiesAllowed);
      setGroupFundsPublic(d.areGroupFundsVisible);
      setGroupGamesVisible(d.areGroupGamesVisible);
      setBcRequired(d.isBuildersClubRequired);
    })
  }, [props.groupId]);

  return <div className='row mt-4'>
    <div className='col-12'>
      <h3>Settings</h3>
    </div>
    <div className='col-12'>
      <p className='fw-bold mb-1'>Require Approval</p>
      <div className='w-100 mb-1'>
        <input type='radio' checked={!isManualApproval} onChange={() => {
          setIsManualApproval(false);
        }} /> <span>Anyone can join</span>
      </div>
      <div className='w-100'>
        <input type='radio' checked={isManualApproval} onChange={() => {
          setIsManualApproval(true);
        }} /> <span>Manual approval</span>
      </div>
    </div>

    <div className='col-12 mt-4'>
      <p className='fw-bold mb-1'>Entry Qualifications</p>
      <input type='checkbox' checked={bcRequired} onChange={v => {
        setBcRequired(v.currentTarget.checked);
      }} /> <span>User must have Builders Club.</span>
    </div>

    <div className='col-12 mt-4'>
      <p className='fw-bold mb-1'>Miscellaneous</p>
      <div className='w-100'>
        <input type='checkbox' checked={enemiesAllowed} onChange={v => {
          setEnemiesAllowed(v.currentTarget.checked);
        }} /> <span>Allow enemy declarations</span>
      </div>
      <div className='w-100'>
        <input type='checkbox' checked={groupFundsPublic} onChange={v => {
          setGroupFundsPublic(v.currentTarget.checked);
        }} /> <span>Group funds are publicly visible</span>
      </div>
      <div className='w-100'>
        <input type='checkbox' checked={groupGamesVisible} onChange={v => {
          setGroupGamesVisible(v.currentTarget.checked);
        }} /> <span>Group games are visible on the group home page</span>
      </div>
    </div>

    <div className='col-12 mt-4'>
      <div>
        <ActionButton label='Save' className={btn.buyButton + ' float-left w-auto'} onClick={() => {
          setGroupSettings({
            groupId: props.groupId,
            isApprovalRequired: isManualApproval,
            areEnemiesAllowed: enemiesAllowed,
            areGroupFundsVisible: groupFundsPublic,
            areGroupGamesVisible: groupGamesVisible,
          }).then(() => {
            window.location.reload();
          }).catch(e => {
            store.setFeedback('Error updating settings: ' + e.message);
          })
        }}/>
      </div>
    </div>

    <Owner />
  </div>
}

export default GroupSettings;