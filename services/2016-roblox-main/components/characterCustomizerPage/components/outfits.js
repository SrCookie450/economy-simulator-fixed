import dayjs from "dayjs";
import React, { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import getFlag from "../../../lib/getFlag";
import { getBaseUrl } from "../../../lib/request";
import { deleteOutfit, getOutfits, renameOutfit, wearOutfit } from "../../../services/avatar";
import { multiGetOutfitThumbnails } from "../../../services/thumbnails";
import CharacterCustomizationStore from "../../../stores/characterPage";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import GearDropdown from "../../gearDropdown";
import CreateOutfitModal from "./createOutfitModal";

const useOutfitEntryStyles = createUseStyles({
  label: {
    marginBottom: 0,
    fontSize: '12px',
    fontWeight: 700,
  },
  image: {
    maxWidth: '100px',
    width: '100%',
    margin: '0 auto',
    display: 'block',
  },
  gearWrapper: {
    position: 'relative',
    float: 'right',
    height: 0,
  },
  thumbnailWrapper: {

  },
  createdAtLabel: {
    marginBottom: '0',
    fontSize: '14px',
  },
  renameButton: {
    fontSize: '12px',
    margin: 0,
    cursor: 'pointer',
    lineHeight: 'normal',
    color: '#0055b3',
  },
});

const OutfitEntry = props => {
  const { outfits, setOutfits } = props;
  const s = useOutfitEntryStyles();
  const showCreated = getFlag('avatarPageOutfitCreatedAtAvailable', false);
  const [isRenaming, setIsRenaming] = useState(false);
  const [newName, setNewName] = useState(props.name);
  const [locked, setLocked] = useState(false);

  return <div className='col-3'>
    <div className={s.gearWrapper}>
      <GearDropdown boxDropdownRightAmount={0} options={[
        {
          name: 'Wear',
          onClick: () => {
            if (locked) return;
            wearOutfit({
              outfitId: props.id
            }).then(() => {
              window.location.reload();
            })
          },
        },
        {
          name: 'separator',
        },
        {
          name: isRenaming ? 'Close Rename' : 'Rename',
          onClick: () => {
            if (locked) return;
            setIsRenaming(!isRenaming);
          },
        },

        {
          name: 'Delete',
          onClick: () => {
            if (locked) return;
            deleteOutfit({
              outfitId: props.id,
            }).then(() => {
              setOutfits(outfits.filter(v => v.id !== props.id));
            })
          },
        }
      ]}></GearDropdown>
    </div>
    <div className={s.thumbnailWrapper}>
      {props.thumbnail && <img className={s.image} src={props.thumbnail.imageUrl}></img>}
    </div>
    {isRenaming ? <input type='text' value={newName} minLength={1} maxLength={25} onChange={(newValue) => {
      setNewName(newValue.currentTarget.value);
    }}></input> : <p className={s.label}>{props.name}</p>}
    {isRenaming && <p className={s.renameButton} onClick={() => {
      for (const item of outfits) {
        if (item.id === props.id) {
          item.name = newName;
          break;
        }
      }
      setOutfits([...outfits]);
      setIsRenaming(false);
      renameOutfit({
        outfitId: props.id,
        name: newName,
      })
    }}>Save</p>}
    {showCreated &&
      <>
        <p className={s.createdAtLabel}>Created:</p>
        <p className={s.createdAtLabel}>{dayjs(props.created).format('MM/D/YYYY')}</p>
      </>
    }
  </div>
}

const useOutfitStyles = createUseStyles({
  createButton: {
    fontSize: '14px',
    float: 'right',
    marginTop: '20px',
    width: 'auto',
  },
});

const Outfits = props => {
  const [outfits, setOufits] = useState(null);
  const [thumbnails, setThumbnails] = useState(null);
  const [showCreateOutfit, setShowCreateOutfit] = useState(false);
  const s = useOutfitStyles();
  const buttonStyles = useButtonStyles();
  const characterStore = CharacterCustomizationStore.useContainer();

  useEffect(() => {
    getOutfits({
      userId: characterStore.userId,
    }).then(d => {
      setOufits(d.data);
      multiGetOutfitThumbnails({
        userOutfitIds: d.data.map(v => v.id)
      }).then(res => {
        setThumbnails(res);
      }).catch(e => {
        console.error('outfit thumbnail error', e)
      })
    })
  }, []);

  if (outfits === null) return null;
  return <div className='row'>
    {showCreateOutfit && <CreateOutfitModal onClose={() => {
      setShowCreateOutfit(false);
    }}></CreateOutfitModal>}
    <div className='col-12'>
      <div className='row'>
        <div className='col-12'>
          <ActionButton className={s.createButton + ' ' + buttonStyles.continueButton} label='Create Outfit' onClick={() => {
            setShowCreateOutfit(true);
          }}></ActionButton>
        </div>
      </div>
      <div className='row mt-4'>
        {outfits && outfits.length === 0 ? <p>No outfits</p> : outfits && outfits.map((v) => {
          const thumb = thumbnails && thumbnails.find(x => x.targetId === v.id);
          return <OutfitEntry key={v.id} thumbnail={thumb} outfits={outfits} setOutfits={setOufits} {...v}></OutfitEntry>
        })}
      </div>
    </div>
  </div>;
}

export default Outfits;