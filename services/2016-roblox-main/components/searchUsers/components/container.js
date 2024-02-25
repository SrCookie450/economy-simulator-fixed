import searchUsersStore from "../stores/searchUsersStore";
import {useEffect} from "react";
import InputRow from "./inputRow";
import UsersRow from "./usersRow";
import {createUseStyles} from "react-jss";

const useStyles = createUseStyles({
  row: {
    background: '#fff',
    minHeight: '100vh',
  }
})

const Container = props => {
  const store = searchUsersStore.useContainer();
  const s = useStyles();
  useEffect(() => {
    store.setKeyword(props.keyword);
    store.setData(null);
  }, [props]);

  return <div className={'row '  +s.row}>
    <div className='col-12'>
      <InputRow />
      <UsersRow />
    </div>
  </div>
}

export default Container;