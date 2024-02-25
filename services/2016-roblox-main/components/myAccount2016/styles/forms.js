import { createUseStyles } from "react-jss";

const useFormStyles = createUseStyles({
  accountInfoLabel: {
    marginBottom: '6px',
    color: '#c3c3c3',
    fontSize: '15px',
  },
  accountInfoValue: {
    color: '#666',
  },
  descInput: {
    width: '100%',
    borderRadius: '4px',
    padding: '6px 8px',
    border: '1px solid #c3c3c3',
  },
  select: {
    borderRadius: '2px',
    fontSize: '16px',
  },
  disabled: {
    background: 'white!important',
    color: '#c3c3c3',
    '&:focus': {
      color: '#c3c3c3',
      boxShadow: 'none',
    },
  },
  fakeInput: {
    height: '100%',
    overflow: 'hidden',
  },
  saveButtonWrapper: {
    float: 'right',
    marginTop: '16px',
  },
  saveButton: {
    background: 'white',
    border: '1px solid #c3c3c3',
    borderRadius: '4px',
    fontSize: '16px',
    padding: '4px 8px',
    cursor: 'pointer',
  },
  genderUnselected: {
    color: '#c3c3c3',
    cursor: 'pointer',
  },
});

export default useFormStyles;