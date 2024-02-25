<script lang="ts">
	import { link } from "svelte-routing";
	import request from "../lib/request";
    import Loader from "./misc/Loader.svelte";
    import Permission from "./Permission.svelte";
    import Modal from './modal/Confirm.svelte';

    export let assetId: number;

    let pendingDelete = {
        id: 0,
        assetId: 0,
        amount: 0,
        userId: 0,
    };
    let showPendingDelete = false;

    let showMultiDelete = false;
    let mulitDelete = [];
    let showMultiLoading = false;
    let multiLoadingProgress = {
        total: 0,
        pending: 0,
        done: 0,
        errors: [],
    }

    let limit = 100;

    let start = '';
    let end = '';

    let sales: {id: number; user_id_one: number; username: string; amount: number; currency_type: number; user_asset_id: number; created_at :string; isChecked: boolean;}[]|undefined = undefined;

    $: {
        request.get(`/asset/sale-history?assetId=${assetId}&limit=${limit}&offset=0&start=${start}&end=${end}`).then(d => {
            sales = d.data;
        })
    }
</script>

{#if showPendingDelete}
    <Modal title='Confirm Deletion' message={`Are you sure you want to delete and refund transaction #${pendingDelete.id}? This cannot be undone.`} cb={didConfirm => {
        if (didConfirm) {
            console.log('Delete trans:',pendingDelete.id);
            const id = pendingDelete.id;
            request.post(`/asset/refund-transaction?transactionId=${pendingDelete.id}&assetId=${pendingDelete.assetId}&userId=${pendingDelete.userId}&expectedAmount=${pendingDelete.amount}`).then(d => {
                sales = sales.filter(v => v.id !== id);
            })
        }


        pendingDelete = undefined;
        showPendingDelete = false;
    }} />
{/if}



{#if showMultiDelete}
    <Modal title='Confirm Deletion' message={`Are you sure you want to delete and refund all ${mulitDelete.length.toLocaleString()} selected transactions? This cannot be undone.`} cb={didConfirm => {
        if (didConfirm) {
            multiLoadingProgress = {
                total: 0,
                pending: mulitDelete.length,
                done: 0,
                errors: [],
            };

            (async () => {
                sales = sales.filter(v => !mulitDelete.find(a => a.id === v.id));
                for (const item of mulitDelete) {
                    const id = item.id;
                    await request.post(`/asset/refund-transaction?transactionId=${item.id}&assetId=${item.assetId}&userId=${item.userId}&expectedAmount=${item.amount}`).then(d => {
                        multiLoadingProgress.done++;
                    }).catch(e => {
                        multiLoadingProgress.errors.push({
                            id: id,
                            error: e,
                        })
                    })


                    multiLoadingProgress.total++;
                    if (multiLoadingProgress.total === multiLoadingProgress.pending) {
                        console.log('[info] multi delete over');
                        showMultiLoading = false;
                    }
                }
            })()
        }


        showMultiDelete = false;
        mulitDelete = [];
        showMultiLoading = true;
    }} />
{/if}

<Permission p="GetSaleHistoryForAsset">
    <div class="row">
        <div class="col-6">
            <h4 class="mt-4">Sale History</h4>
        </div>
        <div class="col-3">
            <select class="form-control" bind:value={limit}>
                <option value={10}>10</option>
                <option value={100}>100</option>
                <option value={1000}>1k</option>
                <option value={10000}>10k</option>
            </select>
        </div>
        <div class="col-3">
            {#if sales && sales.find(a => a.isChecked)}
                <button class="btn btn-outline-warning" on:click={() => {
                    mulitDelete = [];
                    for (const sale of sales.filter(a => a.isChecked)) {
                        mulitDelete.push({
                            id: sale.id,
                            assetId: assetId,
                            userId: sale.user_id_one,
                            amount: sale.amount,
                        })
                    }
                    showMultiDelete = true;
                }}>Refund Selected</button>
            {/if}
        </div>
    </div>
    <div class="row">
        <div class="col-3">
            <input class="form-control" type="text" bind:value={start} placeholder="Start Date" />
        </div>
        <div class="col-3">
            <input class="form-control" type="text" bind:value={end} placeholder="End Date" />
        </div>
    </div>
    {#if multiLoadingProgress.errors.length !== 0}
        {#each multiLoadingProgress.errors as err}
            <p>Refund for {err.id} failed: {err.error && err.error.message}</p>
        {/each}
    {/if}
    {#if showMultiLoading}
        <Loader />
        <p>Refunding and deleting transactions. {multiLoadingProgress.total} / {multiLoadingProgress.pending}</p>
        {#if multiLoadingProgress.errors.length !== 0}
            <p>{multiLoadingProgress.errors.length} refund failed.</p>
        {/if}
    {:else if !sales}
        <Loader />
    {:else if sales.length === 0}
        <p>There are no purchases associated with this asset.</p>
    {:else}
        <table class="table">
            <thead>
                <tr>
                    <th>
                        <input on:change={(e) => {
                            sales = sales.map(v => {
                                v.isChecked = e.currentTarget.checked;
                                return v;
                            })
                        }} class="form-check-input" type="checkbox" value="" />
                    </th>
                    <th>#</th>
                    <th>User</th>
                    <th>Amount</th>
                    <th>UAID</th>
                    <th>Date</th>
                    <Permission p="RefundAndDeleteFirstPartyAssetSale">
                        <th>Refund</th>
                    </Permission>
                </tr>
            </thead>
            <tbody>
                {#each sales as sale}
                    <tr>
                        <td>
                            <input checked={sale.isChecked} on:change={(e) => {
                                sales = sales.map(v => {
                                    if (v.id === sale.id) {
                                        v.isChecked = e.currentTarget.checked;
                                    }
                                    return v;
                                })
                            }} class="form-check-input" type="checkbox" value="" />
                        </td>
                        <td>{sale.id}</td>
                        <td>
                            <a use:link href={`/admin/manage-user/${sale.user_id_one}`}>
                                {sale.username}
                            </a>
                        </td>
                        <td>
                            {sale.amount} {sale.currency_type === 1 ? 'Robux' : 'Tickets'}
                        </td>
                        <td>{sale.user_asset_id}</td>
                        <td>{sale.created_at}</td>
                        <Permission p="RefundAndDeleteFirstPartyAssetSale">
                            <td>
                                <button class="btn btn-sm btn-outline-warning mb-0" on:click={() => {
                                    pendingDelete = {
                                        id: sale.id,
                                        assetId: assetId,
                                        userId: sale.user_id_one,
                                        amount: sale.amount,
                                    }
                                    showPendingDelete = true;
                                }}>Refund</button>
                            </td>
                        </Permission>
                    </tr>
                {/each}
            </tbody>
        </table>
    {/if}
</Permission>