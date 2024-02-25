import React from "react";
import { createUseStyles } from "react-jss";
import { setInventoryPrivacy, setTradePrivacy, setTradeValue } from "../../../services/accountSettings";
import Selector from "../../selector";
import useCardStyles from "../../userProfile/styles/card";
import MyAccountStore from "../stores/myAccountStore"
import Subtitle from "./subtitle";

const useStyles = createUseStyles({
  label: {
    color: '#c3c3c3',
    marginBottom: 0,
    fontSize: '15px',
  },
})

const PrivacySettings = props => {
  const store = MyAccountStore.useContainer();

  const cardStyles = useCardStyles();
  const s = useStyles();

  const inventoryPrivacy = [
    {
      name: 'All Users',
      value: 'AllUsers',
    },
    {
      name: 'Friends, Users I follow, and Followers',
      value: 'FriendsFollowingAndFollowers',
    },
    {
      name: 'Friends, and Users I Follow',
      value: 'FriendsAndFollowing',
    },
    {
      name: 'Friends',
      value: 'Friends',
    },
    {
      name: 'No one',
      value: 'NoOne',
    },
  ];


  const defaultPrivacySettings = [
    {
      name: 'All Users',
      value: 'All',
    },
    {
      name: 'Friends, Users I follow, and Followers',
      value: 'Followers',
    },
    {
      name: 'Friends, and Users I Follow',
      value: 'Following',
    },
    {
      name: 'Friends',
      value: 'Friends',
    },
    {
      name: 'No one',
      value: 'NoOne',
    },
  ];

  if (!store.inventoryPrivacy || !store.tradePrivacy || !store.tradeFilter) return null;

  return <div className='row'>
    <div className='col-12 mt-2'>
      <Subtitle>Privacy Setting</Subtitle>
      <div className={cardStyles.card + ' p-3'}>


        <p className={s.label}>Who can message me:</p>
        <Selector onChange={newPrivacy => {

        }} options={defaultPrivacySettings}></Selector>


        <p className={s.label + ' mt-2'}>Who can Invite me to VIP Servers:</p>
        <Selector onChange={newPrivacy => {

        }} options={defaultPrivacySettings}></Selector>


        <p className={s.label + ' mt-2'}>Who can follow me into the game:</p>
        <Selector onChange={newPrivacy => {

        }} options={defaultPrivacySettings}></Selector>


        <p className={s.label + ' mt-2'}>Who can see my inventory:</p>
        <Selector value={store.inventoryPrivacy} onChange={newPrivacy => {
          store.setInventoryPrivacy(newPrivacy.value);
          setInventoryPrivacy({
            newPrivacy: newPrivacy.value,
          });
        }} options={inventoryPrivacy}></Selector>


        <p className={s.label + ' mt-2'}>Who can trade with me:</p>
        <Selector value={store.tradePrivacy} onChange={newPrivacy => {
          setTradePrivacy({
            newPrivacy: newPrivacy.value,
          });
        }} options={defaultPrivacySettings}></Selector>


        <p className={s.label + ' mt-2'}>Trade quality filter:</p>
        <Selector value={store.tradeFilter} onChange={newValue => {
          store.setTradeFilter(newValue.value);
          setTradeValue({
            newValue: newValue.value,
          });
        }} options={[
          {
            name: 'None',
            value: 'None',
          },
          {
            name: 'Low',
            value: 'Low',
          },
          {
            name: 'Medium',
            value: 'Medium',
          },
          {
            name: 'High',
            value: 'High',
          }
        ]}></Selector>
      </div>
    </div>
  </div>
}

export default PrivacySettings;