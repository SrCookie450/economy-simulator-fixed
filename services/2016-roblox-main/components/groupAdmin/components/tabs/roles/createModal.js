import {useModalStyles} from '../../../../catalogDetailsPage/components/buyItemModal';
import authentication from "../../../../../stores/authentication";
import ActionButton from "../../../../actionButton";
import useButtonStyles from "../../../../../styles/buttonStyles";
import Robux from "../../../../catalogDetailsPage/components/robux";
import {useState} from "react";
import {createRole} from "../../../../../services/groups";

const CreateRoleModal = props => {
  const btnStyles = useButtonStyles();
  const s = useModalStyles();
  const auth = authentication.useContainer();
  const [locked, setLocked] = useState(false);
  const [feedback, setFeedback] = useState(null);

  const [name, setName] = useState('');
  const [desc, setDesc] = useState('');
  const [rank, setRank] = useState('');

  const AfterTransactionBalance = () => {
    const amt = auth.robux - 25;
    return <p className={s.footerText}>Your balance after this transaction will be R${amt.toLocaleString()} robux.</p>
  }

  return <div className={s.modalBg}>
    <div className={s.modalWrapper}>
      <h3 className={s.title}>
        Create New Roleset
      </h3>
      <div className={s.innerSection} style={{height: '230px'}}>
        <div className='row'>
          <div className='col-3'>

          </div>
          <div className='col-9'>

          </div>
        </div>
        <div className='row mt-4'>
          <div className='col-12'>
            {
              feedback ? <p className='text-danger fw-bold text-center'>{feedback}</p> : <p className='fw-bold text-center'>Purchasing a new RoleSet will cost <Robux inline={true}>25</Robux>.</p>

            }


            <div className='row'>
              <div className='col-3 pe-0'>
                <p className='text-end fw-bold'>Name:</p>
              </div>
              <div className='col-9 ps-1'>
                <input type='text' value={name} onChange={e => {
                  setName(e.currentTarget.value);
                }} />
              </div>
            </div>
            <div className='row'>
              <div className='col-3 pe-0'>
                <p className='text-end fw-bold'>Description:</p>
              </div>
              <div className='col-9 ps-1'>
                <input type='text' value={desc} onChange={e => {
                  setDesc(e.currentTarget.value);
                }} />
              </div>
            </div>

            <div className='row'>
              <div className='col-3 pe-0'>
                <p className='text-end fw-bold'>Rank (1-254):</p>
              </div>
              <div className='col-9 ps-1'>
                <input type='text' value={rank} onChange={e => {
                  setRank(e.currentTarget.value);
                }} />
              </div>
            </div>

          </div>


                <div className='col-8 offset-2'>
                  <div className='row'>
                    <div className='col-7'>
                      <ActionButton disabled={locked} label={'Purchase'} className={btnStyles.buyButton} onClick={(e) => {
                        e.preventDefault();
                        setLocked(true);
                        createRole({
                          groupId: props.groupId,
                          name,
                          description: desc,
                          rank: parseInt(rank, 10),
                        }).then(d => {
                          window.location.reload();
                        }).catch(e => {
                          setFeedback(e.message);
                        }).finally(() => {
                          setLocked(false);
                        })
                      }}/>
                    </div>
                    <div className='col-5'>
                      <ActionButton disabled={locked} label='Cancel' className={btnStyles.cancelButton} onClick={(e) => {
                        e.preventDefault();
                        props.setModal(null);
                      }}/>
                    </div>
                  </div>
                </div>

          <div className='col-12'>
            <AfterTransactionBalance />
          </div>

        </div>
      </div>
    </div>
  </div>
}

export default CreateRoleModal;