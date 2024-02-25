<script lang="ts">
    import Permission from "../components/Permission.svelte";
import Main from "../components/templates/Main.svelte";
    import request from '../lib/request';

    let threadId;
</script>

<svelte:head>
	<title>Forums</title>
</svelte:head>

<Main>
    <div class="row">
        <div class="col-12">
            <h3>Forums</h3>
        </div>
        <Permission p="LockForumThread">
            <div class="col-6">
                <p>Lock a thread by its ID</p>
                <div class="row">
                    <div class="col-6">
                        <input class="form-control" placeholder="ThreadID" bind:value={threadId} />
                    </div>
                    <div class="col-6">
                        <button class="btn btn-outline-warning" on:click={() => {
                            if (!Number.isSafeInteger(parseInt(threadId, 10))) {
                                return
                            }
                            request.request({
                                method: 'POST',
                                url: '/lock-forum-thread?threadId=' + threadId,
                            }).then(() => {
                                alert('Post locked');
                            }).catch(e => {
                                alert(e.message);
                            })
                        }}>Lock Thread</button>
                    </div>
                </div>
            </div>
        </Permission>
    </div>
</Main>