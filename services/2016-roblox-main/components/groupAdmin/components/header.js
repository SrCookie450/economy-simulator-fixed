import groupAdminStore from "../stores/groupAdminStore";
import Link from "../../link";
import styles from './header.module.css';
import Robux from "../../catalogDetailsPage/components/robux";
import Tickets from "../../tickets";

const Header = props => {
  const store = groupAdminStore.useContainer();
  return <div className='row'>
    {
      store.info ? <div className='col-6'>
        <h3>{store.info.name}</h3>
        <p className='fw-600 mb-0'>
          <span className={styles.labelName}>Owned by: </span>
          {
            store.info.owner ? <Link href={`/users/${store.info.owner.userId}/profile`}>
              <a className='fst-italic'>
                {
                  store.info.owner.username
                }
              </a>
            </Link> : <span>Nobody!</span>
          }
        </p>

        {
          store.funds !== null ? <p className='fw-600 mb-0'>
            <span className={styles.labelName}>Group Funds: </span>
            <Robux inline={true}>{store.funds.robux}</Robux>
            {
              store.funds.tickets !== undefined ? <span className='ms-2'><Tickets>{store.funds.tickets}</Tickets></span> : null
            }
          </p> : null
        }
      </div> : null
    }

    <div className='col-12'>
      {
        store.feedback ? <p className={styles.feedbackBox}>{store.feedback}</p> : null
      }
      <div className={styles.line + ' pt-3 mb-2'} />
    </div>
  </div>
}

export default Header;