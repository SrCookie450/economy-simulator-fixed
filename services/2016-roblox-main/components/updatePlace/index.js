import UpdatePlaceStore from "./stores/updatePlaceStore";
import Container from "./container";

const UpdatePlace = props => {
  return <UpdatePlaceStore.Provider>
    <Container {...props} />
  </UpdatePlaceStore.Provider>
}

export default UpdatePlace;