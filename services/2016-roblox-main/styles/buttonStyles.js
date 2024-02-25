import { createUseStyles } from "react-jss";

const useButtonStyles = createUseStyles({
  buyButton: {
    width: '100%',
    paddingTop: '5px',
    paddingBottom: '5px',
    background: 'linear-gradient(0deg, rgba(0,113,0,1) 0%, rgba(64,193,64,1) 100%)', // 40c140 #007100
    '&:hover': {
      background: 'linear-gradient(0deg, rgba(71,232,71,1) 0%, rgba(71,232,71,1) 100%)', // 47e847 02a101
    },
  },
  normal: {
    width: 'auto!important',
  },
  cancelButton: {
    width: '100%',
    paddingTop: '5px',
    paddingBottom: '5px',
    background: 'linear-gradient(0deg, rgba(69,69,69,1) 0%, rgba(140,140,140,1) 100%)', // top #8c8c8c bottom #454545
    border: '1px solid #404041',
    '&:hover': {
      'background': 'grey!important',
    },
  },
  continueButton: {
    width: '100%',
    paddingTop: '5px',
    paddingBottom: '5px',
    background: 'linear-gradient(0deg, rgba(8,79,192,1) 0%, rgba(5,103,234,1) 100%)', // #0567ea #084fc0
    border: '1px solid #084ea6',
    '&:hover': {
      background: 'linear-gradient(0deg, rgba(2,73,198,1) 0%, rgba(7,147,253,1) 100%); ',
    },
  },
  badPurchaseRow: {
    marginTop: '70px',
  },
});

export default useButtonStyles