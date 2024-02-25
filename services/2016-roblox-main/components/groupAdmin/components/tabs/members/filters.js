import {useState} from "react";
import buttonStyles from '../../../../../styles/buttons.module.css';

const MemberFilters = props => {
  const [query, setQuery] = useState('');
  const [rankFilter, setRankFilter] = useState('Filter By Rank');

  return <div>
    <div className='d-inline-block'>
      <input type='text' value={query} onChange={v => {
        setQuery(v.currentTarget.value);
      }} />
    </div>
    <div className='d-inline-block ms-2'>
      <select value={rankFilter} onChange={e => {
        if (e.currentTarget.value === 'Filter By Rank') {
          props.setRoleFilter(null);
        }else{
          props.setRoleFilter(parseInt(e.currentTarget.value, 10));
        }
        setRankFilter(e.currentTarget.value);
      }}>
        <option value='Filter By Rank'>Filter By Rank</option>
        {
          props.roles ? props.roles.filter(a => a.rank !== 0).map(v => {
            return <option value={v.id} key={v.id}>{v.name} ({v.memberCount.toLocaleString()})</option>
          }) : null
        }
      </select>
    </div>
    <div className='d-inline-block ms-2'>
      <button className={buttonStyles.legacyButton} onClick={() => {

      }}>Search</button>
    </div>
    <div className='d-inline-block ms-2'>
      <button className={buttonStyles.legacyButton} onClick={() => {
        setRankFilter('Filter By Rank');
        props.setRoleFilter(null);
      }}>Show All Members</button>
    </div>
  </div>
}

export default MemberFilters;