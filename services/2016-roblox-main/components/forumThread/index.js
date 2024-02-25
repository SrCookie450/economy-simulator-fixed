import {useEffect, useState} from "react";
import {
  deletePost,
  getCategoryBySubCategoryId,
  getRepliesToThread,
  getSubcategoryById,
  getThreadInfoById, markAsRead
} from "../../services/forums";
import UserAdvertisement from "../userAdvertisement";
import {ForumHeaderSubCategory} from "../forumHeader";
import {createUseStyles} from "react-jss";
import Link from "../link";
import PlayerImage from "../playerImage";
import dayjs from "../../lib/dayjs";
import ForumContainer from "../forumContainer";
import BcOverlay from "../bcOverlay";

const useStyles = createUseStyles({
  forumHeader: {
    background: '#29508d',
  },
  avatarHead: {
    width: '150px',
  },
  postRow: {
    background: '#f9f9f9',
    borderBottom: '1px solid #c3c3c3',
  },
  userStat: {
    marginBottom: 0,
    fontSize: '0.9rem',
  },
  postText: {
    whiteSpace: 'break-spaces',
    wordWrap: 'anywhere',
  },
})
const limit = 15;
const ForumThread = props => {
  const s = useStyles();
  const {id} = props;
  const [threadInfo, setThreadInfo] = useState(null);
  const [sub, setSub] = useState(null);
  const [cat, setCat] = useState(null);
  const [posts, setPosts] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);

  useEffect(() => {
    getThreadInfoById({threadId: id}).then(d => {
      setThreadInfo(d);
      setCat(getCategoryBySubCategoryId(d.subCategoryId));
      setSub(getSubcategoryById(d.subCategoryId));
    });
  }, [id]);

  useEffect(() => {
    setPageNumber(props.page);
  }, [props.page]);

  useEffect(() => {
    setPosts(props.posts);
    if (props.posts.data.length) {
      markAsRead({
        postId: props.posts.data[props.posts.data.length-1].post.postId,
      });
    }
  }, [pageNumber, props]);
  const nextPageAvailable = posts && posts.data && posts.data.length >= limit;

  if (!threadInfo || !cat || !sub)
    return null;

  const PreviousAndNextThread = () => {
    return  <div className={s.forumHeader}>
      <h4 className='text-end pe-2 pt-1 pb-1 text-white mb-0'>
        {threadInfo.previousThreadId ? <Link href={`/Forum/ShowPost.aspx?PostID=${threadInfo.previousThreadId}`}>
          <a className='text-white'>
            Previous Thread
          </a>
        </Link> : <span className='opacity-50'>Previous Thread</span>}
        <span className='fw-bolder ps-1 pe-1 text-black'>::</span>
        {threadInfo.nextThreadId ? <Link href={`/Forum/ShowPost.aspx?PostID=${threadInfo.nextThreadId}`}>
          <a className='text-white'>
            Next Thread
          </a>
        </Link> : <span className='opacity-50'>Next Thread</span>}
      </h4>
    </div>
  }

  return <ForumContainer>
        <ForumHeaderSubCategory cat={cat} sub={sub} />
        <h3>{threadInfo.title}</h3>
        <PreviousAndNextThread />
        <table className='w-100'>
          <thead>
          <tr>
            <th className={s.avatarHead} />
            <th />
          </tr>
          </thead>
          <tbody>
          {posts && posts.data.length === 0 ? <tr>
            <td>No posts available</td>
          </tr> : null}
          {
            posts ? posts.data.map(v => {
              const post = v.post;

              return <tr className={s.postRow}>
                <td className='align-top'>
                  <Link href={`/users/${post.userId}/profile`}>
                    <a>
                      {post.username}
                    </a>
                  </Link>
                  <PlayerImage id={post.userId} />
                  <BcOverlay id={post.userId} />
                  <p className={s.userStat}>
                    <span className='fw-bold'>Joined: </span><span>{dayjs(v.createdAt).format('DD MMM YYYY')}</span>
                  </p>
                  <p className={s.userStat + ' mb-4'}>
                    <span className='fw-bold'>Total Posts: </span><span>{v.postCount}</span>
                  </p>
                </td>
                <td className='align-top'>
                  <div className='ps-4'>
                    <p className='mb-0'>{dayjs(post.createdAt).format('MM-DD-YYYY hh:mm A')}</p>
                    <p className={'mt-1 ' + s.postText}>{post.post}</p>
                    {
                      v.canDelete ? <p className='text-danger cursor-pointer' onClick={() => {
                        let res = prompt('Type "yes" to confirm deletion');
                        if (res === 'yes') {
                          deletePost({
                            postId: post.postId,
                          }).then(() => {
                            post.post = '[ Content Deleted ]';
                            if (post.title) {
                              post.title = '[ Content Deleted ]';
                            }
                            if (post.threadId === null) {
                              threadInfo.title = '[ Content Deleted ]';
                            }
                            setPosts({...posts});
                            setThreadInfo({...threadInfo});
                          })
                        }
                      }}>Delete</p> : null
                    }
                  </div>
                </td>
              </tr>
            }) : null
          }
          </tbody>
        </table>
        <PreviousAndNextThread />
        <div className='row'>
          <div className='col-6'>
            <p className='fw-bold'>Page {pageNumber}</p>
          </div>
          <div className='col-6'>
            <p className='mb-0 text-end'>
              {pageNumber > 1 ? <Link href={`/Forum/ShowPost.aspx?PostID=${id}&Page=${pageNumber-1}`}>
                <a className='pe-2'>{pageNumber-1}</a>
              </Link> : null}
              <span>{pageNumber}</span>
              {nextPageAvailable ? <Link href={`/Forum/ShowPost.aspx?PostID=${id}&Page=${pageNumber+1}`}>
                <a className='ps-2'>{pageNumber+1}</a>
              </Link> : null}
            </p>
          </div>
        </div>
        <p className='text-center mb-0 mt-4'>
          <Link href={`/Forum/AddPost.aspx?PostID=${id}&mode=flat`}>
            <a>Add a Reply</a>
          </Link>
        </p>
  </ForumContainer>
}

export default ForumThread;