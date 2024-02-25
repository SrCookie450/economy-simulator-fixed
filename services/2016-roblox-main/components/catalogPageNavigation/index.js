import React from "react";
import { createUseStyles } from "react-jss";
import CatalogPageStore from "../../stores/catalogPage";
import CatalogLegend from "../catalogLegend";
import Dropdown from "../dropdown";

const navigationItems = [
  {
    name: 'Featured',
    clickData: '', // don't care
    children: {
      title: 'Featured Types',
      children: [
        {
          name: 'All Featured Items',
          clickData: 'Featured,',
        },
        {
          name: 'Featured Hats',
          clickData: 'Featured,Accessories',
        },
        {
          name: 'Featured Gear',
          clickData: 'Featured,Gear',
        },
        {
          name: 'Featured Faces',
          clickData: 'Featured,Faces',
        },
        // TODO:
        /*
        {
          name: 'Featured Packages',
          clickData: 'Featured,Packages',
        },
        */
      ],
    },
  },
  {
    name: 'Collectibles',
    clickData: '',
    children: {
      title: 'Collectible Types',
      children: [
        {
          name: 'All Collectibles',
          clickData: 'Collectibles,',
        },
        {
          name: 'Collectible Faces',
          clickData: 'Collectibles,Faces',
        },
        {
          name: 'Collectible Hats',
          clickData: 'Collectibles,Accessories',
        },
        {
          name: 'Collectible Gear',
          clickData: 'Collectibles,Gear',
        },
      ]
    },
  },
  {
    name: 'separator',
    clickData: '',
  },
  {
    name: 'All Categories',
    clickData: 'all,all',
  },
  {
    name: 'Clothing',
    clickData: '',
    children: {
      title: 'Clothing Types',
      children: [
        {
          name: 'All Clothing',
          clickData: 'null,Clothing',
        },
        {
          name: 'Hats',
          clickData: 'null,Accessories',
        },
        {
          name: 'Shirts',
          clickData: 'null,Shirt',
        },
        {
          name: 'T-Shirts',
          clickData: 'null,TeeShirt',
        },
        {
          name: 'Pants',
          clickData: 'null,Pants',
        },
        {
          name: 'Packages',
          clickData: 'null,Packages',
        },
      ]
    }
  },
  {
    name: 'Body Parts',
    clickData: '',
    children: {
      title: 'Body Part Types',
      children: [
        {
          name: 'All Body Parts',
          clickData: 'bodyparts,All',
        },
        {
          name: 'Heads',
          clickData: 'bodyparts,Heads'
        },
        {
          name: 'Faces',
          clickData: 'bodyparts,Faces',
        },
        {
          name: 'Packages',
          clickData: 'bodyparts,Packages'
        },
      ],
    },
  },
  {
    name: 'Gear',
    clickData: '',
    children: {
      title: 'Gear Categories',
      children: [
        {
          name: 'All Gear',
          clickData: 'gear,all',
        },
        {
          name: 'Melee Weapon',
          clickData: 'gear,melee',
        },
        {
          name: 'Ranged Weapon',
          clickData: 'gear,ranged',
        },
        {
          name: 'Explosive',
          clickData: 'gear,explosive',
        },
        {
          name: 'Power Up',
          clickData: 'gear,powerup',
        },
        {
          name: 'Navigation Enhancer',
          clickData: 'gear,navigation',
        },
        {
          name: 'Musical Instrument',
          clickData: 'gear,musical',
        },
        {
          name: 'Social Item',
          clickData: 'gear,social',
        },
        {
          name: 'Building Tool',
          clickData: 'gear,building',
        },
        {
          name: 'Personal Transport',
          clickData: 'gear,transport',
        },
      ]
    }
  }
];

const useTitleStyles = createUseStyles({
  top: {
    fontSize: '12px',
    color: 'white',
    fontWeight: 600,
    lineHeight: 'normal',
    marginBottom: '-5px',
    paddingLeft: '4px',
    textShadow: '1px 1px 1px black',
  },
  bottom: {
    fontSize: '20px',
    color: 'white',
    fontWeight: 700,
    lineHeight: 'normal',
    paddingLeft: '4px',
    textShadow: '1px 1px 1px black',
  }
})
const CatalogPageNavigation = () => {
  const store = CatalogPageStore.useContainer();
  const title = useTitleStyles();
  return <>
    <Dropdown
      onClick={(e, clickData) => {
        console.log('[info] catalog dropdown clicked with', clickData);
        const [category, subCategory] = clickData.split(',');
        if (category === '') {
          console.log('[info] bad category');
          return;
        }
        store.setCategory(category);
        store.setSubCategory(subCategory);
      }}
      title={<div>
        <p className={`mt-0 ${title.top}`}>Browse by</p>
        <h2 className={`mb-0 mt-0 ${title.bottom}`}>Category</h2>
      </div>}
      items={navigationItems}></Dropdown>
  </>
}

export default CatalogPageNavigation;