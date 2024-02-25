import {createUseStyles} from "react-jss";
import assetTypes from "../../characterCustomizerPage/assetTypes";
import configureItemStore from "../stores/configureItemStore";
import {getItemUrl, itemNameToEncodedName} from "../../../services/catalog";
import Link from "../../link";

const useStyles = createUseStyles({
  header: {
    fontSize: '32px',
    fontWeight: 700,
    marginLeft: '8px',
  },
});

const ConfigureHeader = props => {
  const s = useStyles();
  const store = configureItemStore.useContainer();

  return <>
    <h1 className={s.header}>Configure {assetTypes[store.details.assetType]}</h1>
    <p>
      <Link href={getItemUrl({assetId: store.details.id, name: store.details.name})}>
        <a>Back</a>
      </Link>
    </p>
  </>
}

export default ConfigureHeader;