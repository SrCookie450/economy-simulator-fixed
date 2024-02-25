import {useEffect, useState} from "react";
import {getCategoryBySubCategoryId, getPostsInSubcategory, getSubcategoryById} from "../../services/forums";
import Link from "../link";
import {ForumHeaderSubCategory} from "../forumHeader";
import dayjs from "../../lib/dayjs";
import ForumContainer from "../forumContainer";
import useForumStyles from "../forumHome/forumStyles";
import getFlag from "../../lib/getFlag";

const limit = 15;
const ForumSubcategory = (props) => {
  const s = useForumStyles();
  const {id} = props;
  const [sub, setSub] = useState(null);
  const [cat, setCat] = useState(null);
  const [posts, setPosts] = useState(null);
  const [offset, setOffset] = useState(props.page * limit - limit);
  useEffect(() => {
    setPosts(null);
    setOffset(props.page * limit - limit);
  }, [props]);

  useEffect(() => {
    const subId = parseInt(id, 10);
    setSub(getSubcategoryById(subId));
    setCat(getCategoryBySubCategoryId(subId));

    getPostsInSubcategory({
      subCategoryId: subId,
      cursor: offset.toString(),
      limit: limit,
    }).then(data => {
      setPosts(data);
    })
  }, [id, offset]);

  const nextPageAvailable = posts && posts.data.length >= limit;
  const pageNumber = props.page;

  if (!sub)
    return null;

  return <ForumContainer>
        <ForumHeaderSubCategory cat={cat} sub={sub} />
        <div className='w-100'>
          <div className='float-left'>
            <Link href={`/Forum/AddPost.aspx?ForumID=${sub.id}`}>
              <a>
                New Thread
              </a>
            </Link>
          </div>
          {getFlag('forumSearchEnabled', false) ?
          <div className='float-right'>
            <form method='GET' action='/Forum/SearchForum.aspx'>
              <input type='hidden' name='ForumID' value={sub.id} />
              <div className='d-inline-block'>
                <p className='fw-bold'>Search This Forum: </p>
              </div>
              <div className='d-inline-block me-1 ms-1'>
                <input type='text' name='query' />
              </div>
              <div className='d-inline-block'>
                <input type='submit' value='Go' />
              </div>
            </form>
          </div> : null}
        </div>

            <table className='w-100'>
              <thead className={s.header}>
              <tr className={s.headerRow}>
                <th>Subject</th>
                <th>Author</th>
                <th className='text-center'>Replies</th>
                <th className='text-center'>Views</th>
                <th className='text-center'>Last Post</th>
              </tr>
              </thead>
              <tbody>
              {
                posts === null ? <tr>
                  <td>
                    <div className='min-vh-100' />
                  </td>
                  <td />
                  <td />
                  <td />
                  <td />
                </tr> : null
              }
              {
                posts ? posts.data.map(e => {
                  const v = e.post;
                  const replyCount = e.replyCount;
                  const isRead = e.isRead;

                  return <tr className={s.bodyRow}>
                    <td>
                      <Link href={`/Forum/ShowPost.aspx?PostID=${v.threadId}`}>
                        <a className='normal'>
                          <div className='d-inline-block'>
                            <img src={isRead ? '/img/thread-read.png' : '/img/thread-unread.png'} alt='Thread Icon' />
                          </div>
                          <div className='d-inline-block'>
                            {v.title}
                          </div>
                        </a>
                      </Link>
                    </td>
                    <td>{v.username}</td>
                    <td className='text-center'>{replyCount.toLocaleString()}</td>
                    <td className='text-center'>-</td>
                    <td>
                      <p className='text-center mb-0 fw-bold'>{dayjs(v.createdAt).fromNow()/*.format('hh:mm A')*/}</p>
                      <p className='text-center mb-0'>
                        <Link href={`/users/${v.userId}/profile`}>
                          <a className='normal'>
                            {v.username}
                          </a>
                        </Link>
                      </p>
                    </td>
                  </tr>
                }) : null
              }
              </tbody>
              <thead className={s.header}>
              {/* Hacky but I can't think of a more reliable way */}
              <tr className={s.headerRow}>
                <th><p className='mb-0'>&emsp;</p></th>
                <th/>
                <th/>
                <th/>
                <th/>
              </tr>
              </thead>
            </table>
    <div className='row'>
      <div className='col-6'>
        <p className='fw-bolder'>Page {pageNumber}</p>
      </div>
      <div className='col-6'>
        <p className='mb-0 text-end'>
          {pageNumber > 1 ? <Link href={`/Forum/ShowForum.aspx?ForumID=${id}&Page=${pageNumber-1}`}>
            <a className='pe-2'>{pageNumber-1}</a>
          </Link> : null}
          <span>{pageNumber}</span>
          {nextPageAvailable ? <Link href={`/Forum/ShowForum.aspx?ForumID=${id}&Page=${pageNumber+1}`}>
            <a className='ps-2'>{pageNumber+1}</a>
          </Link> : null}
        </p>
      </div>
    </div>

  </ForumContainer>
}

export default ForumSubcategory;