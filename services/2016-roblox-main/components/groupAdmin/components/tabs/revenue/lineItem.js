import MyTransactionsTable from "../../../../myMoney/components/myTransactionsTable";

const LineItem = props => {
  return <MyTransactionsTable creatorType='Group' creatorId={props.groupId} hideTransactionTypeSelector={true} />
}

export default LineItem;