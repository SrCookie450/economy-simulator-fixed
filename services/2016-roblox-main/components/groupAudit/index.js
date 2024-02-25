import {useEffect, useState} from "react";
import {getGroupAuditLog, getGroupInfo, getGroupSettings} from "../../services/groups";
import Table from "../myMoney/components/table";
import dayjs from "dayjs";
import PlayerHeadshot from "../playerHeadshot";
import Link from "../link";
import {useRouter} from "next/dist/client/router";

const Description = ({actor, description, actionType}) => {
  if (actionType === 'Spend Group Funds') {
    return <p className='mb-0'>
      One-time payout of {description.Amount} {description.CurrencyTypeName} to <Link href={`/users/${actor.user.userId}/profile`}>
      <a>
        {actor.user.username}
      </a>
    </Link>.
    </p>
    return description.ItemDescription;
  }
  if (actionType === 'Update Roleset Data') {
    return 'Configured ' + description.RoleSetName;
  }
  if (actionType === 'Update Roleset Rank') {
    return 'Changed ' + description.RoleSetName + ' rank from ' + description.OldRank + ' to ' + description.NewRank + '.';
  }
  if (actionType === 'Change Rank') {
    return <p className='mb-0'>
      Changed the rank of <Link href={`/users/${description.TargetId}/profile`}>
      <a>
        {description.TargetName}
      </a>
    </Link> from {description.OldRoleSetName} to {description.NewRoleSetName}.
    </p>
  }
  if (actionType === 'Change Owner') {
    return <p className='mb-0'>
      Changed the group owner to <Link href={`/users/${description.NewOwnerId}/profile`}>
      <a>
        {description.NewOwnerName}
      </a>
    </Link>
    </p>
  }
  if (actionType === 'Delete Post') {
    return <p className='mb-0'>
      Deleted post "{description.PostDesc}" by <Link href={`/users/${description.TargetId}/profile`}>
        <a>
          {description.TargetName}
        </a>
      </Link>
    </p>
  }

  if (actionType === 'Lock')
    return 'Locked the group.';
  if (actionType === 'Unlock')
    return 'Unlocked the group.'

  return <p className='mb-0'>Unknown action {actionType}.</p>
}

const GroupAudit = props => {
  const router = useRouter();
  const {groupId, userId, action} = props;
  const [groupInfo, setGroupInfo] = useState(null);
  const [entries, setEntries] = useState(null);

  useEffect(() => {
    getGroupInfo({groupId: groupId}).then(d => {
      setGroupInfo(d);
    });
    loadAuditLog('');
  }, [groupId, userId]);

  const loadAuditLog = (cursor) => {
    getGroupAuditLog({
      groupId,
      cursor: cursor,
      userId: '',
      action: '',
    }).then(d => {
      setEntries(d);
    })
  }

  if (!groupInfo) return null;
  return <div className='container card br-none'>
    <div className='row'>
      <div className='col-6'>
        <h3>{groupInfo.name}</h3>
      </div>
    </div>
    <div className='row'>
      <div className='col-12'>
        <Table keys={
          [
            'Date',
            'User',
            'Rank',
            'Description',
          ]
        } ad={true} entries={entries ? entries.data.map(v => {
          const d = dayjs(v.created);
          return [
            <div style={{minWidth: '60px'}}>
              {d.format('M/D/YY')}
              <br />
              {d.format('HH:mm')}
            </div>,
            <div style={{minWidth: '120px'}}>
              <div style={{width: '20px', borderRadius: '50%'}} className='d-inline-block overflow-hidden'>
                <PlayerHeadshot name={v.actor.user.username} id={v.actor.user.userId} />
              </div>
              <div className='ms-1 d-inline-block' style={{position: 'relative', top: '-5px', width: 'calc(100% - 24px)'}}>
                <Link href={`/users/${v.actor.id}/profile`}>
                  <a>
                    {v.actor.user.username}
                  </a>
                </Link>
              </div>
            </div>,
            v.actor.role.name,
            <Description {...v} />,
          ];
        }) : undefined} />
      </div>
      <div className='col-12'>
        <p className='text-center mb-2'>
          {entries && entries.previousPageCursor ? <span>
            <a href='#' onClick={e => {
              e.preventDefault();
              loadAuditLog(entries.previousPageCursor);
            }}>Previous</a>
          </span> : null}

          {entries && entries.nextPageCursor ? <span>
            <a href='#' onClick={e => {
              e.preventDefault();
              loadAuditLog(entries.nextPageCursor);
            }}>Next</a>
          </span> : null}
        </p>
      </div>
    </div>
  </div>
}

export default GroupAudit;