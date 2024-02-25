import useCardStyles from "../../userProfile/styles/card";
import useFormStyles from "../styles/forms";
import Subtitle from "./subtitle";
import {logoutFromAllOtherSessions} from "../../../services/auth";

const Security = props => {
  const cardStyles = useCardStyles();
  const s = useFormStyles();
  return <div className='row'>
    <div className='col-12 mt-2'>
      <Subtitle>Secure Sign Out</Subtitle>

      <div className={cardStyles.card + ' p-3'}>
        <div className='row'>
          <div className='col-10 col-lg-6'>
            <p>Sign out of all other sessions</p>
          </div>
          <div className='col-2 col-lg-6'>
            <button className={s.saveButton + ' float-right'} onClick={() => {
              logoutFromAllOtherSessions().then(() => {
                window.location.reload();
              })
            }}>Sign Out</button>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default Security;