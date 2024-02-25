import SearchUsers from "../../components/searchUsers";
import {useRouter} from "next/dist/client/router";

const SearchUsersPage = props => {
  const router = useRouter();
  return <SearchUsers keyword={router.query.keyword} />
}

export default SearchUsersPage;