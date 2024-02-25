import UserProfileStore from "../stores/UserProfileStore";

const TabSection = props => {
  const store = UserProfileStore.useContainer();
  if (store.tab === props.tab) {
    return <>
      {props.children}
    </>
  }
  return null;
}

export default TabSection;