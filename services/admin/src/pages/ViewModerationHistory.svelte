<script lang="ts">
import dayjs from "dayjs";

    import Main from "../components/templates/Main.svelte";
import request from "../lib/request";
    export let userId: number|string;

    let modHistory = [];
    let offset = 0;
    let url = "/users/" + userId + "/moderation-history"
    $: {
        let url = "/users/" + userId + "/moderation-history"
        request.get(url).then(d => {
            modHistory = d.data;
        })
    }
</script>

<Main>
    <div class="row">
        <div class="col-12">
            <h3>Moderation History</h3>
        </div>
        <div class="col-12">
            <table class="table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Reason</th>
                        <th>Created</th>
                        <th>Author</th>
                        <th>Internal Reason</th>
                        <th>Expires</th>
                    </tr>
                </thead>
                <tbody>
                    {#each modHistory as modAction}
                        <tr>
                            <td>{modAction.id}</td>
                            <td>{modAction.reason}</td>
                            <td>{dayjs(modAction.created_at).format('MMM DD YYYY, h:mm A')}</td>
                            <td><a href={`/users/${modAction.author_user_id}/profile`}>{modAction.author_user_id}</a></td>
                            <td>{modAction.internal_reason}</td>
                            <td>{dayjs(modAction.expired_at).format('MMM DD YYYY, h:mm A')}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    </div>
</Main>