import React from "react";
import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../stores/catalogPage";
import GenreFilter from "./genreFilter";
import SubCategoryFilter from "./subcategory";

const useFilterStyles = createUseStyles({
  header: {
    paddingTop: '20px',
  },
});

const useClothingFilterStyles = createUseStyles({
  label: {
    fontSize: '15px',
    fontWeight: 600,
    paddingBottom: '5px',
  },
  subCategories: {
    paddingLeft: '5px',
  },
})
const ClothingFilter = props => {
  const s = useClothingFilterStyles();
  return <div>
    <p className={`mb-0 ${s.label}`}>Clothing Type</p>
    <div className={s.subCategories}>
      <SubCategoryFilter subCategories={
        [
          {
            displayName: 'All Clothing',
            value: 'Clothing',
          },
          {
            displayName: 'Hats',
          },
          {
            displayName: 'Shirts',
          },
          {
            displayName: 'T-Shirts',
            value: 'TeeShirt',
          },

          {
            displayName: 'Pants',
          },
          {
            displayName: 'Packages',
          },
        ]
      }></SubCategoryFilter>
    </div>
  </div>
}

const CatalogFilters = props => {
  const s = useFilterStyles();
  const store = CatalogPageStore.useContainer();
  const hasClothingTypeFilter = store.subCategory === 'Clothing' || store.subCategory === 'Hats' || store.subCategory === 'Accessories' || store.subCategory === 'TeeShirt' || store.subCategory === 'Shirt' || store.subCategory === 'Pant';

  if (store.category === 'featured') {
    return null;
  }
  return <div className='row'>
    <div className='col-12'>
      <h2 className={s.header}>Filters</h2>
    </div>
    <div className='col-12'>
      {hasClothingTypeFilter && <ClothingFilter></ClothingFilter>}
      {hasClothingTypeFilter && <div className='divider-top mt-4 mb-4 divider-light'></div>}
      <GenreFilter></GenreFilter>
    </div>
  </div>
}

export default CatalogFilters;