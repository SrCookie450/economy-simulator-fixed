import React from "react";
import { createUseStyles } from "react-jss";
import ReportAbuse from "../../reportAbuse";
import UserProfileStore from "../stores/UserProfileStore";
import useCardStyles from "../styles/card";
import PreviousUsernames from "./previousUsernames";
import Subtitle from "./subtitle";

const useStyles = createUseStyles({
  body: {
    fontWeight: 300,
    marginBottom: 0,
    fontSize: '16px',
    padding: '15px 20px',
    whiteSpace: 'break-spaces',
  },
  report: {
    paddingBottom: '40px',
  },
  reportWrapper: {
    float: 'right',
  },
})

const Description = props => {
  const s = useStyles();
  const cardStyles = useCardStyles();
  const store = UserProfileStore.useContainer();
  return <div className='row'>
    <div className='col-12'>
      <Subtitle>About</Subtitle>
      <div className={cardStyles.card}>
        <p className={s.body}>
          {store.userInfo && store.userInfo.description}
        </p>
        <div className='divider-top me-4 ms-4'></div>
        <div className='row'>
          <div className='col-6'>
            <PreviousUsernames></PreviousUsernames>
          </div>
          <div className='col-6'>
            <div className={s.report + ' me-4'}>
              <div className={s.reportWrapper}>
                <ReportAbuse type='UserProfile' id={store.userId}></ReportAbuse>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
}

export default Description;