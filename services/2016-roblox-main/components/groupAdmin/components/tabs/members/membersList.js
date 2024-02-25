import PlayerHeadshot from "../../../../playerHeadshot";
import Link from "../../../../link";
import styles from './membersList.module.css';
import authentication from "../../../../../stores/authentication";
import GearDropdown from "../../../../gearDropdown";
import {leaveGroup, setUserRole} from "../../../../../services/groups";

const MembersList = props => {
  const auth = authentication.useContainer();
  if (!props.roles) return null;

  return <div className='row'>
    <div className='col-12 col-lg-12'>
      <div className='row'>
        {
          props.members ? (
            props.members.data.length ? props.members.data.map(v => {
              const isMe = auth.userId === v.user.userId;
              return <div className='col-2' key={v.user.userId}>
                <div className={styles.gearDropdown}>
                  <div className={styles.gear}>
                    {
                      !isMe ? <GearDropdown options={[
                        {
                          name: 'Kick User',
                          onClick: () => {
                            leaveGroup({
                              groupId: props.groupId,
                              userId: v.user.userId,
                            }).then(d => {
                              props.refreshMembers();
                            })
                          },
                        }]}></GearDropdown> : null
                    }
                  </div>
                </div>
                <div className={styles.headshot}>
                  <PlayerHeadshot id={v.user.userId} name={v.user.username} />
                </div>
                <p className='mb-0 text-truncate'>
                  <Link href={`/users/${v.user.userId}/profile`}>
                    <a>{v.user.username}</a>
                  </Link>
                </p>
                {!isMe ? <select className='w-100 mt-2' defaultValue={v.role.id.toString()} onChange={e => {
                  setUserRole({
                    userId: v.user.userId,
                    roleId: parseInt(e.currentTarget.value, 10),
                    groupId: props.groupId,
                  }).then(() => {

                  }).catch(e => {
                    // todo: something

                  })
                }}>
                  {
                    props.roles.filter(a => a.rank !== 0).map(v => {
                      return <option value={v.id} key={v.id}>{v.name}</option>
                    })
                  }
                </select> : null}
              </div>
            }) : <div className='col-12'> <p className='text-center mt-4 mb-4'>No results</p> </div>
          ) : null
        }
      </div>
    </div>
  </div>
}

export default MembersList;