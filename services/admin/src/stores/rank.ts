import { get as getStore, Readable, Writable, writable } from "svelte/store";
interface IRankResponse {
	rank: {
		name: string,
		details: {
			isAdmin: boolean,
			isModerator: boolean,
			isOwner: boolean,
		},
		permissions: string[];
	}
	restrictions: Record<string, boolean>;
}
const lsKey = 'v.1_adm_rank_data';
const store: Writable<IRankResponse> = writable(localStorage.getItem(lsKey) && JSON.parse(localStorage.getItem(lsKey)) || null);
store.subscribe(newRankData => {
	localStorage.setItem(lsKey, JSON.stringify(newRankData));
});

import request from '../lib/request';
const permissionsPromise = request.get<IRankResponse>('/permissions').then(data => {
	console.log('[info] current perms', data.data);
	store.set(data.data);
});

export const promise = permissionsPromise;

export default store as Readable<IRankResponse>

export const get = () => {
	return getStore(store).rank.name;
}

type RankTypes = 'Admin' | 'Moderator' | 'Owner' | 'Mod' | 'admin' | 'moderator' | 'owner' | 'mod';

export const is = (rank: RankTypes): boolean => {
	const data = getStore(store);
	if (data) {
		// details is basically inherited permissions
		// so if user is owner, they will always have owner, admin, and mod
		// etc...
		// so checks for "if (mod || admin || owner)" in the final if statement isn't required
		let d = data.rank.details;
		if (rank.toLowerCase() === 'owner') {
			return d.isOwner;
		}
		if (rank.toLowerCase() === 'admin') {
			return d.isAdmin;
		}
		if (rank.toLowerCase() === 'mod' || rank.toLowerCase() === 'moderator') {
			return d.isModerator;
		}
	}

	return false;
}

export const hasPermission = (permission: string) => {
	const data = getStore(store);
	if (data && data.rank) {
		return data.rank.permissions.includes(permission);
	}
	console.log('No data available') // should not be hit
	return false;
}