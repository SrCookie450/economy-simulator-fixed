export const getStatusText = (tradeData, tradeType, authenticatedUserId) => {
  switch (tradeData.status) {
    case 'Open':
      if (tradeData.user.id === authenticatedUserId || tradeType === 'outbound' || tradeType === 'sent') {
        let otherUser;
        if (tradeData.offers) {
          otherUser = tradeData.offers.find(v => v.user.id !== authenticatedUserId);
        } else {
          otherUser = tradeData;
        }
        return 'Pending approval from ' + otherUser.user.name;
      }
      return 'Pending approval from you';
    default:
      return tradeData.status;
  }
}