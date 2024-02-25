import useForumStyles from "../forumHome/forumStyles";
import ForumContainer from "../forumContainer";
import Link from "../link";
import {useEffect, useState} from "react";
import authentication from "../../stores/authentication";
import {getPostsByUser} from "../../services/forums";
import ForumHeader from "../forumHeader";

const MyForums = props => {
  const s = useForumStyles();
  const [posts, setPosts] = useState(null);
  const [offset, setOffset] = useState(0);
  const limit = 15;
  const auth = authentication.useContainer();
  useEffect(() => {
    if (auth.isAuthenticated && auth.userId) {
      getPostsByUser({
        offset,
        limit,
        userId: auth.userId
      }).then(d => {
        setPosts(d);
      })
    }
  }, [auth, offset]);
  const pageNumber =(offset / limit) + 1;
  const nextPageAvailable = posts && posts.data.length >= limit;

  return <ForumContainer>
    <ForumHeader />
    <table className='w-100'>
      <thead className={s.header}>
      <tr className={s.headerRow}>
        <th>Subject</th>
        <th>Post</th>
      </tr>
      </thead>
      <tbody>
      {
        posts === null ? <tr>
          <td>
            <div className='min-vh-100' />
          </td>
          <td />
        </tr> : null
      }
      {
        posts ? posts.data.map(v => {
          return <tr className={s.bodyRow}>
            <td>
              <Link href={`/Forum/ShowPost.aspx?PostID=${v.threadId || v.postId}`}>
                <a>
                  {v.title ? v.title : 'Reply to ' + v.threadId}
                </a>
              </Link>
            </td>
            <td>{v.post}</td>
          </tr>
        }) : null
      }
      </tbody>
    </table>

    <div className='row'>
      <div className='col-6'>
        <p className='fw-bold'>Page {pageNumber}</p>
      </div>
      <div className='col-6'>
        <p className='mb-0 text-end'>
          {pageNumber > 1 ? <span className='cursor-pointer' onClick={() => {
            setOffset(offset-limit);
          }}>Previous</span> : null}
          <span className='ms-2 me-2'>{pageNumber}</span>
          {nextPageAvailable ? <span className='cursor-pointer me-2' onClick={() => {
            setOffset(offset+limit);
          }}>Next</span> : null}
        </p>
      </div>
    </div>
  </ForumContainer>
}

export default MyForums;