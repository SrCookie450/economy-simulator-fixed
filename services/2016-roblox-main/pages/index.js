import { useEffect } from "react"
import AuthenticationStore from "../stores/authentication"

const IndexPage = props => {
  const auth = AuthenticationStore.useContainer();
  useEffect(() => {
    if (auth.isAuthenticated) {
      window.location.href = '/home';
    }
  }, [auth.isAuthenticated]);

  return null;
}

export default IndexPage;