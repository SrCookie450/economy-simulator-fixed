const PositionsPaging = props => {
  const {count, setStartId, startIdHistory, page, setPage, positions} = props;
  return <div className='w-100'>
    <p className='text-center cursor-pointer'>
      <span onClick={() => {
        if (page === 1) return;
        setStartId(0);
        setPage(1);
      }}>First </span>
      <span onClick={() => {
        if (page === 1) return;
        setStartId(startIdHistory.current[page-1]);
        setPage(page-1);
      }}>Previous </span>
      <span onClick={() => {
        if (!positions.length || positions.length < 20)
          return;
        setStartId(startIdHistory.current[page+1]);
        setPage(page+1);
      }}>Next </span>
      {/*<span>Last</span>*/}
    </p>
  </div>
}

export default PositionsPaging;