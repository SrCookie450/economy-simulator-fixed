import dayjs from "dayjs";
import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { createComment, getComments } from "../../../services/catalog";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import CreatorLink from "../../creatorLink";
import ReportAbuse from "../../reportAbuse";
import PlayerImage from "../../playerImage";
import getFlag from "../../../lib/getFlag";

const useCreateCommentStyles = createUseStyles({
  createCommentTextArea: {
    width: '100%',
    background: '#ecf4ff',
    border: '2px solid #dee1ec',
    borderRadius: '4px',
    '&:focus-visible': {
      outline: 'none',
    },
  },
  buttonWrapper: {
    position: 'relative',
    width: '80px',
    float: 'right',
    marginTop: '-45px',
    marginRight: '13px',
  },
  continueButton: {
    fontSize: '14px',
  },
});

const CreateComment = props => {
  const s = useCreateCommentStyles();
  const buttonStyles = useButtonStyles();
  const authStore = AuthenticationStore.useContainer();
  const textAreaRef = useRef(null);
  const [locked, setLocked] = useState(null);
  const [error, setError] = useState(null);
  const onClick = e => {
    e.preventDefault();
    const text = textAreaRef.current.value;
    if (!text || text.length < 1) {
      return setError('Your comment is too short!');
    }
    if (text.length > 200) {
      return setError('Your comment is too long! It must be under 200 characters.');
    }
    setLocked(true);
    createComment({
      assetId: props.assetId,
      comment: text,
    }).then(() => {
      window.location.reload(); // todo: if we ever want to add spa support, then we need to add current comment to top of comments list
    }).catch(e => {
      const errorMessage = (() => {
        switch (e.message) {
          case 'VerifyEmail':
            return 'Your account must have a verified email before you can comment.';
          case 'RequiresCaptcha':
            return 'You must pass the robot test.';
          case 'UserTooNew':
            return 'Your account is too new to post comments. Try again later.';
          case 'FloodedGlobally':
          case 'FloodedPerAsset':
            return 'You are flood checked. Try again later.';
          case 'Moderated':
            return 'Your comment was moderated. Try again.';
          default:
            return 'An unknown error occurred posting your comment. Error: ' + e.message;
        }
      })();
      setError(errorMessage);
    }).finally(() => {
      setLocked(false);
    });
  }

  return <div className='row mt-4 mb-4'>
    <div className='col-3 pe-4'>
      <PlayerImage id={authStore.userId}/>
    </div>
    <div className='col-9'>
      {error && <p className='text-danger mb-0'>{error}</p>}
      <textarea disabled={locked} maxLength={200} rows={8} className={s.createCommentTextArea} ref={textAreaRef}/>
      <div className={s.buttonWrapper}>
        <ActionButton onClick={onClick} disabled={locked} className={buttonStyles.continueButton + ' ' + s.continueButton} label="Continue"/>
      </div>
    </div>
    <div className='col-12'>
      <div className='divider-top-thick divider-light mt-3'/>
    </div>
  </div>
}

const useCommentEntryStyles = createUseStyles({
  commentEntryDiv: {
    height: '120px',
    width: '100%',
    background: '#f6f6f5',
    border: '2px solid #e6e6e6',
    borderRadius: '4px',
  },
  commentText: {
    padding: '10px 10px',
  },
  commentCreatedAt: {
    color: '#666',
    fontSize: '12px',
    marginBottom: '8px',
  },
  report: {
    marginTop: '-15px',
    float: 'right',
  },
});

const CommentEntry = props => {
  const s = useCommentEntryStyles();
  const createdAt = dayjs(props.PostedDate, 'MMM D[,] YYYY [|] h:mm A').fromNow();

  return <div className='row mt-3'>
    <div className='col-3 pe-4'>
      <PlayerImage id={props.AuthorId}/>
    </div>
    <div className='col-9'>
      <div className={s.commentEntryDiv}>
        <div className={s.commentText}>
          <div className='row'>
            <div className='col-7'>
              <p className={s.commentCreatedAt}>Posted {createdAt} by <CreatorLink type='User' id={props.AuthorId} name={props.AuthorName}/></p>
            </div>
            <div className='col-5'>
              <div className={s.report}>
                <ReportAbuse id={props.Id} type='comment'/>
              </div>
            </div>
          </div>
          <p className='mb-0'>{props.Text}</p>
        </div>
      </div>
    </div>
  </div>
}

const useCommentStyles = createUseStyles({
  loadMore: {
    color: '#0055b3',
    cursor: 'pointer',
  },
})

const Comments = (props) => {
  const [comments, setComments] = useState(null);
  const authStore = AuthenticationStore.useContainer();
  const [offset, setOffset] = useState(0);
  const [locked, setLocked] = useState(false);
  const [areMoreAvailable, setAreMoreAvailable] = useState(false);
  const [disabled, setDisabled] = useState(false);
  const s = useCommentStyles();

  useEffect(() => {
    if (offset === 0) {
      setComments(null);
    }
    getComments({
      assetId: props.assetId,
      offset,
    }).then(data => {
      if (getFlag('commentsEndpointHasAreCommentsDisabledProp', false) && data.AreCommentsDisabled) {
        setDisabled(true);
        return;
      }
      setAreMoreAvailable(data.Comments.length >= data.MaxRows)
      if (comments) {
        comments.Comments.reverse().forEach(v => {
          data.Comments.unshift(v);
        })
      }
      setComments(data);
    })
  }, [props, offset]);

  if (disabled) {
    return <div className='row'>
      <div className='col-12'>
        <p className='mt-4 fw-600'>Comments are disabled for this item.</p>
      </div>
    </div>
  }

  return <div className='row'>
    <div className='col-12 col-lg-9'>
      {authStore.isAuthenticated && <CreateComment assetId={props.assetId}/>}
      {
        comments && comments.length === 0 ? <p>No comments.</p> : comments && comments.Comments.map(v => {
          return <CommentEntry key={v.Id} {...v}/>
        })
      }
    </div>
    <div className='col-12'>
      {
        !locked && comments && areMoreAvailable && <p className={`mb-0 text-center mt-2 ${s.loadMore}`} onClick={() => {
          setOffset(offset + comments.MaxRows)
        }}>More</p>
      }
    </div>
  </div>
}

export default Comments;