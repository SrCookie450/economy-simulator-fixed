import {useRouter} from "next/dist/client/router";
import UpdatePlace from "../../../components/updatePlace";

const PlaceUpdatePage = props => {
  const router = useRouter();
  const {placeId} = router.query;
  if (!placeId)
    return null; // why is this undefined?
  return <UpdatePlace placeId={placeId} />
}

export default PlaceUpdatePage;