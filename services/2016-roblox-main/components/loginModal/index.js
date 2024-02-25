import { useRef, useState } from "react";
import { createUseStyles } from "react-jss";
import { login } from '../../services/auth';

const useLoginModalStyles = createUseStyles({
  wrapper: {
    position: 'absolute',
    display: 'block',
    right: '10px',
    top: '45px',
    boxShadow: '0 5px 10px rgb(0 0 0 / 20%)',
    zIndex: 99,
    borderRadius: '3px',
  },
  card: {
    width: '320px',
    background: 'white',
    paddingTop: '20px',
    paddingBottom: '20px',
    paddingLeft: '20px',
    paddingRight: '20px',
  },
  input: {
    fontSize: '16px',
    margin: '10px 0',
    width: '280px',
  },
  btn: {
    width: '100%',
    color: 'white',
    fontWeight: 400,
    transition: 'none',
    '&:hover': {
      color: 'white',
      transition: 'none',
      boxShadow: '0 1px 3px rgb(150 150 150 / 74%)',
    },
  },
  btnPrimary: {
    background: '#00A2FF',
    '&:hover': {
      background: '#32B5FF',
    },
  },
  btnSecondary: {
    color: '#757575',
    border: '1px solid #B8B8B8',
    '&:hover': {
      color: '#757575',
    },
  },
  forgotPass: {
    color: '#00A2FF',
  },
});

const LoginModal = props => {
  const s = useLoginModalStyles();
  const userRef = useRef(null);
  const passRef = useRef(null);
  const [locked, setLocked] = useState(null);
  const [error, setError] = useState(null);

  return <div className={s.wrapper}>
    <div className={s.card}>
      {error && <p className='text-danger mb-0'>{error}</p>}
      <input ref={userRef} className={`form-control ${s.input}`} type='text' placeholder='Username'></input>
      <input ref={passRef} className={`form-control ${s.input}`} type='password' placeholder='Password'></input>
      <div className='row'>
        <div className='col-6'>
          <button disabled={locked} className={`btn ${s.btnPrimary} ${s.btn}`} onClick={(e) => {
            e.preventDefault();
            setLocked(true);
            const username = userRef.current.value;
            const password = passRef.current.value;
            login({ username, password }).then(userInfo => {
              console.log(userInfo);
              window.location.reload();
            })
              .catch(e => {
                if (e.response && e.response.data) {
                  setError(e.response.data.errors[0].message);
                } else {
                  setError(e.message);
                }
              })
              .finally(() => {
                setLocked(false);
              })
          }}>Log In</button>
        </div>
        <div className='col-6'>
          <button className={`btn ${s.btnSecondary} ${s.btn}`}>Sign up</button>
        </div>
      </div>
      <div className='row mt-2'>
        <div className='col-12'>
          <p>
            <a className={s.forgotPass} href='/auth/password-reset'>Forgot Password?</a>
          </p>
        </div>
      </div>
    </div>
  </div>
}

export default LoginModal;