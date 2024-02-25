import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../stores/catalogPage";

const useStyles = createUseStyles({
  link: {
    fontSize: '13px',
  }
})

const SubCategoryFilter = props => {
  const store = CatalogPageStore.useContainer();
  const s = useStyles();
  const sub = store.subCategory;
  return <>
    {
      props.subCategories.map(v => {
        return <p key={v.value || v.displayName} onClick={() => {
          if (store.locked) return;
          store.setSubCategory(v.value || v.displayName);
        }} className={'mb-0 ' + s.link + ' ' + ((v.value || v.displayName) === sub ? '' : 'fake-link')}>
          {v.displayName}
        </p>
      })
    }
  </>
}

export default SubCategoryFilter;