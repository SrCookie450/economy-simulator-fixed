import groupAdminStore from "../stores/groupAdminStore";
import {useEffect} from "react";
import Header from "./header";
import {getInfo} from "../../../services/groups";
import UserAdvertisement from "../../userAdvertisement";
import {getRobuxGroup} from "../../../services/economy";
import Content from "./content";

const Container = props => {
  const store = groupAdminStore.useContainer();
  useEffect(() => {
    // reset
    store.setGroupId(props.groupId);
    store.setInfo(null);
    store.setFunds(null);

    getInfo({groupId: props.groupId}).then(result => {
      store.setInfo(result);
    }).catch(e => {
      store.setFeedback('Error loading group information: ' + e.message);
    });

    getRobuxGroup({groupId: props.groupId}).then(result => {
      store.setFunds(result);
    }).catch(e => {})

  }, [props]);

  return <div className='container'>
    <UserAdvertisement type={1} />
    <div className='card br-none mt-4 border-0'>
      <Header />
      <Content />
    </div>
  </div>
}

export default Container;