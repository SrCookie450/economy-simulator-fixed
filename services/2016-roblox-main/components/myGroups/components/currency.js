import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss"
import { abbreviateNumber } from "../../../lib/numberUtils";
import { getRobuxGroup } from "../../../services/economy";
import Robux from "../../catalogDetailsPage/components/robux";
import OldCard from "./oldCard"

const useStyles = createUseStyles({
  inline: {
    display: 'inline-block',
  },
})

const Currency = props => {
  const s = useStyles();
  const [robux, setRobux] = useState(null);
  useEffect(() => {
    getRobuxGroup({
      groupId: props.groupId,
    }).then(d => {
      setRobux(d.robux);
    })
  }, [props]);

  if (robux === null) return null
  return <OldCard>
    <div className={s.inline}>
      <div className='mt-1 mb-1'>
        <p className='mb-0 fw-600 font-size-16'>Funds: <Robux inline={true}>{abbreviateNumber(robux)}</Robux> </p>
      </div>
    </div>
  </OldCard>
}

export default Currency;