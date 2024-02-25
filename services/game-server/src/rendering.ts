import sharp = require("sharp");

// Poor man's anti-aliasing
export const resolutionMultiplier = {
	game: 4,
	asset: 1,
	userThumbnail: 4,
	userHeadshot: 4,
};

let uploadCallbacks: Record<string, Array<(args: any) => void>> = {};
export const doesCallbackExist = (id: string): boolean => {
	return uploadCallbacks[id] !== undefined;
}
export const getUploadCallbacks = () => uploadCallbacks;
export const awaitResult = (key: string): Promise<void> => {
	return new Promise((res, rej) => {
		let cb = async () => {
			clearTimeout(interval);
			res();
		}
		let interval = setTimeout(() => {
			rej('Timeout');
			if (!uploadCallbacks[key])
				return;
			uploadCallbacks[key] = uploadCallbacks[key].filter(v => v !== cb);
			delete uploadCallbacks[key];
		}, 2 * 60 * 1000);
		if (!uploadCallbacks[key]) {
			uploadCallbacks[key] = [];
		}
		uploadCallbacks[key].push(cb);
	});
}

let shutdownTimer: any;
export const getResult = (key: string, upscaleAmount: number): Promise<any> => {
	return new Promise((res, rej) => {
		let interval = setTimeout(() => {
			rej('Timeout');
			delete uploadCallbacks[key];
		}, 2 * 60 * 1000);
		if (!uploadCallbacks[key]) {
			uploadCallbacks[key] = [];
		}
		uploadCallbacks[key].push(async (data) => {
			clearTimeout(interval);
			if (typeof data.thumbnail === 'string') {
				if (!shutdownTimer) {
					console.log('[info] creating shutdown timer');
					shutdownTimer = setTimeout(() => {
						process.exit(0);
					}, 24 * 60 * 60 * 1000);
				}
				let originalImage = await sharp(Buffer.from(data.thumbnail, 'base64')).metadata();
				if (typeof originalImage.width !== 'number' || typeof originalImage.height !== 'number') {
					throw new Error('Bad image metadata: ' + JSON.stringify(originalImage));
				}
				let image = await sharp(Buffer.from(data.thumbnail, 'base64')).resize(Math.trunc(originalImage.width / upscaleAmount), Math.trunc(originalImage.height / upscaleAmount))
				
				.png({
					compressionLevel: 9,
					quality: 99,
					effort: 10,
				})
				
				.toBuffer();
				data.thumbnail = image.toString('base64');
			}

			res(data);
		})
	});
}