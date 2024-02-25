import {useRouter} from "next/router";

// Client-side only version of useRouter().query that will return correct result on first render.
export const getQueryParams = () => {
  if (typeof window === 'undefined') {
    return useRouter().query;
  }

  let queryParams = window.location.search;
  if (typeof queryParams !== 'string' || queryParams.length === 0 || queryParams.indexOf('=') === -1) return {};
  if (queryParams.startsWith('?'))
    queryParams = queryParams.substring(1);

  let kv = {};
  let split = queryParams.split('=');
  for (let i = 0; i < split.length; i++) {
    if (i%2===0) continue;
    let k = split[i-1];
    let v = split[i];
    kv[decodeURIComponent(k)] = decodeURIComponent(v);
  }

  return kv;
}