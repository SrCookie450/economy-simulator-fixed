import { useRouter } from "next/dist/client/router";
import Develop from "../components/develop";
import t from "../lib/t";

const DevelopPage = props => {
  const router = useRouter();
  const id = t.string(router.query['View']);

  return <Develop id={parseInt(id, 10) || 0}/>
}

DevelopPage.getInitialProps = () => {
  return {
    title: 'Develop - ROBLOX',
  }
}

export default DevelopPage;
