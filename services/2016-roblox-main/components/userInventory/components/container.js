import { useEffect } from "react";
import UserInventoryStore from "../stores/userInventoryStore"
import {createUseStyles} from "react-jss";
import CategorySelection from "./categorySelection";
import InventoryGrid from "./inventoryGrid";
import {getUserInfo} from "../../../services/users";

const useStyles = createUseStyles({
  title: {
    fontSize: '48px',
    fontWeight: 300,
    color: 'rgb(25,25,25)',
  },
  container: {
    background: '#e3e3e3',
  }
})

const Container = props => {
  const store = UserInventoryStore.useContainer();
  const s = useStyles();
  useEffect(() => {
    store.setMode(props.mode);
    store.setUserId(props.userId);

    if (!props.userId) return;
    getUserInfo({userId: props.userId}).then(data => store.setUserInfo(data));
    store.requestInventory(props.mode, props.userId, store.category.value, '');
  }, [props]);

  return <div className={'container ' + s.container}>
    <div className='row'>
      <div className='col-12'>
        <h1 className={s.title}>{store.userInfo?.name}'s {props.mode}</h1>
      </div>
      <CategorySelection />
      <InventoryGrid />
    </div>
  </div>
}

export default Container;