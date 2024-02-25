<script lang="ts">
    import Permission from '../Permission.svelte';
    import moment from 'dayjs';
    import request from '../../lib/request';
    import Confirm from '../modal/Confirm.svelte';

    export let userId: string|number;
    let textView: string = 'Status';

    let modalBody: string|undefined;
    let modalCb: (didClickyes: boolean) => void|undefined;
    let modalVisible: boolean = false;

    let errorMessage: string|undefined = undefined;
</script>

<div class="row">
    <div class="col-12 mt-4">
        {#if modalVisible}
            <Confirm
                title="Confirm"
                message={modalBody}
                cb={(e) => {
                    modalVisible = false;
                    modalCb(e);
                }}
            />
        {/if}
        {#if errorMessage !== undefined}
            <p class="text-danger mt-4 mb-4">{errorMessage}</p>
        {/if}
        <div class="btn-group">
            <Permission p="GetUserStatusHistory">
                <button class={textView === "Status" ? "btn btn-primary" : "btn btn-outline-primary"} on:click={(e) => {
                    e.preventDefault();
                    textView = 'Status';
                }}>Status</button>
            </Permission>
            <Permission p="GetUserCommentHistory">
                <button class={textView === "Comments" ? "btn btn-primary" : "btn btn-outline-primary"} on:click={(e) => {
                    e.preventDefault();
                    textView = 'Comments';
                }}>Comments</button>
            </Permission>
            <Permission p="ManageInvites">
                <button class={textView === "Invites" ? "btn btn-primary" : "btn btn-outline-primary"} on:click={(e) => {
                    e.preventDefault();
                    console.log('new text view',textView);
                    textView = 'Invites';
                }}>Invites</button>
            </Permission>
        </div>
    </div>
        {#await request.get(textView === 'Status' ? ("/user/status-history?userId=" + userId) : textView === 'Comments' ? ('/user/comment-history?userId=' + userId) : ('/invites/' + userId)) then data}
            {#each data.data as item}
                <div class="col-12">
                    <div class="card card-body mt-2">
                        {#if textView === 'Invites'}
                            <p class="mb-0"><a href={`/admin/manage-user/${item.userId}`}>Invited {item.userId}</a></p>
                        {:else}
                            <p class="mb-0">{item.status || item.comment}</p>
                        {/if}
                        <p class="mb-0">{moment(item.created_at || item.createdAt).format("MMM DD YYYY, h:mm A")}</p>
                        {#if (textView === 'Status' || textView === 'Comments')}
                        <div class="row">
                            <div class="col-6 col-lg-3">
                                <button class="btn btn-outline-danger btn-sm mt-2" on:click={(e) => {
                                    e.preventDefault();
                                    modalBody = 'Confirm that you want to delete this content: ' + (item.status || item.comment);
                                    modalCb = (t) => {
                                        if (t) {
                                            // null
                                            request
                                                .delete(textView === 'Status' ? ("/user/status?userId=" + userId + "&statusId=" + item.id) : ('/user/comment?userId=' + userId + '&commentId=' + item.id))
                                                .then(() => {
                                                    window.location.reload();
                                                })
                                                .catch((err) => {
                                                    errorMessage = err.message;
                                                });
                                        }
                                    };
                                    modalVisible = true;
                                }}>Delete</button>
                            </div>
                        </div>
                        {/if}
                    </div>
                </div>
            {/each}
        {/await}
</div>