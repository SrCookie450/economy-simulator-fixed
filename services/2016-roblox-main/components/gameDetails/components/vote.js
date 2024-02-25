import gameDetailsStore from "../stores/gameDetailsStore";
import {useEffect, useState} from "react";
import {multiGetGameVotes, voteOnGame} from "../../../services/games";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  redBg: {
    background: '#CE645B',
    width: '100%',
    height: '5px',
  },
  greenBg: {
    background: '#52A846',
    height: '5px',
  },
  borderLeft: {
    borderLeft: '1px solid #c3c3c3',
  },
  thumbsText: {
    fontSize: '12px',
  },
});

const Vote = props => {
  const s= useStyles();
  const store = gameDetailsStore.useContainer();
  const [votes, setVotes] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [locked, setLocked] = useState(false);

  const loadVotes = () => {
    if (store.universeDetails && store.universeDetails.id) {
      multiGetGameVotes({universeIds: [store.universeDetails.id]}).then(data => {
        setVotes(data[0]);
      })
    }
  }
  useEffect(() => {
    loadVotes();
  }, [store.universeDetails]);

  const submitVote = (didUpvote) => {
    if (locked) return;
    setLocked(true);
    setFeedback(null);

    voteOnGame({universeId: store.universeDetails.id, isUpvote: didUpvote}).then(result => {
      loadVotes();
    }).catch(e => {
      if (!e.response || !e.response.data || !e.response.data.errors) {
        setFeedback('An unknown error has occurred. Try again.');
        return
      }
      const status = e.response.status;
      const err = e.response.data.errors[0];
      const code = err.code;
      const msg = err.message;
      if (status === 403 && code === 6) {
        setFeedback('You must play this game before you can vote on it.');
      }else if (status === 400 && (code === 3 || code === 2)) {
        setFeedback('You cannot vote on this game.');
      }else if (status === 429 && code === 5) {
        setFeedback('Too many attempts to vote. Try again later.');
      }else if(msg) {
        setFeedback(msg);
      }
    }).finally(() => {
      setLocked(false);
    })
  }
  if (votes !== null) {
    const total = votes.upVotes + votes.downVotes;
    const greenPercent = Math.ceil((votes.upVotes / total) * 100);
    return <div className='row mt-1'>
      <div className='col-10 offset-1 col-lg-6 offset-lg-3'>
        {feedback ? <p className='text-danger mb-0'>{feedback}</p> : null}
        <div className={'row ps-3 pe-3 ' + (locked ? 'opacity-50' : '')}>
          <div className='col-6 p-0'>
            <div className='tiny-thumbs-up cursor-pointer' onClick={() => {
              submitVote(true);
            }}/>
            <span className={s.thumbsText + ' ms-1'}>{votes.upVotes.toLocaleString()}</span>
          </div>
          <div className={'col-6 p-0 text-right ' + s.borderLeft}>
            <div className='float-right'>
              <span className={s.thumbsText}>{votes.downVotes.toLocaleString()}</span>
              <div className='tiny-thumbs-down cursor-pointer' onClick={() => {
                submitVote(false);
              }}/>
            </div>
          </div>
        </div>
        <div className={s.redBg + ' mt-1'}>
          <div className={s.greenBg} style={{width: greenPercent + '%'}} />
        </div>
      </div>
    </div>
  }

  return null;
}

export default Vote;