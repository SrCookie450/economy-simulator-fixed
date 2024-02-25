import {createUseStyles} from "react-jss";
import {useEffect, useRef, useState} from "react";
import {getCategoryBySubCategoryId, getPostById, getSubcategoryById, replyToPost} from "../../services/forums";
import {ForumHeaderSubCategory} from "../forumHeader";
import Link from "../link";
import dayjs from "../../lib/dayjs";
import {useRouter} from "next/dist/client/router";
import ForumContainer from "../forumContainer";

const useStyles = createUseStyles({
  subheaderCard: {
    background: '#29508d',
  },
  leftTable: {
    width: '100px',
  },
  postText: {
    whiteSpace: 'break-spaces',
  },
});

const ForumPostReply = props => {
  const router = useRouter();
  const inputRef = useRef(null);
  const [feedback, setFeedback] = useState(null);
  const [locked, setLocked] = useState(false);
  const s = useStyles();
  const {id} = props;
  const [details, setDetails] = useState(null);
  useEffect(() => {
    getPostById({postId: id}).then(d => {
      setDetails(d);
    })
  }, [id]);

  if (!details)
    return null;

  return <ForumContainer>
        <ForumHeaderSubCategory cat={getCategoryBySubCategoryId(details.subCategoryId)} sub={getSubcategoryById(details.subCategoryId)} />
        <h3 className='mt-4 mb-4'>Reply to Post</h3>
        <div className={s.subheaderCard}>
          <h4 className='text-white text-center mb-0 pt-2 pb-2'>Original Post</h4>
        </div>
        <p>
          <Link href={`/Forum/ShowPost.aspx?PostID=${id}`}>
            <a>
              {details.title}
            </a>
          </Link>
          <p>Posted by <Link href={`/users/${details.userId}/profile`}>
            <a>
              {details.username}
            </a>
          </Link> on {dayjs(details.createdAt).format('MM-DD-YY hh:mm A')}</p>
          <p className={s.postText}>{details.post}</p>
        </p>

        <div className={s.subheaderCard}>
          <h4 className='text-white text-center mb-0 pt-2 pb-2'>New Post</h4>
        </div>
        <table className='w-100'>
          <thead>
          <tr>
            <td className={s.leftTable} />
            <td />
          </tr>
          </thead>
          <tbody>
          <tr>
            <td className='fw-bolder text-end'>Subject:</td>
            <td>Re: {details.title}</td>
          </tr>
          <tr>
            <td className='fw-bolder text-end align-top'>Message:</td>
            <td>
              <textarea disabled={locked} ref={inputRef} className='w-100' rows={12} />
              {feedback ? <p className='text-danger'>{feedback}</p> : null}
              <button disabled={locked} className='mt-1' onClick={() => {
                setLocked(true);
                setFeedback(null);
                replyToPost({
                  postId: id,
                  post: inputRef.current.value,
                }).then(() => {
                  router.push(`/Forum/ShowPost.aspx?PostID=${id}`);
                }).catch(e => {
                  setLocked(false);
                  setFeedback(e.message);
                })
              }}>Post</button>
            </td>
          </tr>
          </tbody>
        </table>
  </ForumContainer>
}

export default ForumPostReply;