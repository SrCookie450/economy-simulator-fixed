import { createUseStyles } from "react-jss";
import { acceptTrade, declineTrade } from "../../../services/trades";
import AuthenticationStore from "../../../stores/authentication";
import useButtonStyles from "../../../styles/buttonStyles";
import ActionButton from "../../actionButton";
import TradeStore from "../stores/tradeStore";

const acceptFeedbackMessage = `You have accepted {username}'s trade request. The trade is now being processed by our system.`;

const useTradeButtonStyles = createUseStyles({
  acceptWrapper: {
    width: '125px',
    margin: '0 auto',
    display: 'block',
  },
  acceptButton: {
    paddingTop: '6px',
    paddingBottom: '6px',
    fontSize: '24px',
  },
});

const TradeButtons = props => {
  const { trade } = props;
  const auth = AuthenticationStore.useContainer();
  const tradeStore = TradeStore.useContainer();
  const s = useTradeButtonStyles();
  const buttonStyles = useButtonStyles();

  const canAccept = trade.status === 'Open' && trade.user.id !== auth.userId;
  const canDecline = canAccept || trade.user.id === auth.userId;
  const canCounter = canAccept;
  const canOk = !canAccept;
  const labelDecline = canAccept ? 'Decline' : 'Cancel';

  return <div className='row mt-4'>
    <div className='col-8 offset-2'>
      <div className='row mx-auto'>
        {canOk && <div className={'col-4 mx-auto'}>
          <ActionButton label='OK' className={buttonStyles.continueButton + ' ' + s.acceptButton} onClick={() => {
            tradeStore.setSelectedTrade(null);
          }}></ActionButton>
        </div>}
        {canAccept && <div className={'col-4 mx-auto'}>
          <ActionButton label='Accept' className={buttonStyles.continueButton + ' ' + s.acceptButton} onClick={() => {
            tradeStore.setSelectedTrade(null);
            acceptTrade({
              tradeId: trade.id,
            }).then(() => {
              tradeStore.setFeedback(acceptFeedbackMessage.replace(/{username}/g, trade.user.name));
              tradeStore.refershTrades();
            }).catch(e => {
              tradeStore.setFeedback('Could not accept ' + trade.user.name + '\'s trade. Please try again.');
            })
          }}></ActionButton>
        </div>}
        {canCounter && <div className={'col-4 mx-auto ps-0 pe-0'}>
          <ActionButton label='Counter' className={buttonStyles.continueButton + ' ' + s.acceptButton} onClick={() => {
            window.open("/Trade/TradeWindow.aspx?TradeSessionId=" + trade.id + "&TradePartnerID=" + trade.user.id, "_blank", "scrollbars=0, height=608, width=914");
          }}></ActionButton>
        </div>}
        {canDecline && <div className={'col-4 mx-auto'}>
          <ActionButton label={labelDecline} className={buttonStyles.cancelButton + ' ' + s.acceptButton} onClick={() => {
            tradeStore.setFeedback(null);
            tradeStore.setSelectedTrade(null);
            declineTrade({
              tradeId: trade.id,
            }).then(() => {
              tradeStore.refershTrades();
            })
          }}></ActionButton>
        </div>}
      </div>
    </div>
  </div>
}

export default TradeButtons;