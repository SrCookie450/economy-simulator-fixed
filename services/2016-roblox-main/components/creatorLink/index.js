import {itemNameToEncodedName} from "../../services/catalog";
import Link from "../link";

/**
 * Creator link
 * @param {{type: string | number; id: number; name: string;}} props 
 * @returns 
 */
const CreatorLink = (props) => {
  const url = (props.type === 'User' || props.type === 1) ? '/users/' + props.id + "/profile" : '/My/Groups.aspx?gid=' + props.id;
  return <Link href={url}>
    <a>
      {props.name}
    </a>
  </Link>
}

export default CreatorLink;