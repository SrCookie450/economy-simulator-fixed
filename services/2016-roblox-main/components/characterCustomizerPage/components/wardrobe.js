import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { itemNameToEncodedName } from "../../../services/catalog";
import { getInventory } from "../../../services/inventory";
import CharacterCustomizationStore from "../../../stores/characterPage";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import ItemImage from "../../itemImage";
import OldVerticalTabs from "../../oldVerticalTabs";
import assetTypes from "../assetTypes";
import WardrobeEntry from "./wardrobeEntry";
import Link from "../../link";

const creatableAssets = [
  2, // T-Shirt
  11, // Shirt
  12, // Pants
];

const wardrobeItems = [
  {
    label: null,
    categories: [
      {
        id: 17,
        name: 'Heads',
      },
      {
        id: 18,
        name: 'Faces',
      },
      {
        id: 2,
        name: 'T-Shirts',
      },
      {
        id: 11,
        name: 'Shirts',
      },
      {
        id: 12,
        name: 'Pants',
      },
      {
        id: 19,
        name: 'Gear',
      },
    ]
  },
  {
    label: 'Accessories',
    categories: [
      {
        id: 8,
        name: 'Hats',
      },
      {
        id: 41,
        name: 'Hair',
      },
      {
        id: 42,
        name: 'Face',
      },
      {
        id: 43,
        name: 'Neck',
      },
      {
        id: 44,
        name: 'Shoulder',
      },
      {
        id: 45,
        name: 'Front',
      },
      {
        id: 46,
        name: 'Back',
      },
      {
        id: 47,
        name: 'Waist',
      }
    ],
  },
  {
    label: null,
    categories: [
      {
        id: 27,
        name: 'Torsos',
      },
      {
        id: 29,
        name: 'L Arms',
      },
      {
        id: 28,
        name: 'R Arms',
      },
      {
        id: 30,
        name: 'L Legs',
      },
      {
        id: 31,
        name: 'R Legs',
      },
      {
        id: 32,
        name: 'Packages',
      }
    ]
  }
];


const useWardrobeStyles = createUseStyles({
  categoryEntry: {
    cursor: 'pointer',
    color: '#0055b3',
    textAlign: 'center',
    marginBottom: 0,
  },
  categoryWrapper: {
    margin: '0 auto',
  },
  selected: {
    fontWeight: 700,
  },
  paginationText: {
    textAlign: 'center',
    marginBottom: 0,
    userSelect: 'none',
  },
  pageEnabled: {
    cursor: 'pointer',
    color: '#0055b3',
  },
  pageDisabled: {
    color: 'inherit',
  },
});

const Wardrobe = props => {
  const s = useWardrobeStyles();
  const limit = getFlag('avatarPageInventoryLimit', 10);
  const characterStore = CharacterCustomizationStore.useContainer();
  const [inventory, setInventory] = useState(null);
  const [category, setCategory] = useState({ id: 2, name: 'T-Shirts' });
  const [page, setPage] = useState(1);
  const [cursor, setCursor] = useState(null);
  const [locked, setLocked] = useState(false);

  const Category = props => {
    return <span onClick={() => {
      setCursor(null);
      setPage(1);
      setCategory({
        name: props.name,
        id: props.id,
      });
    }} className={category.id === props.id ? s.selected : ''}> {props.children} {!props.last ? ' | ' : null} </span>
  }

  useEffect(() => {
    if (locked) return;
    setLocked(true);
    getInventory({
      userId: characterStore.userId,
      limit,
      cursor,
      assetTypeId: category.id
    }).then(result => {
      if (result.Data) {
        setInventory({
          nextPageCursor: result.Data.nextPageCursor,
          previousPageCursor: result.Data.previousPageCursor,
          data: result.Data.Items,
        })
      }
      console.log(result);
    }).catch(e => {
      console.error('[error] getInventory() error', e);
    }).finally(() => {
      setLocked(false);
    })
  }, [cursor, category]);

  return <div className='row'>
    <div className='col-12'>
      <div className={s.categoryWrapper}>
        {wardrobeItems.map(entry => {
          return <p className={s.categoryEntry} key={entry.categories.map(v => v.id).join(',')}>
            {entry.label ? <span className={s.categoryEntry}>{entry.label}</span> : null}
            {
              entry.categories.map((cat, idx, arr) => {
                const isLast = idx === (arr.length - 1);
                return <Category key={cat.id} id={cat.id} last={isLast}>{cat.name}</Category>
              })
            }
          </p>
        })}
        <p className={s.categoryEntry}>
          <Link href='/catalog'>
            <a>Shop</a>
          </Link>
          {creatableAssets.includes(category.id) && <span> || <Link href='/develop'><a>Create</a></Link></span>}
        </p>
      </div>
    </div>
    {
      inventory && <div className='col-12'>
        <div className='row'>
          {inventory.data.map((v, i) => {
            return <WardrobeEntry key={i} assetId={v.Item.AssetId} name={v.Item.Name} assetTypeId={v.Item.AssetType} assetTypeName={assetTypes[v.Item.AssetType]}/>
          })}
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            {
              inventory && inventory.data.length === 0 && inventory.previousPageCursor === null ? <p className='text-center'>No items available</p> :
                <p className={s.paginationText}>
                  <span className={inventory.previousPageCursor === null ? s.pageDisabled : s.pageEnabled} onClick={(e) => {
                    if (inventory.previousPageCursor === null) return;
                    setCursor(inventory.previousPageCursor);
                    setPage(page - 1);
                  }}>Previous </span>
                  <span className={s.pageDisabled}> {page} </span>
                  <span className={inventory.nextPageCursor === null ? s.pageDisabled : s.pageEnabled} onClick={(e) => {
                    if (inventory.nextPageCursor === null) return;
                    setCursor(inventory.nextPageCursor);
                    setPage(page + 1);
                  }}> Next</span>
                </p>
            }
          </div>
        </div>
      </div>
    }
  </div>
}

export default Wardrobe;