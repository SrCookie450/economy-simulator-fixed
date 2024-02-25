import {getQueryParams} from "../../lib/getQueryParams";
import GroupAudit from "../../components/groupAudit";

const GroupAuditPage = props => {
  const query = getQueryParams();
  return <GroupAudit groupId={query.groupid} userId={query.userid} action={query.action} />
}

export default GroupAuditPage;