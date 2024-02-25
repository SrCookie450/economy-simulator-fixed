<script lang="ts">
    import dayjs from "dayjs";
    
        import Main from "../components/templates/Main.svelte";
    import request from "../lib/request";
        export let userId: number|string;
    
        let transactions:{
            trade: {
                id: number;
                partnerId: number;
                partnerUsername: string;
                status: string;
                createdAt: string;
                updatedAt: string;
            };
            db: {
                id: number;
                userIdOne: number;
                userIdTwo: number;
                userOneRobux: number|null;
                userTwoRobux: number|null;
                usernameOne: string;
                usernameTwo: string;
                status: string;
            };
            items: {
                userId: number;
                userAssetId: number;
                recentAveragePrice: number;
                serial: number|null;
                name: string;
                price: number;
                assetId:number;
            }[]
        }[] = [];
        let offset = 0;
        let limit = 25;
        let type = 'Inbound';
    
        $: {
            let url = '/users/' + userId + '/trades?type='+type+'&offset=' + offset + '&limit=' + limit;
            request.get(url).then(d => {
                transactions = d.data;
            })
            console.log('state change for',type)
        }
    </script>
    
    <Main>
        <div class="row">
            <div class="col-12">
                <h3>User Trades</h3>
                <select class="form-control" bind:value={type}>
                    <option value='Inbound'>Inbound</option>
                    <option value='Outbound'>Outbound</option>
                    <option value='Completed'>Completed</option>
                    <option value='Inactive'>Inactive</option>
                </select>
            </div>
            <div class="col-12">
                <table class="table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Status</th>
                            <th>Created</th>
                            <th>Other Player</th>
                            <th>Robux</th>
                            <th>Player's Items</th>
                            <th>Other Player's Items</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each transactions as item}
                            <tr>
                                <td>{item.trade.id}</td>
                                <td>{item.trade.status}</td>
                                <td>{dayjs(item.trade.createdAt).format('MMM DD YYYY, h:mm A')}</td>
                                <td>
                                    <a href={`/admin/manage-user/${item.trade.partnerId}`}>{item.trade.partnerUsername}</a>
                                </td>
                                <td>
                                    {#if item.db.userOneRobux !== null}
                                        <p class="mb-0">{item.db.usernameOne} gives {item.db.userOneRobux || '0'}</p>
                                    {/if}
                                    {#if item.db.userTwoRobux !== null}
                                        <p class="mb-0">{item.db.usernameTwo} gives {item.db.userTwoRobux}</p>
                                    {/if}
                                </td>
                                <td>
                                    {#each item.items.filter(v => v.userId == userId) as item}
                                        <p class="mb-0">
                                            <a href={`/catalog/${item.assetId}/--`}>
                                                {item.name}
                                            </a>
                                            {#if item.serial !== null}
                                            <span>(#{item.serial})</span>
                                            {:else}
                                            <span>(UAID {item.userAssetId})</span>
                                            {/if}
                                        </p>
                                    {/each}
                                </td>
                                <td>
                                    {#each item.items.filter(v => v.userId != userId) as item}
                                        <p class="mb-0">
                                            <a href={`/catalog/${item.assetId}/--`}>
                                                {item.name}
                                            </a>
                                            {#if item.serial !== null}
                                            <span>(#{item.serial})</span>
                                            {:else}
                                            <span>(UAID {item.userAssetId})</span>
                                            {/if}
                                        </p>
                                    {/each}
                                </td>
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