import {useEffect, useState} from "react";
import MemberFilters from "./filters";
import {getMembers, getRoles, getRolesetMembers, getUserGroups} from "../../../../../services/groups";
import MembersList from "./membersList";

const Members = props => {
  const [role, setRole] = useState(null);
  const [roles, setRoles] = useState(null);
  const [members, setMembers] = useState(null);
  const [roleFilter, setRoleFilter] = useState(null);
  const [username, setUsername] = useState(null);

  useEffect(() => {
    getRoles({
      groupId: props.groupId,
    }).then(data => {
      setRoles(data);
    });

  }, [props.groupId]);

  useEffect(() => {
    refreshMembers();
  }, [roleFilter]);

  const refreshMembers = (cursor = '') => {
    if (roleFilter) {
      getRolesetMembers({
        groupId: props.groupId,
        roleSetId: roleFilter,
        cursor,
        limit: 12,
        sortOrder: 'desc',
      }).then(d => {
        setMembers({
          ...d,
          data: d.data.map(v => {
            return {
              user: {
                userId: v.userId,
                username: v.username,
              },
              role: {
                id: v.roleId,
              },
            }
          })
        })
      })
    }else{
      getMembers({groupId: props.groupId, cursor, limit: 12, sortOrder: 'desc'}).then(mems => {
        setMembers(mems);
      })
    }
  }

  return <div className='row mt-4'>
    <div className='col-12'>
      <h3>Members</h3>
      <MemberFilters roles={roles} setRoleFilter={setRoleFilter} />
      <MembersList groupId={props.groupId} members={members} roles={roles} refreshMembers={refreshMembers} />
      <p className='text-center mt-4'>
        {
          members && members.previousPageCursor ? <a className='ms-4 me-4' href='#' onClick={e => {
            e.preventDefault();
            refreshMembers(members.previousPageCursor);
          }}>Previous</a> : null
        }

        {
          members && members.nextPageCursor ? <a className='ms-4 me-4' href='#' onClick={e => {
            e.preventDefault();
            refreshMembers(members.nextPageCursor);
          }}>Next</a> : null
        }
      </p>
    </div>
  </div>
}

export default Members;