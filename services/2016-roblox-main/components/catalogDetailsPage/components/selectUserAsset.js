import { createUseStyles } from "react-jss";

const useSelectionStyles = createUseStyles({
  inlineSelect: {
    display: 'inline-block'
  },
  select: {},
  inlineRow: {},
  buttonRow: {
    marginTop: '20px',
  },
  priceInput: {
    width: '125px',
  },
});

const SelectUserAsset = props => {
  const { locked, selected, userAssets, setSelected } = props;
  const s = useSelectionStyles();
  return <div className={s.inlineRow}>
    <div className={s.inlineSelect}>
      <p className='mb-0'>Serial Number:</p>
    </div>
    <div className={s.inlineSelect}>
      <select disabled={locked} className={s.select} value={selected || userAssets[0].userAssetId} onChange={(nv) => {
        setSelected(parseInt(nv.currentTarget.value, 10));
      }}>
        {userAssets.map(v => {
          return <option key={v.userAssetId} value={v.userAssetId}>{v.serialNumber || 'N/A'}</option>
        })}
      </select>
    </div>
  </div>
}

export default SelectUserAsset;