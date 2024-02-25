<script lang="ts">
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = true;
	let errorMessage: string | undefined;
    let successMessage: string | undefined;
    let lotteryItems = [];
	import * as rank from "../stores/rank";
	rank.promise.then(() => {
		if (!rank.hasPermission('RunLottery')) {
			errorMessage = "You must have the lottery permission to do this.";
		}
	});

    request.get('/lottery/get-items').then(d => {
        lotteryItems = d.data;
        disabled = !lotteryItems || !lotteryItems.length;
    })
</script>

<svelte:head>
	<title>Lottery</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Lottery</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
            {#if successMessage}
                <p class="text-success">{successMessage}</p>
            {/if}
		</div>

        <div class="col-12 mt-4 mb-4">
            <p class="fw-bold">Lottery Item Pool</p>
            <table class="table">
                <thead>
                    <tr>
                        <th>Item</th>
                        <th>RAP</th>
                        <th>Player</th>
                        <th>Last Online</th>
                    </tr>
                </thead>
                <tbody>
                    {#each lotteryItems as item}
                        <tr>
                            <td>{item.name}</td>
                            <td>{item.recentAveragePrice}</td>
                            <td>{item.username}</td>
                            <td>{item.onlineAt}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>

		<div class="col-12 mt-4">
			<button
				class="btn btn-success"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					
					disabled = true;
					request
						.post("/lottery/run")
						.then((d) => {
							successMessage = `Lottery Success! Item ${d.data.name} was given to ${d.data.username}`;
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Run Lottery</button
			>
		</div>
	</div>
</Main>

<style>
	p.err {
		color: red;
	}
</style>
