import UserAdvertisement from "../userAdvertisement";
import Container from "./components/container";
import SearchUsersStore from "./stores/searchUsersStore";

const SearchUsers = props => {
  return <div className='container'>
    <div className='row'>
      <div className='col-12 col-lg-10'>
        <div className='mb-4'>
          <UserAdvertisement type={1} />
        </div>
        <SearchUsersStore.Provider>
          <Container keyword={props.keyword} />
        </SearchUsersStore.Provider>
      </div>
      <div className='d-none d-lg-block col-lg-2'>
        <UserAdvertisement type={2} />
      </div>
    </div>
  </div>
}

export default SearchUsers;