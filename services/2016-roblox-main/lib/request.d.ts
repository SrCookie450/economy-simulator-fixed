import { AxiosResponse } from "axios";

export default (method: string, url: string, data?: any) => Promise<AxiosResponse<any>>();
export const getFullUrl = (service: string, url: string) => string;
export const getBaseUrl = () => string;
export const getUrlWithProxy = (url: string) => string;