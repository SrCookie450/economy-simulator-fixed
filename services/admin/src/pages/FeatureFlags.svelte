<script lang="ts">
    import { Link } from "svelte-routing";
import Main from "../components/templates/Main.svelte";
    import request from "../lib/request";

    const toggleFeature = (flag, enabled) => {
        return request.post('/feature-flags/' + (enabled ? 'enable' : 'disable') + '?featureFlag='+flag).then(() => {
            window.location.reload();
        })
    }
</script>

<svelte:head>
	<title>Manage Feature Flags</title>
</svelte:head>

<Main>
	<div class="row">
        <div class="col-12">
            <h3>Manage Feature Flags</h3>
            <p>It will take at most 30 seconds for flags to update across all servers. Please <a href="/admin">update the site-wide alert</a> after disabling major features.</p>
        </div>
        {#await request.get('/feature-flags/all') then result}
            {#each Object.getOwnPropertyNames(result.data) as flag}
                <div class="col-12 col-lg-4 mb-2">
                    <div class="card card-body">
                        <p class="mb-0 font-weight-bold">{flag}</p>
                        <p>{result.data[flag] ? 'Enabled' : 'Disabled'}</p>
                        <div class="btn-group">
                            {#if result.data[flag]}
                                <button class="btn btn-sm btn-outline-danger" on:click={() => {
                                    toggleFeature(flag, false);
                                }}>Disable</button>
                            {:else}
                                <button class="btn btn-sm btn-outline-success" on:click={() => {
                                    toggleFeature(flag, true);
                                }}>Enable</button>
                            {/if}
                        </div>
                    </div>
                </div>
            {/each}
        {/await}
    </div>
</Main>