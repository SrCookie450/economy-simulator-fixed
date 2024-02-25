import { useState } from "react";
import { createUseStyles } from "react-jss";
import CharacterCustomizationStore from "../../../stores/characterPage";

const useColorStyles = createUseStyles({
  head: {
    height: '50px',
    width: '60px',
    margin: '0 auto',
    display: 'block',
    cursor: 'pointer',
    border: '1px solid #dededd',
  },
  torso: {
    height: '110px',
    width: '120px',
    margin: '0 auto',
    display: 'block',
    marginTop: '10px',
    cursor: 'pointer',
    border: '1px solid #dededd',
  },
  leftArm: {
    height: '110px',
    width: '40px',
    margin: '0 auto',
    display: 'block',
    marginLeft: '-50px',
    cursor: 'pointer',
    zIndex: 99,
    border: '1px solid #dededd',
  },
  rightArm: {
    height: '110px',
    width: '40px',
    margin: '0 auto',
    display: 'block',
    marginTop: '-110px',
    marginLeft: '130px',
    cursor: 'pointer',
    zIndex: 99,
    border: '1px solid #dededd',
  },
  leftLeg: {
    width: 'calc(50% - 10px)',
    float: 'left',
    height: '110px',
    marginRight: '10px',
    cursor: 'pointer',
    border: '1px solid #dededd',
  },
  rightLeg: {
    width: 'calc(50% - 10px)',
    height: '110px',
    float: 'right',
    marginLeft: '10px',
    cursor: 'pointer',
    border: '1px solid #dededd',
  },
  header: {
    fontWeight: 400,
    fontSize: '24px',
    margin: 0,
  },
  legs: {
    margin: '10px auto 0 auto',
    display: 'block',
    width: '120px',
    height: '100%',
  },
  colorSelectorWrapper: {
    position: 'absolute',
    width: '200px',
    height: '300px',
    background: 'white',
    marginLeft: '10px',
    overflowY: 'auto',
    border: '1px solid #9e9e9e',
    padding: '5px 15px',
  },
  colorPaletteEntry: {
    height: '25px',
    width: '25px',
    margin: '0 auto',
    display: 'block',
    '&:hover': {
      border: '1px solid white',
      cursor: 'pointer',
    },
  },
  close: {
    cursor: 'pointer',
  },
  row: {
    marginTop: '20px',
  },
});

const Color = props => {
  const s = useColorStyles();
  const store = CharacterCustomizationStore.useContainer();
  const [colorMode, setColorMode] = useState(null);
  if (!store.colors || !store.rules || !store.rules.bodyColorsPalette) return null;

  const idToHex = (id) => {
    return store.rules.bodyColorsPalette.find(v => v.brickColorId === id).hexColor;
  }

  return <div className={`${s.row} row`}>
    <div className='col-12'>
      <h2 className={s.header}>Avatar Colors</h2>
    </div>
    {
      colorMode && <div className={s.colorSelectorWrapper}>
        <div className='row'>
          <div className='col-12'>
            <p className={'mb-1 ' + s.close} onClick={() => {
              setColorMode(null);
            }}>Close</p>
          </div>
          {
            store.rules.bodyColorsPalette.map(v => {
              return <div className='col-3 col-lg-2 ps-0 pe-0' key={v.brickColorId}>
                <div className={s.colorPaletteEntry} style={{ background: v.hexColor }} onClick={() => {
                  let obj = {
                    ...store.colors,
                  }
                  obj[colorMode.id] = v.brickColorId;
                  store.setColors(obj);
                  setColorMode(null);
                }}>

                </div>
              </div>
            })
          }
        </div>
      </div>
    }
    <div className='col-12'>
      <div className={s.head} style={{ backgroundColor: idToHex(store.colors.headColorId) }} onClick={(e) => {
        e.stopPropagation();
        setColorMode({
          id: 'headColorId',
          name: 'Head',
        });
      }}/>
      <div className={s.torso} style={{ backgroundColor: idToHex(store.colors.torsoColorId) }} onClick={(e) => {
        e.stopPropagation();
        setColorMode({
          id: 'torsoColorId',
          name: 'Torso',
        });
      }}>
        <div className={s.leftArm} style={{ backgroundColor: idToHex(store.colors.leftArmColorId) }} onClick={(e) => {
          e.stopPropagation();
          setColorMode({
            id: 'leftArmColorId',
            name: 'Left Arm',
          });
        }}/>
        <div className={s.rightArm} style={{ backgroundColor: idToHex(store.colors.rightArmColorId) }} onClick={(e) => {
          e.stopPropagation();
          setColorMode({
            id: 'rightArmColorId',
            name: 'Right Arm',
          });
        }}/>
      </div>
      <div className={s.legs}>
        <div className={s.leftLeg} style={{ backgroundColor: idToHex(store.colors.leftLegColorId) }} onClick={(e) => {
          e.stopPropagation();
          setColorMode({
            id: 'leftLegColorId',
            name: 'Left Leg',
          });
        }}/>
        <div className={s.rightLeg} style={{ backgroundColor: idToHex(store.colors.rightLegColorId) }} onClick={(e) => {
          e.stopPropagation();
          setColorMode({
            id: 'rightLegColorId',
            name: 'Right Leg',
          });
        }}/>
      </div>
    </div>
  </div>
}

export default Color;