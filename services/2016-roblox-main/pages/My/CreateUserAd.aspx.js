import { useRouter } from "next/router";
import CreateUserAd from "../../components/createUserAd";

const CreateUserAdPage = props => {
  const router = useRouter();
  const { targetId, targetType } = router.query;
  return <CreateUserAd targetId={targetId} targetType={targetType}></CreateUserAd>
}

export default CreateUserAdPage;