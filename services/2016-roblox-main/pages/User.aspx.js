import { useRouter } from "next/dist/client/router";

const UserPage = props => {
  return null;
}


export async function getServerSideProps({ query, res, req }) {
  const userId = query['ID'];
  return {
    redirect: {
      destination: `/users/${userId}/profile`,
    },
    props: {}
  };
}


export default UserPage;