import { useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import ActionButton from "../../components/actionButton";
import Robux from "../../components/catalogDetailsPage/components/robux";
import { createGroup } from "../../services/groups";

const useStyles = createUseStyles({
  input: {
    width: '100%',
  },
  buttonWrapper: {
    float: 'left',
  },
  createGroupContainer: {
    background: '#fff',
    padding: '4px 8px',
  },
});

const CreateGroupPage = props => {
  const [locked, setLocked] = useState(false);
  const [feedback, setFeedback] = useState(null);

  const nameRef = useRef(null);
  const descRef = useRef(null);
  const iconRef = useRef(null);
  const s = useStyles();
  return <div className={'container ' + s.createGroupContainer}>
    <div className='row'>
      <div className='col-12 col-lg-8'>
        <div className='row'>
          <div className='col-12'>
            <h2>Create A Group</h2>
            {feedback && <p className='text-danger mb-2'>{feedback}</p>}
          </div>
        </div>
        <div className='row'>
          <div className='col-12'>
            <p className='ms-4 fw-600'>Name:</p>
            <input ref={nameRef} disabled={locked} type='text' autoComplete='off' className={s.input}></input>
          </div>
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            <p className='ms-4 fw-600'>Description:</p>
            <textarea ref={descRef} disabled={locked} rows={16} autoComplete='off' className={s.input}></textarea>
          </div>
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            <p className='ms-4 fw-600'>Emblem:</p>
            <input ref={iconRef} disabled={locked} type='file' id='upload-file'></input>
          </div>
        </div>
        <div className='row mt-0'>
          <div className='col-12'>
            <p>Creating a group costs <Robux inline={true}>100</Robux>. By clicking Purchase, your account will be charged <Robux inline={true}>100</Robux>.</p>
          </div>
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            <div className={s.buttonWrapper}>
              <ActionButton disabled={locked} label='Purchase' onClick={() => {
                setLocked(true);
                setFeedback(null);
                createGroup({
                  name: nameRef.current.value,
                  description: descRef.current.value,
                  iconElement: document.getElementById('upload-file'),
                }).then(d => {
                  window.location.href = `/My/Groups.aspx?gid=${d.id}`;
                }).catch(e => {
                  setFeedback(e.response?.data?.errors[0]?.message || e.message);
                }).finally(() => {
                  setLocked(false);
                })
              }}></ActionButton>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default CreateGroupPage;