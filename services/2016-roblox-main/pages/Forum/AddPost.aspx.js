import {useRouter} from "next/dist/client/router";
import ForumSubcategory from "../../components/forumSubcategory";
import ForumPostReply from "../../components/forumPostReply";
import ForumThreadCreate from "../../components/forumThreadCreate";

const AddPostPage = props => {
  const router = useRouter();
  const id = router.query.PostID;
  const subId = router.query.ForumID;
  if (subId)
    return <ForumThreadCreate id={subId} />
  if (id)
    return <ForumPostReply id={id} />

  return null;
}

export default AddPostPage;