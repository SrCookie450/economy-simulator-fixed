<script lang="ts">
import { link } from "svelte-routing";
import Main from "../components/templates/Main.svelte";
import client from "../lib/request";


    let allStaff: {userId: number}[] = [];
    client.get('/staff/list').then(d => {
        allStaff = d.data;
    })
</script>

<Main>
    <div class="row">
        <div class="col-12">
            <h3>Staff List</h3>
        </div>
        <div class="col-12">
            <table class="table">
                <thead>
                    <tr>
                        <th>User</th>
                    </tr>
                </thead>
                <tbody>
                    {#each allStaff as staff}
                        <tr>
                            <td>
                                <a use:link href={`/admin/manage-user/${staff.userId}`}>
                                    {staff.userId}
                                </a>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    </div>
</Main>