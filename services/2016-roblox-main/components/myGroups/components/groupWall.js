import dayjs from "dayjs";
import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { deletePost, getWall, postToWall } from "../../../services/groups";
import AuthenticationStore from "../../../stores/authentication";
import BcOverlay from "../../bcOverlay";
import CreatorLink from "../../creatorLink";
import GenericPagination from "../../genericPagination";
import PlayerImage from "../../playerImage";
import Button from "./button";
import OldCard from "./oldCard";

const useStyles = createUseStyles({
  textarea: {
    width: '100%',
  },
  wallpost: {
    minHeight: '100px',
  }
})

const GroupWall = props => {
  const { canPost, canView, canDelete, groupId } = props;
  const [posts, setPosts] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [loadWallFeedback, setLoadWallFeedback] = useState(null);
  const [hasNoPosts, setHasNoPosts] = useState(false);

  const auth = AuthenticationStore.useContainer();

  const textAreaRef = useRef(null);
  const postLock = useRef({ isLocked: false });
  const wallLock = useRef({ isLocked: false });
  const page = useRef(1);

  const getPosts = (cursor) => {
    if (wallLock.current.isLocked) return;
    wallLock.current.isLocked = true;
    getWall({
      groupId,
      cursor,
      limit: 10,
      sort: 'Desc',
    }).then(d => {
      if (d.data.length === 0 && cursor === null) {
        setHasNoPosts(true);
      }
      setPosts(d);
    }).catch(e => {
      setLoadWallFeedback('Wall is temporarily unavailable. Try again later.')
    }).finally(() => {
      wallLock.current.isLocked = false;
    })
  }

  useEffect(() => {
    getPosts(null);
  }, []);

  const s = useStyles();

  // conditionals
  const canPaginate = posts && (posts.nextPageCursor || posts.previousPageCursor);

  if (!canView) return null;
  return <div className='row'>
    <div className='col-12'>
      <OldCard>
        <div className='pe-2 ps-2 pt-1'>
          <h4 className='mb-0 fw-600 font-size-16'>Wall</h4>
          {feedback ? <p className='text-danger mt-2 mb-2'>{feedback}</p> : null}
          {canPost && <div className='row mt-2'>
            <div className='col-10'>
              <textarea ref={textAreaRef} rows={3} className={s.textarea} maxLength={1000} />
            </div>
            <div className='col-2'>
              <Button onClick={() => {
                if (postLock.current.isLocked) return
                postLock.current.isLocked = true;
                postToWall({
                  groupId,
                  content: textAreaRef.current.value,
                }).then(() => {
                  window.location.reload();
                }).catch(e => {
                  setFeedback(e.response?.data?.errors[0]?.message || e.message);
                }).finally(() => {
                  postLock.current.isLocked = false
                })
              }}>Post</Button>
            </div>
          </div>}
        </div>
      </OldCard>
    </div>
    <div className='col-12 mt-1'>
      {loadWallFeedback && <p className='mt-4 mb-4 text-center text-danger'>{loadWallFeedback}</p>}
      {hasNoPosts ? <p className='mt-4 mb-4 text-center'>Nobody has posted anything</p> : null}
      {
        posts && posts.data && posts.data.map(v => {
          if (v.poster === null) return null
          return <OldCard key={v.id}>
            <div className='row pe-2 ps-2 pt-1 pb-1'>
              <div className='col-3 pe-4'>
                <PlayerImage name={v.poster.user.username} id={v.poster.user.userId} />
                <BcOverlay id={v.poster.user.userId} />
              </div>
              <div className='col-7'>
                <p className={s.wallpost}>{v.body}</p>
                <p className='mb-0'><span className='lighten-3'>{dayjs(v.created).format('M/D/YYYY h:mm:ss A')}</span> by <CreatorLink type='User' id={v.poster.user.userId} name={v.poster.user.username} /></p>
                {(canDelete || v.poster.user.userId === auth.userId) && <a href='#' onClick={(e) => {
                  e.preventDefault();
                  deletePost({
                    postId: v.id,
                    groupId,
                  }).then(() => {
                    window.location.reload();
                  })
                }}>Delete</a>}
              </div>
            </div>
          </OldCard>
        })
      }
      <div className='mt-4'>
        {canPaginate ? <GenericPagination page={page.current} onClick={mode => {
          return e => {
            if (postLock.current.isLocked) return
            if (mode === 1) {
              if (!posts.nextPageCursor) return;
              getPosts(posts.nextPageCursor);
              page.current = page.current + 1;
            } else if (mode === -1) {
              if (!posts.previousPageCursor) return;
              getPosts(posts.previousPageCursor);
              page.current = page.current + 1;
            }
          }
        }} /> : null}
      </div>
    </div>
  </div>
}

export default GroupWall;