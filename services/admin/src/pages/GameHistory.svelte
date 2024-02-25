<script lang="ts">
    import Main from '../components/templates/Main.svelte';
    import Permission from '../components/Permission.svelte';
    import request from '../lib/request';
    import dayjs from 'dayjs';
    import {link} from 'svelte-routing';
</script>

<Main>
    <div class="row">
		<Permission p="GetUsersInGame">
			<div class="col-12 mt-4">
				<h3>Game Play History</h3>
				<table class="table">
					<thead>
						<tr>
							<th>Game</th>
							<th>User</th>
							<th>Started</th>
							<th>Ended</th>
							<th>Duration</th>
						</tr>
					</thead>
					<tbody>
						{#await request.get('games/play-history?limit=100&offset=0') then data}
							{#each data.data as game}
							<tr>
								<td>
                                    <a href="/games/{game.asset_id}/--" target="_blank">
                                        {game.name}
                                    </a>
                                </td>
								<td>
									<a use:link href="/admin/manage-user/{game.user_id}">{game.username}</a>
								</td>
								<td>
									{dayjs(game.created_at).fromNow()} ({dayjs(game.created_at).format('MMMM DD, h:mm A')})
								</td>
								<td>
									{#if game.ended_at}
										{dayjs(game.ended_at).fromNow()} ({dayjs(game.ended_at).format('MMMM DD, h:mm A')})
									{:else}
										<span class="badge bg-success">Running</span>
									{/if}
								</td>
								<td>
									{#if game.ended_at}
										{dayjs(game.ended_at).diff(game.created_at, 'seconds')} Seconds
									{:else}
										{dayjs(new Date(Date.now()).toISOString()).diff(game.created_at, 'seconds')} Seconds
									{/if}
								</td>
							</tr>
							{/each}
						{/await}
					</tbody>
				</table>
			</div>
		</Permission>
	</div>
</Main>