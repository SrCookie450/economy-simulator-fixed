<script lang="ts">
import dayjs from "dayjs";

    import Main from "../components/templates/Main.svelte";
import request from "../lib/request";
    export let userId: number|string;

    let transactions = [];
    let offset = 0;
    let limit = 25;
    let type = 'Purchase';

    $: {
        let url = '/users/' + userId + '/transactions?type=' + type + '&offset=' + offset + '&limit=' + limit;
        if (type === 'All') {
            url = '/users/' + userId + '/all-transactions?offset=' + offset + '&limit=' + limit
        }
        request.get(url).then(d => {
            transactions = d.data;
        })
        console.log('state change for',type)
    }
</script>

<Main>
    <div class="row">
        <div class="col-12">
            <h3>User Transactions</h3>
            <select class="form-control" on:change={(v) => {
                type = v.currentTarget.value;
            }}>
                <option value='Purchase'>Purchase</option>
                <option value='Sale'>Sale</option>
                <option value='All'>All</option>
            </select>
        </div>
        <div class="col-12">
            <table class="table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Type</th>
                        <th>Created</th>
                        <th>Other Player</th>
                        <th>Amount</th>
                        <th>Sub Type</th>
                        <th>Asset Data</th>
                        <th>Username Data</th>
                    </tr>
                </thead>
                <tbody>
                    {#each transactions as item}
                        <tr>
                            <td>{item.id}</td>
                            <td>{item.type} / {item.subType || '-'}</td>
                            <td>{dayjs(item.createdAt).format('MMM DD YYYY, h:mm A')}</td>
                            <td><a href={`/users/${item.userIdTwo}/profile`}>{item.username}</a></td>
                            <td>{item.amount}</td>
                            <td>{item.type} / {item.subType}</td>
                            <td>ID: {item.assetId} / {item.assetName} / UAID: {item.userAssetId}</td>
                            <td>Old Username: {item.oldUsername} / New: {item.newUsername}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
        <div class="col-12">
            <nav aria-label="Page navigation example">
                <ul class="pagination">
                    <li class="page-item">
                        <a class="page-link svelte-674n1s" href="#!" on:click={() => {
                            if (offset !== 0) {
                                offset -= limit;
                            }
                        }}>Previous</a>
                    </li> 
                    <li class="page-item active">
                        <a class="page-link svelte-674n1s" href="#!">{(offset/limit)+1}</a>
                    </li> 
                    <li class="page-item">
                        <a class="page-link svelte-674n1s" href="#!" on:click={() => {
                            if (transactions && transactions.length >= limit) {
                                offset += limit;
                            }
                        }}>Next</a>
                    </li>
                </ul>
            </nav>
        </div>
    </div>
</Main>