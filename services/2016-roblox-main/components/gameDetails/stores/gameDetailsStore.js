import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import { getGameMedia, multiGetPlaceDetails, multiGetUniverseDetails } from "../../../services/games";

const GameDetailsStore = createContainer(() => {
  const [details, setDetails] = useState(null);
  const [media, setMedia] = useState(null);
  const [placeDetails, setPlaceDetails] = useState(null);
  const [universeDetails, setUniverseDetails] = useState(null);
  const [servers, setServers] = useState(null);

  useEffect(() => {
    // reset our states, then get new details
    setMedia(null);
    setPlaceDetails(null);
    setUniverseDetails(null);

    if (!details) return;

    multiGetPlaceDetails({
      placeIds: [details.id],
    }).then(d => setPlaceDetails(d[0]));
  }, [details]);

  useEffect(() => {
    if (!placeDetails) return;
    multiGetUniverseDetails({
      universeIds: [placeDetails.universeId],
    }).then(d => {
      setUniverseDetails(d[0]);
      getGameMedia({
        universeId: d[0].id,
      }).then(setMedia);
    })
  }, [placeDetails]);

  return {
    details,
    setDetails,

    servers,
    setServers,

    placeDetails,

    universeDetails,

    media,
    setMedia,
  }
});

export default GameDetailsStore;