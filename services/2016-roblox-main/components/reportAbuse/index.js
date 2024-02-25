import { createUseStyles } from "react-jss";

const useReportStyles = createUseStyles({
  text: {
    marginBottom: 0,
    fontSize: '10px',
    marginTop: '15px',
    '&:hover > a': {
      color: '#F00',
    },
    '&:hover > span': {
      backgroundImage: `url("/img/abuse.png")`,
    },
  },
  link: {
    color: '#F99',
    paddingLeft: '2px',
  },
  image: {
    height: '13px',
    width: '14px',
    display: 'block',
    float: 'left',
  },
})

/**
 * ReportAbuse button
 * @param {{assetId?: number; id?: number; url?: string; type?: string;}} props 
 * @returns 
 */
const ReportAbuse = props => {
  const url = props.url || window.location.href;
  const s = useReportStyles();
  return <p className={s.text}>
    <span className={s.image}></span>
    <a className={s.link} href={`/abusereport/${props.type || 'asset'}?id=${props.assetId || props.id}&RedirectUrl=${encodeURIComponent(url)}`}>
      Report Abuse
    </a>
  </p>
}

export default ReportAbuse;