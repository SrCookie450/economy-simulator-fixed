import { useEffect, useState } from "react"
import { createUseStyles } from "react-jss"
import {getItemUrl, getRecommendations, itemNameToEncodedName} from "../../../services/catalog"
import CreatorLink from "../../creatorLink"
import ItemImage from "../../itemImage"
import Link from "../../link";

const useEntryStyles = createUseStyles({
  creator: {
    fontSize: '12px',
    color: '#999',
    marginTop: '2px',
  },
  image: {
    maxWidth: '110px',
    display: 'block',
    margin: '0 auto',
  },
})

const RecommendationEntry = props => {
  const s = useEntryStyles();
  return <div className='col-4 col-lg-2'>
    <div className={s.image}>
      <ItemImage id={props.id}/>
    </div>
    <p className='mb-0 text-center'>
      <Link href={getItemUrl({assetId: props.id, name: props.name})}>
        <a>
          {props.name}
        </a>
      </Link>
    </p>
    <p className={`${s.creator} mb-0 text-center`}>

      Creator: <CreatorLink id={props.creatorId} type={props.creatorType} name={props.creatorName}/>
    </p>
  </div>
}

const useRecommenndationStyles = createUseStyles({
  row: {
    '@media (min-width: 500px)': {
      '& >div': {
        width: '20%',
      },
    }
  }
});

/**
 * Recommendations based off the {assetId}
 * @param {{assetId: number; assetType: number;}} props 
 */
const Recommendations = props => {
  const s = useRecommenndationStyles();
  const [recommendations, setRecommendations] = useState(null);

  useEffect(() => {
    getRecommendations({
      assetId: props.assetId,
      assetTypeId: props.assetType,
      limit: 10,
    }).then(result => {
      setRecommendations(result.data);
    })
  }, [props.assetId])

  return <div className={`row ${s.row}`}>
    {
      recommendations && recommendations.map((v) => {
        return <RecommendationEntry key={v.item.assetId} id={v.item.assetId} name={v.item.name} creatorId={v.creator.creatorId} creatorType={v.creator.creatorType} creatorName={v.creator.name}/>
      })
    }
  </div>
}

export default Recommendations;