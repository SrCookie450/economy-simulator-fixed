import { createUseStyles } from "react-jss";
import { leaveGroup, removePrimaryGroup, setGroupAsPrimary } from "../../../services/groups";
import AuthenticationStore from "../../../stores/authentication";
import GroupPageStore from "../stores/groupPageStore";
import Button from "./button";
import OldCard from "./oldCard";

const useStyles = createUseStyles({
  button: {
    width: '100%',
    marginTop: '6px',
  }
})

const GroupControls = props => {
  const store = GroupPageStore.useContainer();
  const auth = AuthenticationStore.useContainer();

  const s = useStyles();

  if (!store.permissions || !store.info || !store.rank || store.rank.rank === 0) return null;

  const isAdmin = store.permissions['changeRank'] || store.permissions['manageClan'] || store.permissions['manageRelationships'] || store.permissions['removeMembers'] || store.permissions['spendGroupFunds'] || store.permissions['viewGroupPayouts'];
  const canViewAuditLog = store.permissions['viewAuditLogs'];
  const canAdvertise = store.permissions['advertiseGroup'];
  const isPrimary = store.primary && store.primary.group.id === store.groupId;

  return <OldCard>
    <div className='pe-2 ps-2 pb-2'>
      <p className='fw-700 font-size-18 mb-2 lighten-2'>Controls</p>
      {isAdmin && <Button href={`/My/GroupAdmin.aspx?gid=${store.groupId}`} className={s.button}>Group Admin</Button>}
      {canAdvertise && <Button href={`/My/CreateUserAd.aspx?targetId=${store.groupId}&targetType=group`} className={s.button}>Advertise Group</Button>}
      <Button href='#' className={s.button} onClick={() => {
        if (isPrimary) {
          removePrimaryGroup().then(() => {
            window.location.reload();
          })
        } else {
          setGroupAsPrimary({
            groupId: store.groupId,
          }).then(() => {
            window.location.reload();
          })
        }
      }}>{isPrimary ? 'Remove' : 'Set'} Primary</Button>
      <Button className={s.button} onClick={(e) => {
        // 
        leaveGroup({
          groupId: store.groupId,
          userId: auth.userId,
        }).then(() => {
          window.location.reload();
        })
      }}>Leave Group</Button>
      {canViewAuditLog && <Button href={'/Groups/Audit.aspx?groupid=' + store.groupId} className={s.button}>Audit Log</Button>}
    </div>
  </OldCard>
}

export default GroupControls;