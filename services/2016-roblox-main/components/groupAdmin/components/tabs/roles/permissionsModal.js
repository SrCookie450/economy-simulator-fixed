import useButtonStyles from "../../../../../styles/buttonStyles";
import {useModalStyles} from "../../../../catalogDetailsPage/components/buyItemModal";
import authentication from "../../../../../stores/authentication";
import {useEffect, useState} from "react";
import Robux from "../../../../catalogDetailsPage/components/robux";
import ActionButton from "../../../../actionButton";
import {createRole, getPermissionsForRoleset, setRolePermissions} from "../../../../../services/groups";
import Permissions from "./permissions";

const PermissionsModal = props => {
  const btnStyles = useButtonStyles();
  const s = useModalStyles();
  const [locked, setLocked] = useState(false);
  const [feedback, setFeedback] = useState(null);
  const [tab, setTab] = useState(null);
  const [permissions, setPermissions] = useState(null);

  useEffect(() => {
    getPermissionsForRoleset({
      groupId: props.groupId,
      rolesetId: props.role.id,
    }).then(d => {
      setPermissions(d.permissions);
    })
  }, [props.groupId]);

  const height = (() => {
    if (tab === 'Assets' || tab === 'Posts') return '297px';
    if (tab === 'Members') return '265px';

    return 230;
  })();

  if (permissions === null) return null;

  return <div className={s.modalBg}>
    <div className={s.modalWrapper}>
      <h3 className={s.title}>
        Change Permissions For {props.role.name}
      </h3>
      <div className={s.innerSection} style={{height: height}}>
        <div className='row'>
          <div className='col-3'>

          </div>
          <div className='col-9'>

          </div>
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            {
              feedback ? <p className='text-danger fw-bold text-center'>{feedback}</p> : null
            }

            <Permissions tab='Posts' openTab={tab} setOpenTab={setTab} options={[
              {
                name: 'View group wall',
                get: () => permissions.groupPostsPermissions.viewWall,
                set: (v) => {
                  permissions.groupPostsPermissions.viewWall = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'View group status',
                get: () => permissions.groupPostsPermissions.viewStatus,
                set: (v) => {
                  permissions.groupPostsPermissions.viewStatus = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Post on group wall',
                get: () => permissions.groupPostsPermissions.postToWall,
                set: (v) => {
                  permissions.groupPostsPermissions.postToWall = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Post on group status',
                get: () => permissions.groupPostsPermissions.postToStatus,
                set: (v) => {
                  permissions.groupPostsPermissions.postToStatus = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Delete group wall posts',
                get: () => permissions.groupPostsPermissions.deleteFromWall,
                set: (v) => {
                  permissions.groupPostsPermissions.deleteFromWall = v;
                  setPermissions({...permissions});
                }
              },
            ]} />

            <Permissions tab='Members' openTab={tab} setOpenTab={setTab} options={[
              {
                name: 'Kick lower-ranked members',
                get: () => permissions.groupMembershipPermissions.removeMembers,
                set: (v) => {
                  permissions.groupMembershipPermissions.removeMembers = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Manage lower-ranked member ranks',
                get: () => permissions.groupMembershipPermissions.changeRank,
                set: (v) => {
                  permissions.groupMembershipPermissions.changeRank = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Manage alies and enemies',
                get: () => permissions.groupManagementPermissions.manageRelationships,
                set: (v) => {
                  permissions.groupManagementPermissions.manageRelationships = v;
                  setPermissions({...permissions});
                }
              },
            ]} />

            <Permissions tab='Assets' openTab={tab} setOpenTab={setTab} options={[
              {
                name: 'Create group items',
                get: () => permissions.groupEconomyPermissions.createItems,
                set: (v) => {
                  permissions.groupEconomyPermissions.createItems = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Configure group items',
                get: () => permissions.groupEconomyPermissions.manageItems,
                set: (v) => {
                  permissions.groupEconomyPermissions.manageItems = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Advertise the group',
                get: () => permissions.groupEconomyPermissions.advertiseGroup,
                set: (v) => {
                  permissions.groupEconomyPermissions.advertiseGroup = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Spend group funds',
                get: () => permissions.groupEconomyPermissions.spendGroupFunds,
                set: (v) => {
                  permissions.groupEconomyPermissions.spendGroupFunds = v;
                  setPermissions({...permissions});
                }
              },
              {
                name: 'Create and edit group games',
                get: () => permissions.groupEconomyPermissions.addGroupPlaces && permissions.groupEconomyPermissions.manageGroupGames,
                set: (v) => {
                  permissions.groupEconomyPermissions.addGroupPlaces = v;
                  permissions.groupEconomyPermissions.manageGroupGames = v;
                  setPermissions({...permissions});
                }
              },
            ]} />


            <Permissions tab='Miscellaneous' openTab={tab} setOpenTab={setTab} options={[
              {
                name: 'View audit log',
                get: () => permissions.groupManagementPermissions.viewAuditLogs,
                set: (v) => {
                  permissions.groupManagementPermissions.viewAuditLogs = v;
                  setPermissions({...permissions});
                }
              },

            ]} />

          </div>


          <div className='col-8 offset-2'>
            <div className='row'>
              <div className='col-6'>
                <ActionButton disabled={locked} label={'Save'} className={btnStyles.buyButton} onClick={(e) => {
                  e.preventDefault();
                  setLocked(true);
                  setRolePermissions(props.groupId, props.role.id, {
                    DeleteFromWall: permissions.groupPostsPermissions.deleteFromWall,
                    PostToWall: permissions.groupPostsPermissions.postToWall,
                    InviteMembers: permissions.groupMembershipPermissions.inviteMembers,
                    PostToStatus: permissions.groupPostsPermissions.postToStatus,
                    RemoveMembers: permissions.groupMembershipPermissions.removeMembers,
                    ViewStatus: permissions.groupPostsPermissions.viewStatus,
                    ViewWall: permissions.groupPostsPermissions.viewWall,
                    ChangeRank: permissions.groupMembershipPermissions.changeRank,
                    AdvertiseGroup: permissions.groupEconomyPermissions.advertiseGroup,
                    ManageRelationships: permissions.groupManagementPermissions.manageRelationships,
                    AddGroupPlaces: permissions.groupEconomyPermissions.addGroupPlaces,
                    ViewAuditLogs: permissions.groupManagementPermissions.viewAuditLogs,
                    CreateItems: permissions.groupEconomyPermissions.createItems,
                    ManageItems: permissions.groupEconomyPermissions.manageItems,
                    SpendGroupFunds: permissions.groupEconomyPermissions.spendGroupFunds,
                    ManageClan: false,
                    ManageGroupGames: permissions.groupEconomyPermissions.manageGroupGames,
                    UseCloudAuthentication: false,
                    AdministerCloudAuthentication: false,
                    ViewAnalytics: false,
                  }).then(() => {
                    window.location.reload();
                  }).catch(e => {
                    setFeedback(e.message);
                  })
                }}/>
              </div>
              <div className='col-6'>
                <ActionButton disabled={locked} label='Cancel' className={btnStyles.cancelButton} onClick={(e) => {
                  e.preventDefault();
                  props.setModal(null);
                }}/>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
}

export default PermissionsModal;