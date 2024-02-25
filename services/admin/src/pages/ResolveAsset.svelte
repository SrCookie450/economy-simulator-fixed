<script lang="ts">
	import Main from "../components/templates/Main.svelte";
import request from "../lib/request";
import {link} from 'svelte-routing';

	let url: string = "";
    let error: string|undefined;
    let disabled = false;
    let results: {
        assets: {assetId:number}[];
        users: {userId: number}[];
        groups: {groupId: number}[];
    }|undefined;

    const onClick = (e) => {
        if (disabled) return;
        disabled = true;
        error = undefined;
        results = undefined;
        request.get('/moderation/get-by-thumbnail?url=' + encodeURIComponent(url)).then(data => {
            results = data.data;
        }).catch(e => {
            error = e.message;
        }).finally(() => {
            disabled = false;
        })
    }
</script>

<Main>
	<div class="row">
		<div class="col-12">
			<h3 class="mb-0">Resolve Asset URL</h3>
            {#if error}
                <p class="text-danger">{error}</p>
            {/if}
			<div class="row">
				<div class="col-12 col-lg-6">
					<input {disabled} class="form-control" type="text" bind:value={url} placeholder="URL" />
				</div>
				<div class="col-12 col-lg-6">
					<button {disabled} class="btn btn-primary" on:click={onClick}>Lookup</button>
				</div>
			</div>
            {#if results !== undefined}
                {#if results.groups.length === 0 && results.users.length === 0 && results.assets.length === 0}
                    <p class="text-align-center mt-4">No results</p>
                {:else}
                    <table class="table">
                        <thead>
                            <tr>
                                <th>Type</th>
                                <th>ID</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each results.groups as group}
                                <tr>
                                    <td>Group</td>
                                    <td>
                                        <a href={`/My/Groups.aspx?GroupID=${group.groupId}`}>{group.groupId}</a>
                                    </td>
                                </tr>
                            {/each}
                            {#each results.users as user}
                                <tr>
                                    <td>User</td>
                                    <td>
                                        <a use:link href={`/admin/manage-user/${user.userId}`}>{user.userId}</a>
                                    </td>
                                </tr>
                            {/each}
                            {#each results.assets as asset}
                                <tr>
                                    <td>Asset</td>
                                    <td>
                                        <a href={`/catalog/${asset.assetId}/--`}>{asset.assetId}</a>
                                    </td>
                                </tr>
                            {/each}
                        </tbody>
                    </table>
                {/if}
            {/if}
		</div>
	</div>
</Main>
