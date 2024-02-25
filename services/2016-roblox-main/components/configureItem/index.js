import ConfigureItemStore from "./stores/configureItemStore";
import Container from "./components/container";

const ConfigureItem = props => {
  return <div className='container bg-white'>
    <ConfigureItemStore.Provider>
      <Container assetId={props.assetId} />
    </ConfigureItemStore.Provider>
  </div>
}

export default ConfigureItem;