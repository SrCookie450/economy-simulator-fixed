import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss"
import { getUserRobloxBadges } from "../../../services/accountInformation";
import SmallButtonLink from "./smallButtonLink";
import Subtitle from "./subtitle";
import Link from "../../link";

const useBadgeStyles = createUseStyles({
  label: {
    width: '100%',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },
  imageWrapper: {
    border: '1px solid #b8b8b8',
    borderRadius: '4px',
    width: '100%',
    height: 'auto',
    overflow: 'hidden',
    minWidth: '142px',
  },
  buttonWrapper: {
    width: '100px',
    float: 'right',
  },
  badgeLink: {
    color: 'inherit',
  },
});
const RobloxBadges = props => {
  const s = useBadgeStyles();
  const [badges, setBadges] = useState(null);
  const [showAll, setShowAll] = useState(false);

  useEffect(() => {
    getUserRobloxBadges({ userId: props.userId }).then(setBadges);
  }, [props.userId]);

  if (!badges || !badges.length) return null;

  return <div className='row d-none d-lg-flex'>
    <div className='col-10'>
      <Subtitle>Roblox Badges ({badges.length})</Subtitle>
    </div>
    <div className='col-6 col-lg-2'>
      {badges.length > 6 &&
        <div className={s.buttonWrapper + ' mt-2'}>
          <SmallButtonLink onClick={(e) => {
            e.preventDefault();
            setShowAll(!showAll);
          }}>{showAll ? 'See Less' : 'See More'}</SmallButtonLink>
        </div>
      }
    </div>
    <div className='col-12'>
      <div className='card pt-4 pb-4 pe-4 ps-4'>
        <div className='row'>
          {
            badges.slice(0, showAll ? badges.length : 6).map((v, i) => {
              return <div className='col-4 col-lg-2' key={i}>
                <Link href='/Badges.aspx' className={s.badgeLink}>
                  <a>
                    <div className={s.imageWrapper}>
                      <span className={`icon-${v.name.toLowerCase().replace(/ /g, '-')}`}/>
                    </div>
                    <p className={`${s.label} mb-0 text-dark`}>{v.name}</p>
                  </a>
                </Link>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}

export default RobloxBadges;