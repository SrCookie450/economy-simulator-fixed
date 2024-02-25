import {useRouter} from "next/dist/client/router";
import ForumThread from "../../components/forumThread";
import {getRepliesToThread, markAsRead} from "../../services/forums";
const limit = 15;

const ShowPostPage = props => {
  const router = useRouter();
  const id = router.query.PostID;
  let page = parseInt(router.query.Page, 10);
  if (page < 1 || isNaN(page) || !Number.isInteger(page)) {
    page = 1;
  }
  if (!id)
    return null;
  return <ForumThread id={id} page={page} posts={props.posts} />
}

ShowPostPage.getInitialProps = async (ctx) => {
  let id = parseInt(ctx.query.PostID, 10);
  let pageNumber = parseInt(ctx.query.Page, 10);
  if (!Number.isInteger(pageNumber) || pageNumber < 1) {
    pageNumber = 1;
  }
  if (!Number.isInteger(id) || id < 1) {
    id = 1;
  }

  const replies = await getRepliesToThread({
    threadId: id,
    limit: limit,
    cursor: (pageNumber*limit-limit).toString(),
  });

  return {
    posts: replies,
  }
}

export default ShowPostPage;