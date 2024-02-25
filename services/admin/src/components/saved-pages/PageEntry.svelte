<script lang="ts">
	import { link } from 'svelte-routing';
	import { FileTextIcon, MinusCircleIcon } from 'svelte-feather-icons';

	export let title: string;
	export let url: string;

	import { removePage, updatePageTitle } from '../../stores/saved-pages';

	let textBoxShown = false;
	function init(el) {
		el.focus();
	}
</script>

<li class="nav-item ml-2">
	<a use:link class="nav-link justify-content-between align-items-center d-flex pb-0" href={url}>
		{#if textBoxShown}
			<input
				type="text"
				class="form-control"
				placeholder="New Name"
				value={title}
				use:init
				on:keydown={(e) => {
					if (e.key === 'Enter') {
						textBoxShown = false;
						updatePageTitle(url, e.currentTarget.value);
					}
				}}
			/>
		{:else}
			<p
				class="pb-0 pt-0 mb-0 mt-0"
				on:click={(e) => {
					// e.preventDefault();
					// textBoxShown = true;
				}}
			>
				<FileTextIcon class="mr-0" />
				{title}
			</p>
		{/if}
		<p class="pb-0 pt-0 mb-0 mt-0">
			<a
				href="#!"
				on:click={(e) => {
					e.preventDefault();
					removePage(url);
				}}
			>
				<MinusCircleIcon class="mr-0" />
			</a>
		</p>
	</a>
</li>
