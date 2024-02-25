declare module 'svelte-routing' {
	import { SvelteComponent, SvelteComponentTyped } from 'svelte';

	interface LinkProps {
		to: string;
		replace?: boolean;
		state?: {
			[k in string | number]: unknown;
		};
		getProps?: (linkParams: GetPropsParams) => Record<string, any>;
	}

	interface GetPropsParams {
		location: RouteLocation;
		href: string;
		isPartiallyCurrent: boolean;
		isCurrent: boolean;
	}

	class Link extends SvelteComponentTyped<
		Omit<LinkProps & svelte.JSX.HTMLProps<HTMLAnchorElement> & svelte.JSX.SapperAnchorProps, 'href'>
		> { }

	export { Link };

	interface RouteProps {
		path?: string;
		component?: SvelteComponent;

		[additionalProp: string]: unknown;
	}

	interface RouteSlots {
		default: {
			location: RouteLocation;
			params: RouteParams;
		};
	}

	interface RouteLocation {
		pathname: string;
		search: string;
		hash?: string;
		state: {
			[k in string | number]: unknown;
		};
	}

	interface RouteParams {
		[param: string]: string;
	}

	class Route extends SvelteComponentTyped<RouteProps, Record<string, any>, RouteSlots> { }

	export { Route, RouteLocation };

	interface RouterProps {
		basepath?: string;
		url?: string;
	}

	class Router extends SvelteComponentTyped<RouterProps> { }

	export { Router };

	const link: (node: Element) => { destroy(): void };
	const links: (node: Element) => { destroy(): void };

	export { link, links };
	const navigate: (
		to: string,
		{
			replace,
			state,
		}?: {
			replace?: boolean;
			state?: {
				[k in string | number]: unknown;
			};
		},
	) => void;

	export { navigate };
}
