import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import { getMyEmail, getMySettingsJson } from "../../../services/accountSettings";

/**
 * @type {null | 'CHANGE_EMAIL' | 'CHANGE_PASSWORD' | 'CHANGE_USERNAME' | 'MODAL_OK'}
 */
const modalTypes = null;

const MyAccountStore = createContainer(() => {
  const [tab, setTab] = useState('Account Info');
  const [email, setEmail] = useState(null);
  const [emailVerified, setEmailVerified] = useState(0); // 0 = no, 1 = pending, 2 = verified
  const [description, setDescription] = useState(null);
  const [gender, setGender] = useState(1); // 1 = unknown, 2 = male, 3 = female
  const [inventoryPrivacy, setInventoryPrivacy] = useState(null);
  const [tradePrivacy, setTradePrivacy] = useState(null);
  const [tradeFilter, setTradeFilter] = useState(null);
  /**
   * @type {[typeof modalTypes, import("react").Dispatch<typeof modalTypes>]}
   */
  const [modal, setModal] = useState(null);
  const [modalMessage, setModalMessage] = useState(null);

  useEffect(() => {
    getMyEmail().then(d => {
      setEmail(d.emailAddress);
      if (d.verified) {
        setEmailVerified(2);
      } else {
        setEmailVerified(1);
      }
    })
  }, []);

  return {
    tab,
    setTab,

    email,
    setEmail,

    emailVerified,
    setEmailVerified,

    description,
    setDescription,

    modal,
    setModal,
    modalMessage,
    setModalMessage,

    gender,
    setGender,

    tradePrivacy,
    setTradePrivacy,

    inventoryPrivacy,
    setInventoryPrivacy,

    tradeFilter,
    setTradeFilter,
  }
});

export default MyAccountStore;