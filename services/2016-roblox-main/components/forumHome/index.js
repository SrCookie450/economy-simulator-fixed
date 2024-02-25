import {createUseStyles} from "react-jss";
import ForumContainer from "../forumContainer";
import {getCategories, getSubCategoryInfo} from "../../services/forums";
import useForumStyles from "./forumStyles";
import dayjs from "../../lib/dayjs";
import ForumHeader from "../forumHeader";
import Link from "../link";
import {useEffect, useReducer, useState} from "react";

const subReducer = (prev, act) => {
  if (act.type === 'ADD') {
    let newSt = {...prev};
    newSt[act.subCategoryId] = act.data;
    return newSt;
  }
  return prev
}

const ForumHome = props => {
  const s = useForumStyles();
  const cats = getCategories();
  const [subMap, dispatchSubMap] = useReducer(subReducer, {});

  useEffect(() => {
    let allSubs = cats.map(v => v.subCategories).reduce((a,b) => [...a, ...b]);
    for (const item of allSubs) {
      getSubCategoryInfo({
        subCategoryId: item.id
      }).then(data => {
        dispatchSubMap({
          type: 'ADD',
          subCategoryId: item.id,
          data: data,
        });
      })
    }
  }, []);

  return <ForumContainer>
    <ForumHeader>
      <p>
        <span className='fw-bold'>Current time: </span><span>{dayjs().format('MMM DD, hh:mm A')}</span>
      </p>
    </ForumHeader>
      {
        cats.map(v => {
          return <table className='w-100' key={v.id}>
          <thead className={s.header}>
            <tr className={s.headerRow}>
              <th>{v.name}</th>
              <th className='text-center'>Threads</th>
              <th className='text-center'>Posts</th>
              <th className={s.lastPostHeader + ' text-center'}>Last Post</th>
            </tr>
            </thead>
            <tbody>
            {
              v.subCategories.map(sub => {
                const loaded = subMap[sub.id] !== undefined;
                const lastPost = loaded ? subMap[sub.id].lastPost : undefined;
                return <tr className={s.bodyRow} key={sub.id}>
                  <td>
                    <Link href={`/Forum/ShowForum.aspx?ForumID=${sub.id}`}>
                      <a className='normal'>
                        <p className='mb-0 fw-bold mt-2'>{sub.name}</p>
                        <p className='mb-2'>{sub.description}</p>
                      </a>
                    </Link>
                  </td>
                  <td className='text-center'>
                    {loaded ? subMap[sub.id].threadCount.toLocaleString() : null}
                  </td>
                  <td className='text-center'>
                    {loaded ? subMap[sub.id].postCount.toLocaleString() : null}
                  </td>
                  <td>
                    {lastPost ? <div>
                      <Link href={`/Forum/ShowPost.aspx?PostID=${lastPost.threadId || lastPost.postId}`}>
                        <a className='normal'>
                          <p className='fw-bold mb-0 text-center'>{dayjs(lastPost.createdAt).fromNow()}</p>
                          <p className='mb-0 text-center'>{lastPost.username}</p>
                        </a>
                      </Link>
                    </div> : null}
                  </td>
                </tr>
              })
            }
            </tbody>
          </table>
        })
      }
  </ForumContainer>
}

export default ForumHome;