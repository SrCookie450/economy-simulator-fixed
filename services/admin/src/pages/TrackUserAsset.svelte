<script lang="ts">
    import { link } from "svelte-routing";
import Main from "../components/templates/Main.svelte";
    import request from '../lib/request';
    let assetId;

    let table = {
        columns: [],
        data: [],
    };
</script>

<Main>
    <div class="row">
        <div class="col-12">
            <p class="fw-bold">User Asset Track</p>
        </div>
        <div class="col-12 col-md-6">
            <input type="text" bind:value={assetId} class="form-control" placeholder="Asset ID" />
        </div>
        <div class="col-6 col-md-3">
            <button class="btn btn-primary w-100" on:click={() => {
                request.get('/assets/giveitem-circ?assetId=' + assetId + '&limit=100').then(data => {
                    table = {
                        data: data.data,
                        columns: ['UserAssetID', 'AssetID', 'UserID', 'Username', 'Serial']
                    };
                    if (data.data.length === 0) {
                        alert('This asset has no terminated copies.');
                    }
                }).catch(e => {
                    alert('Error fetching copies: ' + e.message);
                })
            }}>Get Rollback Circulation</button>
        </div>
        <div class="col-6 col-md-3">
            <button class="btn btn-primary w-100" on:click={() => {
                request.get('/assets/' + assetId + '/owners').then(data => {
                    table = {
                        data: data.data,
                        columns: ['UserAssetID', 'AssetID', 'Price', 'Serial', 'UserID', 'Created', 'Updated']
                    };
                    if (data.data.length === 0) {
                        alert('This asset has no terminated copies.');
                    }
                }).catch(e => {
                    alert('Error fetching copies: ' + e.message);
                })
            }}>Get All Owners</button>
        </div>
        <div class="col-12"><hr /></div>

        <div class="col-12">
            <table class="table">
                <thead>
                    <tr>
                        {#each table.columns as col}
                            <th>{col}</th>
                        {/each}
                    </tr>
                </thead>
                <tbody>
                    {#each table.data as entry}
                        <tr>
                        {#each Object.getOwnPropertyNames(entry) as col}
                            {#if col === "userId"}
                            <td>
                                <a use:link href={"/admin/manage-user/" + entry[col]}>
                                    {entry[col]}
                                </a>
                            </td>
                            {:else}
                            <td>{entry[col]}</td>
                            {/if}
                        {/each}
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    </div>
</Main>