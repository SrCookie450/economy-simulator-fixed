import React from 'react';
import MyDashboard from '../components/myDashboard';
import DashboardStore from '../components/myDashboard/stores/dashboardStore';
import Theme2016 from '../components/theme2016';

export default function AuthenticatedHomePage() {
  return <Theme2016>
    <DashboardStore.Provider>
      <MyDashboard></MyDashboard>
    </DashboardStore.Provider>
  </Theme2016>
}

export const getStaticProps = () => {
  return {
    props: {
      title: 'Home - ROBLOX',
    },
  }
}