import { Writable, writable } from "svelte/store";
const lsKey = 'saved-pages-v.1';
const exists = typeof localStorage === 'object' && localStorage && localStorage.getItem(lsKey);
const store: Writable<{ url: string; title: string }[]> = writable(exists && JSON.parse(exists) || []);

store.subscribe((data) => {
	if (typeof localStorage === 'object' && localStorage) {
		localStorage.setItem(lsKey, JSON.stringify(data));
	}
})

export default store;

export const addPage = (pageTitle: string, pageUrl: string) => {
	store.update((old) => {
		let exists = false;
		for (const item of old) {
			if (item.url === pageUrl) {
				exists = true;
				break;
			}
		}
		if (!exists) {
			old.push({
				url: pageUrl,
				title: pageTitle,
			})
		}
		return old;
	});
}
export const removePage = (pageUrl: string) => {
	store.update(old => {
		let newArr = [];
		for (const item of old) {
			if (item.url !== pageUrl) {
				newArr.push(item);
			}
		}
		return newArr;
	})
}

export const updatePageTitle = (pageUrl: string, newTitle: string) => {
	store.update(old => {
		for (const item of old) {
			if (item.url === pageUrl) {
				item.title = newTitle;
			}
		}
		return old;
	})
}