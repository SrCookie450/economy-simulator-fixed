import {useEffect, useRef, useState} from "react";
import {deleteRole, editRole, getRoles} from "../../../../../services/groups";
import Table from "../../../../myMoney/components/table";
import btn from '../../../../../styles/buttons.module.css';
import ActionButton from "../../../../actionButton";
import CreateRoleModal from "./createModal";
import groupAdminStore from "../../../stores/groupAdminStore";
import PermissionsModal from "./permissionsModal";

const modals = {
  CREATE_ROLE: 'CREATE_ROLE',
  EDIT_ROLE_PERMISSIONS: 'EDIT_ROLE_PERMISSIONS',
}

const GroupRoles = props => {
  const [roles, setRoles] = useState(null);
  const [modal, setModal] = useState(null);
  const [role, setRole] = useState(null);
  const editedRoles = useRef({});
  const store = groupAdminStore.useContainer();

  useEffect(() => {
    getRoles({
      groupId: props.groupId,
    }).then(data => {
      setRoles(data);
    });
  }, [props.groupId]);
  if (!roles) return null;

  return <div className='row'>
    {
      modal === modals.CREATE_ROLE ? <CreateRoleModal setModal={setModal} groupId={props.groupId} /> : null
    }
    {
      modal === modals.EDIT_ROLE_PERMISSIONS ? <PermissionsModal setModal={setModal} groupId={props.groupId} role={role} /> : null
    }
    <div className='col-12'>
      <h3>Roles</h3>
    </div>
    <Table keys={['Name', 'Description', 'Rank (0-255)', <button className={btn.legacyButton} onClick={() => {
      // ?
    }}>View All Permissions</button> ]} entries={roles.sort((a,b) => {
      return a.rank > b.rank ? -1 : 1;
    }).map(v => {
      return [
        <input disabled={v.rank === 0} type='text' value={v.name} style={{width: '80px'}} onChange={e => {
          v.name = e.currentTarget.value;
          editedRoles.current[v.id] = true;
          setRoles([...roles]);
        }} />,
        <input disabled={v.rank === 0} type='text' value={v.description} className='w-100' onChange={e => {
          v.description = e.currentTarget.value;
          editedRoles.current[v.id] = true;
          setRoles([...roles]);
        }} />,
        <input disabled={v.rank === 0 || v.rank === 255} type='text' value={v.rank} style={{width: '40px'}} onChange={e => {
          v.rank = parseInt(e.currentTarget.value, 10);
          if (v.rank === 255 || v.rank === 0)
            v.rank = 254;
          editedRoles.current[v.id] = true;
          setRoles([...roles]);
        }} />,
        v.rank === 255 ? null : <div>
          <div className='d-inline-block'>
            <button className={btn.legacyButton} onClick={() => {
              setRole(v);
              setModal(modals.EDIT_ROLE_PERMISSIONS);
            }}>Permissions</button>
          </div>
          {
            v.rank !== 0 ? <div className='ms-2 d-inline-block'>
            <span className={btn.delete} onClick={() => {
              // Delete role
              deleteRole({groupId: props.groupId, roleId: v.id}).then(() => {
                window.location.reload();
              }).catch(e => {
                store.setFeedback('Error deleting role: ' + e.message);
              })
            }}>X</span>
            </div> : null
          }
        </div>
      ]
    })} />
    <div className='col-12'>
      <div className='col-6'>
        <ActionButton label='Create New' onClick={() => {
        setModal(modals.CREATE_ROLE);
        }} />
      </div>
      <div className='col-6'>
        <ActionButton label='Save' onClick={() => {
          let promises = [];
          for (const roleIdStr in editedRoles.current) {
            const roleId = parseInt(roleIdStr, 10);
            let d = roles.find(a => a.id === roleId);
            promises.push(editRole({
              groupId: props.groupId,
              roleId: roleId,
              name: d.name,
              description: d.description,
              rank: d.rank,
            }));
          }
          Promise.all(promises).then(() => {
            window.location.reload();
          }).catch(e => {
            store.setFeedback(e.message);
          })
        }} />
      </div>
    </div>

    <div className='col-12'>
      <p className='fst-italic mt-4'>Use RoleSets to grant status and administrative permissions to valued group members.</p>
    </div>
  </div>
}

export default GroupRoles;