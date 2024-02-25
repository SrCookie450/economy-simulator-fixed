import {useRouter} from "next/dist/client/router";
import ForumSubcategory from "../../components/forumSubcategory";

const ShowForumPage = props => {
  const router = useRouter();
  const id = router.query.ForumID;
  const page = parseInt(router.query.Page, 10);
  if (!id)
    return null;
  return <ForumSubcategory id={id} page={Number.isInteger(page) && page > 0 ? page : 1} />
}

export default ShowForumPage;