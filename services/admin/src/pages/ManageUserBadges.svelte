<script lang="ts">
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let userBadges: { id: number; name: string }[];
	$: {
		disabled = true;
		request
			.get("/getbadges?userId=" + userId)
			.then((d) => {
				userBadges = d.data;
			})
			.finally(() => {
				disabled = false;
			});
	}
	export let userId: string;

	const tableOfBadgeIconName: Record<string, string> = {
		1: "Administrator",
		2: "Friendship",
		3: "Combat Initiation",
		4: "Warrior",
		5: "Bloxxer",
		6: "Homestead",
		7: "Bricksmith",
		8: "Inviter",
		11: "Builders Club",
		12: "Veteran",
		14: "Ambassador",
		15: "Turbo Builders Club",
		16: "Outrageous Builders Club",
		17: "Official Model Maker",
		18: "Welcome To The Club",
		// 33: 'Official Model Maker', // ST1 RobloxBadges.BadgeTypes table set 33 instead of 17
		// 34: 'Welcome To The Club' // ST1 RobloxBadges.BadgeTypes table set 34 instead of 18
	};
	let badges: { id: number; name: string }[] = [];
	for (const id of Object.getOwnPropertyNames(tableOfBadgeIconName)) {
		let v = parseInt(id, 10);
		badges.push({
			id: v,
			name: tableOfBadgeIconName[id],
		});
	}

	let disabled = false;
	let errorMessage: string | undefined;
	let badgesToAdd: number[] = [];
	let badgesToRemove: number[] = [];

	import * as rank from "../stores/rank";
	rank.promise.then(() => {
		if (!rank.is("admin")) {
			errorMessage = "You must have the administrator permission to manage badges.";
		}
	});
</script>

<style>
	p.err {
		color: red;
	}
</style>

<svelte:head>
	<title>Manage User Badges</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<h2>Add Badge</h2>
			<label for="badge-add">Add Badge</label>
			<select class="form-control" id="badge-add">
				{#each badges as badge}
					<option value={badge.id}>{badge.name}</option>
				{/each}
			</select>
			<button
				class="btn btn-success mt-2"
				on:click={(e) => {
					e.preventDefault();
					let el = document.getElementById("badge-add");
					// @ts-ignore
					let badgeName = el.options[el.selectedIndex].text;
					// @ts-ignore
					let v = el.value;
					if (disabled) {
						return;
					}
					disabled = true;
					request
						.post("/givebadge", {
							userId,
							badgeId: v,
						})
						.then((d) => {
							disabled = false;
							userBadges.push({
								id: v,
								name: badgeName,
							});
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Add Badge</button
			>
		</div>
		<div class="col-12 mt-2 mb-2">
			<hr />
		</div>
		<div class="col-12">
			<h2 class="mt-0">Remove Badge(s)</h2>
			<p>Check the box next to the badge you want to remove, then click the button below to save your changes.</p>
			{#if userBadges && userBadges.length >= 1}
				{#each userBadges as badge}
					<div class="form-check">
						<input
							class="form-check-input badge-checkbox"
							type="checkbox"
							on:change={(e) => {
								let checked = e.currentTarget.checked;
								if (checked) {
									badgesToRemove.push(badge.id);
								} else {
									badgesToRemove = badgesToRemove.filter((v) => {
										return v !== badge.id;
									});
								}
							}}
							id={`checkbox_opt_${badge.id}`}
						/>
						<label class="form-check-label" for={`checkbox_opt_${badge.id}`}>{badge.name}</label>
					</div>
				{/each}
			{:else if userBadges && userBadges.length === 0}
				<p>User does not have any badges</p>
			{:else}
				<p>Loading Badges...</p>
			{/if}
			<button
				class="btn btn-warning mt-4"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					if (badgesToRemove.length === 0) {
						return;
					}
					disabled = true;
					let proms = [];
					for (const item of badgesToRemove) {
						proms.push(
							request.post("/deletebadge", {
								userId,
								badgeId: item,
							})
						);
					}
					Promise.all(proms)
						.then((d) => {
							userBadges = userBadges.filter((v) => {
								return !badgesToRemove.includes(v.id);
							});
							badgesToRemove = [];
						})
						.catch((e) => {
							errorMessage = (e && e.response && e.response.data) || "Unknown exception";
						})
						.finally(() => {
							disabled = false;
						});
				}}>Save Badge Removal Changes</button
			>
		</div>
	</div>
</Main>
