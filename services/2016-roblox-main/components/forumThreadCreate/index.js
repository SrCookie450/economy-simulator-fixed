import {createUseStyles} from "react-jss";
import {useEffect, useRef, useState} from "react";
import {
  createThread,
  getCategoryBySubCategoryId,
  getPostById,
  getSubcategoryById,
  replyToPost
} from "../../services/forums";
import {ForumHeaderSubCategory} from "../forumHeader";
import Link from "../link";
import dayjs from "../../lib/dayjs";
import {useRouter} from "next/dist/client/router";
import ForumContainer from "../forumContainer";

const useStyles = createUseStyles({
  leftTable: {
    width: '100px',
  },
});

const ForumThreadCreate = props => {
  const router = useRouter();
  const inputRef = useRef(null);
  const subjectRef = useRef(null);
  const [feedback, setFeedback] = useState(null);
  const [locked, setLocked] = useState(false);
  const s = useStyles();
  const sub = getSubcategoryById(parseInt(props.id,10));

  return <ForumContainer>
        <ForumHeaderSubCategory cat={getCategoryBySubCategoryId(sub.id)} sub={sub} />
        <h3 className='mt-4 mb-4'>New Post</h3>

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
            <td><input type='text' className='w-100' ref={subjectRef} /> </td>
          </tr>
          <tr>
            <td className='fw-bolder text-end align-top'>Message:</td>
            <td>
              <textarea disabled={locked} ref={inputRef} className='w-100' rows={12} />
              {feedback ? <p className='text-danger'>{feedback}</p> : null}
              <button disabled={locked} className='mt-1' onClick={() => {
                setLocked(true);
                setFeedback(null);
                createThread({
                  subject: subjectRef.current.value,
                  subCategoryId: props.id,
                  post: inputRef.current.value,
                }).then((data) => {
                  router.push(`/Forum/ShowPost.aspx?PostID=${data.id}`);
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

export default ForumThreadCreate;