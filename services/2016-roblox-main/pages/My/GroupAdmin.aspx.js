import {useRouter} from "next/dist/client/router";
import GroupAdmin from "../../components/groupAdmin";

const MyGroupAdmin = props => {
  const router = useRouter();
  if (!router.query.gid) return null;
  return <GroupAdmin groupId={parseInt(router.query.gid, 10)} />
}

export default MyGroupAdmin;