import VerticalSelector from "../../verticalSelector";
import groupAdminStore from "../stores/groupAdminStore";
import buttonStyles from '../../../styles/buttons.module.css';
import Link from "../../link";
import Members from "./tabs/members";
import GroupInfo from "./tabs/groupInfo";
import GroupSettings from "./tabs/settings";
import GroupRevenue from "./tabs/revenue";
import GroupRoles from "./tabs/roles";
import Payout from "./tabs/payout";

const Content = props => {
  const store = groupAdminStore.useContainer();
  const options = [
    'Members',
    'Group Info',
    'Settings',
    'Relationships',
    'Roles',
    'Revenue',
    'Payouts',
  ];

  return <div className='row'>
    <div className='col-2'>
      <VerticalSelector options={options.map(v => {
        return {
          name: v,
          url: undefined,
          onClick: e => {
            e.preventDefault();
            store.setTab(v);
          },
        }
      })} selected={store.tab} />

      <Link href='/My/Groups.aspx'>
        <a className={buttonStyles.legacyButton + ' w-100 mt-4 d-block text-center'}>Back To My Groups</a>
      </Link>
    </div>
    {
      (store.groupId && store.info) ? <div className='col-10'>
        {
          store.tab === 'Members' ? <Members groupId={store.groupId} /> : null
        }
        {
          store.tab === 'Group Info' ? <GroupInfo groupId={store.groupId} info={store.info} /> : null
        }
        {
          store.tab === 'Settings' ? <GroupSettings groupId={store.groupId} info={store.info} /> : null
        }
        {
          store.tab === 'Roles' ? <GroupRoles groupId={store.groupId} info={store.info} /> : null
        }
        {
          store.tab === 'Revenue' ? <GroupRevenue groupId={store.groupId} info={store.info} /> : null
        }
        {
          store.tab === 'Payouts' ? <Payout groupId={store.groupId} /> : null
        }
      </div> : null
    }
  </div>
}

export default Content;