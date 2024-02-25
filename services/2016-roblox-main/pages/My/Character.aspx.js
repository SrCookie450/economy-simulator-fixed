import React from "react";
import { createUseStyles } from "react-jss";
import AdBanner from "../../components/ad/adBanner";
import CharacterPage from "../../components/characterCustomizerPage";
import MyAvatar from "../../components/characterCustomizerPage/components/avatar";
import CharacterCustomizationStore from "../../stores/characterPage";

const useCharacterPageStyles = createUseStyles({
  header: {
    fontWeight: 700,
    marginBottom: 0,
    marginTop: 0,
    fontSize: '30px',
  },
  characterContainer: {
    background: '#fff',
    padding: '4px 8px',
    overflow: 'hidden',
  },
});

const MyCharacterPage = props => {
  const s = useCharacterPageStyles();
  return <div className='container'>
    <AdBanner context='MyCharacterPage'/>
    <div className={s.characterContainer}>
      <div className='row mt-2'>
        <div className='col-12 ps-4 pe-4'>
          <h1 className={s.header}>Character Customizer</h1>
        </div>
      </div>
      <CharacterCustomizationStore.Provider>
        <CharacterPage/>
      </CharacterCustomizationStore.Provider>
    </div>
  </div>
}

export default MyCharacterPage;