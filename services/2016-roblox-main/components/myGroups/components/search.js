import React from "react";
import { createUseStyles } from "react-jss"
import Button from "./button";
import OldCard from "./oldCard"

const useStyles = createUseStyles({
  inline: {
    display: 'inline-block',
    textAlign: 'center',
  },
  inlineWrapper: {
    textAlign: 'center',
  },
  searchGroupsInput: {
    minWidth: '300px',
  },
})

const SearchGroups = props => {
  const s = useStyles();
  return <OldCard>
    <form method='GET' action='/Search/Groups.aspx' autoComplete='off' className='mb-1'>
      <div className={s.inlineWrapper}>
        <div className={s.inline}>
          <input className={s.searchGroupsInput} type='text' placeholder='  Search All Groups' name='query' autoComplete='off'></input>
        </div>
        <div className={s.inline + ' ms-2'}>
          <Button>Search</Button>
        </div>
      </div>
    </form>
  </OldCard>
}

export default SearchGroups;