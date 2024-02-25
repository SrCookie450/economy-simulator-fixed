<script lang="ts">
    import { InfoIcon, LockIcon } from 'svelte-feather-icons';
import Main from '../components/templates/Main.svelte';
    import request from '../lib/request';
    let groupData: any = undefined;
    let auditLog: any = undefined;
    let auditColumns:string[] = [];

    let disabled = false;
    let groupId: number;
    let groupName: string;
    let feedback: string|undefined = undefined;

    const loadAuditLog = () => {
        request.get(`/groups/audit-log?groupId=${groupData.info.id}`).then(data => {
            if (!data.data.length) {
                auditColumns = [];
                auditLog = [];
                return;
            }
            auditColumns = Object.getOwnPropertyNames(data.data[0]);
            auditLog = data.data;
        })  
    }

    let groups: any[] = [];
    let offset = 0;
    const loadGroups = () => {
        request.get(`/groups/list?sortOrder=asc&sortColumn=group.id&limit=10&offset=${offset}`).then(d => {
            groups = d.data;
        })
    }

    $: {
        loadGroups();
    }

</script>

<Main>
    <div class="row">
        <div class="col-12">
            <h3>Group Search</h3>
            {#if feedback !== undefined}
                <p class='text-danger'>{feedback}</p>
            {/if}
        </div>
        <div class="col-12">
            <div class="row">
                <div class="col-8">
                    <input disabled={disabled} bind:value={groupId} type="number" class="form-control" placeholder="Group ID" />
                </div>
                <div class="col-4">
                    <button disabled={disabled} class="btn btn-success" on:click={() => {
                        disabled = true;
                        request.get('/groups/get-by-id?groupId=' + encodeURIComponent(groupId)).then(result => {
                            groupData = result.data;
                            loadAuditLog();
                        }).catch(e => {
                            feedback = e.message;
                        }).finally(() => {
                            disabled = false;
                        })
                    }}>Search</button>
                </div>
            </div>
            <div class="row mt-4">
                <div class="col-8">
                    <input disabled={disabled} bind:value={groupName} type="text" class="form-control" placeholder="Name" />
                </div>
                <div class="col-4">
                    <button disabled={disabled} class="btn btn-success" on:click={(e) => {
                        request.get('/groups/get-by-name?name=' + encodeURIComponent(groupName)).then(result => {
                            groupData = result.data;
                            loadAuditLog();
                        }).catch(e => {
                            feedback = e.message;
                        }).finally(() => {
                            disabled = false;
                        })
                    }}>Search</button>
                </div>
            </div>
        </div>
        <div class="col-12 mt-4">
            {#if groupData}
                <div class="row">
                    <div class="col-4">
                        <img class="w-100" src={groupData.icon.name} />
                        <p><span class="fw-bold">Icon Approved: </span> {groupData.icon.is_approved === 1 ? 'true' : 'false'}</p>
                    </div>
                    <div class="col-8">
                        <h3>{groupData.info.name}</h3>
                        <p>{groupData.info.description}</p>
                        <p><span class="fw-bold">Current Owner: </span> #{groupData.info.user_id}</p>
                        <p><span class="fw-bold">Locked: </span> {groupData.info.locked ? 'true' : 'false'}</p>
                        <button disabled={disabled} class='btn btn-outline-warning' on:click={() => {
                            disabled = true;
                            request.post('/groups/toggle-lock-status?groupId=' + groupData.info.id + '&locked=' + (!groupData.info.locked)).then(result => {
                                groupData.info.locked = !groupData.info.locked;
                                groupData = {...groupData};
                            }).catch(e => {
                                feedback = e.message;
                            }).finally(() => {
                                disabled = false;
                            })
                        }}>{groupData.info.locked ? 'Unlock' : 'Lock'}</button>
                        <button disabled={disabled} class="btn btn-outline-danger" on:click={() => {
                            if (prompt('Type "yes" to confirm deletion.', 'no') !== 'yes') {
                                return;
                            }
                            disabled = true;
                            request.post('/groups/reset?groupId=' + groupData.info.id).then(result => {
                                groupData.info.name = '[ Content Deleted (' + groupData.info.id +') ]';
                                groupData.info.description = '[ Content Deleted ]';

                                groupData = {...groupData};
                            }).catch(e => {
                                feedback = e.message;
                            }).finally(() => {
                                disabled = false;
                            })
                        }}>Delete</button>
                    </div>
                    {#if auditColumns && auditLog}
                    <div class="col-12">
                        <h3>Audit Log</h3>
                        <table class="table">
                            <thead>
                                <tr>
                                    {#each auditColumns as column}
                                        <th>{column}</th>
                                    {/each}
                                </tr>
                            </thead>
                            <tbody>
                                {#each auditLog as log}
                                    <tr>
                                        {#each auditColumns as column}
                                            <td>{log[column]}</td>
                                        {/each}
                                    </tr>
                                {/each}
                            </tbody>
                        </table>
                    </div>
                    {/if}
                </div>
            {/if}

            <table class="table">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Name</th>
                        <th>Owner ID</th>
                        <th>Description</th>
                        <th>Created</th>
                    </tr>
                </thead>
                <tbody>
                    {#each groups as group}
                        <tr>
                            <td>{group.id}</td>
                            <td class="pointer" on:click={(e) => {
                                if (disabled) {
                                    return
                                }
                                disabled = true;
                                request.get('/groups/get-by-id?groupId=' + encodeURIComponent(group.id)).then(result => {
                                    groupData = result.data;
                                    loadAuditLog();
                                }).catch(e => {
                                    feedback = e.message;
                                }).finally(() => {
                                    disabled = false;
                                })
                            }}>
                                {#if group.locked}
                                    <LockIcon />
                                {/if}
                                {group.name}
                            </td>
                            <td>{group.user_id}</td>
                            <td>{group.description}</td>
                            <td>{group.created_at}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
            <div class="col-12">
                <nav aria-label="Page navigation example">
                    <ul class="pagination">
                        <li class={`page-item${disabled || !offset ? " disabled" : ""}`}>
                            <a
                                class="page-link"
                                href="#!"
                                on:click={(e) => {
                                    e.preventDefault();
                                    if (offset >= 10) {
                                        offset -= 10;
                                    }
                                    loadGroups();
                                }}>Previous</a
                            >
                        </li>
                        <li class="page-item active">
                            <a
                                class="page-link"
                                href="#!"
                                on:click={(e) => {
                                    e.preventDefault();
                                }}>{(offset / 10 + 1).toLocaleString()}</a
                            >
                        </li>
                        <li class={`page-item${disabled || (groups && groups.length < 10) ? " disabled" : ""}`}>
                            <a
                                class="page-link"
                                href="#!"
                                on:click={(e) => {
                                    e.preventDefault();
                                    offset += 10;
                                    loadGroups();
                                }}>Next</a
                            >
                        </li>
                    </ul>
                </nav>
            </div>
        </div>
    </div>
</Main>