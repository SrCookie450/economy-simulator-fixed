import getFlag from "../lib/getFlag";
import request, { getBaseUrl, getFullUrl } from "../lib/request"
import { itemNameToEncodedName } from "./catalog";
const gamePage2015Enabled = getFlag('2015GameDetailsPageEnabled', false);
const csrEnabled = getFlag('clientSideRenderingEnabled', false);

export const getGameUrl = ({ placeId, name }) => {
  return `/games/${placeId}/${itemNameToEncodedName(name)}`;
}

export const getUserGames = ({ userId, cursor }) => {
  return request('GET', getFullUrl('games', `/v2/users/${userId}/games?cursor=${encodeURIComponent(cursor || '')}`)).then(d => d.data);
}

export const getGroupGames = ({ groupId, cursor }) => {
  return request('GET', getFullUrl('games', `/v2/groups/${groupId}/games?cursor=${encodeURIComponent(cursor || '')}`)).then(d => d.data);
}

export const getGameSorts = ({ gameSortsContext }) => {
  return request('GET', getFullUrl('games', `/v1/games/sorts?gameSortsContext=${encodeURIComponent(gameSortsContext || '')}`)).then(d => d.data)
}

export const getGameList = ({ sortToken, limit, genre = 0, keyword }) => {
  return request('GET', getFullUrl('games', `/v1/games/list?sortToken=${encodeURIComponent(sortToken)}&maxRows=${limit}&genre=${genre}&keyword=${keyword}`)).then(d => d.data)
}

export const getGameMedia = ({ universeId }) => {
  return request('GET', getFullUrl('games', `/v2/games/${universeId}/media`)).then(d => d.data.data);
}

export const launchGame = async ({ placeId }) => {
  const result = await request('GET', getBaseUrl() + '/game/get-join-script?placeId=' + encodeURIComponent(placeId));
  const toClick = result.data.joinUrl;
  const aTag = document.createElement('a');
  aTag.setAttribute('href', result.data.prefix + '' + result.data.joinScriptUrl);
  document.body.appendChild(aTag);
  aTag.click();
  // delay before deletion is required on some browsers, not sure why
  setTimeout(() => {
    aTag.remove();
  }, 1000);
}

export const multiGetPlaceDetails = ({ placeIds }) => {
  return request('GET', getFullUrl('games', `/v1/games/multiget-place-details?placeIds=${encodeURIComponent(placeIds.join(','))}`)).then(d => d.data);
}

export const multiGetUniverseDetails = ({ universeIds }) => {
  return request('GET', getFullUrl('games', `/v1/games?universeIds=${encodeURIComponent(universeIds.join(','))}`)).then(d => d.data.data);
}

export const getServers = ({ placeId, offset }) => {
  return request('GET', getBaseUrl() + `/games/getgameinstancesjson?placeId=${placeId}&startIndex=${offset}`).then(d => d.data);
}

export const multiGetGameVotes = ({universeIds}) => {
  return request('GET', getFullUrl('games', '/v1/games/votes?universeIds=' + encodeURIComponent(universeIds.join(',')))).then(d => d.data.data);
}

export const voteOnGame = ({universeId, isUpvote}) => {
  return request('PATCH', getFullUrl('games', '/v1/games/'+universeId+'/user-votes'), {
    vote: isUpvote,
  }).then(d => d.data.data);
}