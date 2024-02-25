import { useRouter } from "next/router";
import UserInventory from "../../../components/userInventory";

const UserFavoritesPage = props => {
  const router = useRouter();
  const userId = router.query['userId'];
  return <UserInventory userId={userId} mode='Favorites'/>
}

export default UserFavoritesPage;