import request, { getBaseUrl, getFullUrl } from "../lib/request"

// TODO: What is the documented replacement for this?
export const getAlert = () => {
  return request('GET', getFullUrl('api', '/alerts/alert-info')).then(d => d.data);
}
