<script lang="ts">
	import { link } from "svelte-routing";
import request from "../lib/request";
	import Loader from "./misc/Loader.svelte";


    export let assetId: number;

    let history: {id: number; asset_id: number; name: string; actor_id: number; username: string; is_for_sale: boolean; price_in_tickets: number|null; price_in_robux: number|null; is_limited: boolean; is_limited_unique: boolean; max_copies: number|null; offsale_at: string | null; created_at :string}[];

    $: {
        request.get(`/asset/product-history?assetId=${assetId}`).then(d => {
            history = d.data;
        })
    }
</script>

<div class="row">
    <div class="col-12">
        <h3>Update History</h3>
        {#if !history}
            <Loader />
        {:else if history.length === 0}
            <p>No changes recorded.</p>
        {:else}
            
                <table class="table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>ActorID</th>
                            <th>IsForSale</th>
                            <th>Price (R$)</th>
                            <th>Price (T$)</th>
                            <th>IsLimited</th>
                            <th>IsLimitedU</th>
                            <th>MaxCopies</th>
                            <th>OffsaleAt</th>
                            <th>Created</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each history as h}
                            <tr>
                                <td>{h.id}</td>
                                <td>
                                    <a use:link href={`/admin/manage-user/${h.actor_id}`}>
                                        {h.username}
                                    </a>
                                </td>
                                <td>{h.is_for_sale}</td>
                                <td>{h.price_in_robux}</td>
                                <td>{h.price_in_tickets}</td>
                                <td>{h.is_limited}</td>
                                <td>{h.is_limited_unique}</td>
                                <td>{h.max_copies}</td>
                                <td>{h.offsale_at}</td>
                                <td>{h.created_at}</td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
        {/if}
    </div>
</div>