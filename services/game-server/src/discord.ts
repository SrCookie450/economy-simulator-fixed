import axios from "axios";
const sleep = (ns: number) => new Promise(resolve => setTimeout(resolve, ns));
interface MessageEntry {
	content: string;
	url: string;
}

let queue: MessageEntry[] = [];

const processQueue = async () => {
	const entry = queue.shift();
	if (!entry) return;
	if (typeof entry.content !== 'string') return;
	if (typeof entry.url !== 'string') return;
	if (entry.content.length > 1950) {
		entry.content = entry.content.substring(0, 1950);
	}

	const result = await axios.post(entry.url, {
		content: entry.content,
	}, {
		validateStatus(status) {
			return true;
		},
	});
	if (result.status === 429) {
		console.log('discord err, too many requests');
		const retryStr = result.headers['retry-after'];
		if (retryStr) {
			const i = parseInt(retryStr, 10);
			if (!Number.isNaN(i) && i > 0.5 && i < 30) {
				console.log('discord sleep for',i,'seconds');
				await sleep(i * 1000);
				queue.push(entry);
				return;
			}
		}
		console.log('429 but unsure how long to wait, wait 5s');
		await sleep(5 * 1000);
		queue.push(entry);
		return
	}
	if (!(result.status > 199 && result.status < 300)) {
		console.log('unknown status',result.statusText);
		await sleep(5*1000);
		queue.push(entry);
		return;
	}

}

const runQueue = async () => {
	while (true) {
		try {
			await processQueue();
		}catch(e) {
			console.error('[error] could not send message',e);
		}
		await sleep(5000);
	}
}
runQueue();

export const addMessage = (url: string, content: string)=> {
	queue.push({
		content,
		url
	});
}

