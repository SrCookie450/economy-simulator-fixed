import {useRouter} from "next/dist/client/router";
import ConfigureItem from "../../components/configureItem";

const MyItemPage = props => {
  const router = useRouter();
  const {id} = router.query;
  return <ConfigureItem assetId={parseInt(id,10)} />
}

export default MyItemPage;