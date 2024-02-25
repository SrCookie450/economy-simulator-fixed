import GroupAdminStore from "./stores/groupAdminStore";
import Container from "./components/container";

const GroupAdmin = props => {
  return <GroupAdminStore.Provider>
    <Container {...props} />
  </GroupAdminStore.Provider>
}

export default GroupAdmin;