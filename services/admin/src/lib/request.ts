import axios from 'axios';
const baseUrl: string = '/admin-api/api/';
let goodCsrf = '';
const client = axios.create({
	baseURL: baseUrl,
	maxRedirects: 0,
});
client.interceptors.request.use(ok => {
	if (!ok.headers) {
		ok.headers = {};
	}
	ok.headers['x-csrf-token'] = goodCsrf;
	return ok;
})
client.interceptors.response.use(undefined, (e) => {
	if (e.isAxiosError && e.response && e.response.headers) {
		if (e.response.headers['x-csrf-token']) {
			goodCsrf = e.response.headers['x-csrf-token'];
			return client.request(e.config);
		}
        if (typeof e.response.data === 'string') {
            e.message = e.response.data;
        }else if (typeof e.response.data === 'object') {
            if (e.response.data.errors && e.response.data.errors.length) {
                let msg = e.response.data.errors[0].message;
                if (msg === 'Unauthorized') {
                    e.message = 'You do not have the proper permissions to perform this action.';
                }else{
                    e.message = msg;
                }
            }
        }
	}

	return Promise.reject(e);
})
export default client;