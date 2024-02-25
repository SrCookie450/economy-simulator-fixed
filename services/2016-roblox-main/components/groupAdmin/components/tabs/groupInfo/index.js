import {useEffect, useRef, useState} from "react";
import GroupIcon from "../../../../groupIcon";
import Description from "./description";
import ActionButton from "../../../../actionButton";
import {setGroupDescription, setGroupIcon} from "../../../../../services/groups";
import groupAdminStore from "../../../stores/groupAdminStore";

const GroupInfo = props => {
  const store = groupAdminStore.useContainer();
  const description = useRef(props.info.description);
  const fileRef = useRef(null);

  return <div className='row mt-4'>
    <div className='col-12'>
      <h3>Group Information</h3>
    </div>
    <div className='col-4'>
      <GroupIcon id={props.groupId} />
      <div className='mt-4'>
        <input type='file' ref={fileRef} />
      </div>
    </div>
    <div className='col-8'>
      <Description setDescription={(v) => {
        description.current = v;
      }} description={description} />
    </div>
    <div className='col-12 mt-4'>
      <ActionButton label='Save' onClick={() => {
        let promises = [];
        if (fileRef.current && fileRef.current.files && fileRef.current.files.length) {
          promises.push(setGroupIcon({
            groupId: props.groupId,
            icon: fileRef.current.files[0],
          }));
        }
        if (description.current !== props.info.description) {
          promises.push(setGroupDescription({
            groupId: props.groupId,
            description: description.current,
          }));
        }

        Promise.all(promises).then(() => {
          window.location.reload();
        }).catch(e => {
          store.setFeedback('Error saving changes: ' + e.message);
        })
      }} />
    </div>
  </div>
}

export default GroupInfo;