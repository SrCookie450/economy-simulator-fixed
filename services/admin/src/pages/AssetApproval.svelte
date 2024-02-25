<script>
	import Loader from "../components/misc/Loader.svelte";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	import { link } from "svelte-routing";
	let blur = 'false';
	let manuallyInsertUrl = '';

	let alreadyLoadedIds = [];
	function loadNewAssets(mode) {
		request.get(mode === 'group' ? `/groups/pending-icons` : `/${mode +'s'}/pending-assets`).then((res) => {
			if (!assetsToApprove) {
				assetsToApprove = [];
			}
			let newAssetsToApprove = res.data.filter((v) => {
				// icons and assets have id, group icons have name (always unique)
				v.unique_key = mode + ':' + (v.id || v.name);
				return !alreadyLoadedIds.includes(v.unique_key);
			}).map((v) => {
				v.mode = mode;
				alreadyLoadedIds.push(v.unique_key);
				return v;
			});
			if (newAssetsToApprove.length !== 0) {
				for (const item of newAssetsToApprove) {
					assetsToApprove.push(item);
				}
			}
			assetsToApprove = [...assetsToApprove];
		});
	}
	let modMode = 'default';

	let assetsToApprove = null;
	const loadAllTypes = () => {
		loadNewAssets('asset');
		loadNewAssets('icon');
		loadNewAssets('group');
	}

	$: {
		loadAllTypes();
	}

	function onClick(approve, is18Plus, del, asset) {
		return (e) => {
			assetsToApprove = assetsToApprove.filter((v) => v !== asset);
			if (assetsToApprove.length === 0) {
				loadAllTypes();
			}
			if (asset.mode === "asset") {
				request
					.post("/asset/moderate" + (del ? "-and-delete" : ""), {
						isApproved: approve,
						assetId: asset.id,
						is18Plus: is18Plus,
					})
					.then(() => {})
					.catch((e) => {
						console.error("[error] could not approve asset", e);
					});
			} else if (asset.mode === 'icon') {
				request
					.post("/icon/moderate", {
						isApproved: approve,
						iconId: asset.id,
						is18Plus: is18Plus,
					})
					.then(() => {})
					.catch((e) => {
						console.error("[error] could not approve asset", e);
					});
			}else if (asset.mode === 'group') {
				request.post("/groups/icon-toggle", {
					groupId: asset.group_id,
					name: asset.name,
					approved: approve ? 1 : 2,
				})
				.then(() => {
					console.log('group icon set');
				})
				.catch(e => {
					console.error('[error] could not modify group icon',e);
				})
				.finally(() => {
				})
			}else{
				console.error('invalid mode',asset.mode,asset);
			}
		};
	}

	$:{
		console.log('blur',blur);

	}
</script>


<style>
	button {
		width: 100%;
	}
	input {
		width: 100%;
	}
</style>


<svelte:head>
	<title>Asset Approval</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Asset Approval</h1>
			<div class="row">
				<div class="col-6 col-lg-4">
					<select class="form-control" bind:value={modMode}>
						<option value="default">Striped BG</option>
						<option value="white">White BG</option>
						<option value="black">Black BG</option>
					</select>
				</div>
				<div class="col-6 col-lg-3">
					<select class="form-control" bind:value={blur}>
						<option value="false">Disable Blur</option>
						<option value="true">Enable Blur</option>
					</select>
				</div>
			</div>
				<div class="row">
					<div class="col-10 col-lg-4">
						<label for="manual-insert">Manually Insert Asset Into Queue</label>
						<input
							type="text"
							class="form-control"
							id="manual-insert"
							placeholder="Item URL or Asset ID"
							bind:value={manuallyInsertUrl}
						/>
					</div>
					<div class="col-2">
						<button class="btn btn-primary mt-4" on:click={(e) => {
							if (manuallyInsertUrl) {
								let asset = manuallyInsertUrl.match(/\/[0-9]+\//);
								if (!asset || !asset[0]) {
									asset = manuallyInsertUrl.match(/[0-9]+/);
								} else {
									asset[0] = asset[0].slice(1, -1);
								}
								if (asset[0]) {
									let id = asset[0];
									request.get("/asset/moderation-details?assetId=" + id).then((d) => {
										console.log(d.data);
										if (!assetsToApprove) {
											assetsToApprove = [];
										}
										d.data.mode = 'asset';
										assetsToApprove.unshift(d.data);
										assetsToApprove = [...assetsToApprove];
									});
								}
							}
						}}>Insert</button>
					</div>
				</div>
		</div>
		<div class={"col-12 mt-4"}>
			{#if assetsToApprove === null}
				<Loader />
			{:else if assetsToApprove.length === 0}
				<p class="text-cener">There are no assets to approve at this time.</p>
			{:else}
				<div class={"row mb-4"}>
					{#each assetsToApprove as asset}
						<div class="col-12 mt-4 mb-4">
							<div class={"card card-body mod-card-" + modMode}>
								<div class="row">
									<div class="col-12 col-lg-6">
										<div class="mod-icon">
											<h3 class="text-left">{asset.group_id ? ('Group ' + asset.group_id) : asset.name}</h3>
											<p class="text-left">By <a use:link href={`/admin/manage-user/${asset.creatorId || asset.user_id}`}>{asset.creatorName || asset.creatorname}</a></p>
											<a href={asset.group_id ? `/My/Groups.aspx?gid=${asset.group_id}` : `/catalog/${asset.asset_id || asset.id}/--`}>
												{#if asset.assetType === 'Audio'}
													<audio controls={true}>
														<source src={`/admin-api/api/assets/get-asset-stream?assetId=${asset.asset_id || asset.id}`} />
													</audio>
												{:else}
													<img on:error={(e) => {
														console.log('[warn] image load failure',e);
														let assetId = asset.asset_Id || asset.id;
														console.log('asseTId',assetId);
													}} class={"d-block m-icon-image" + (blur==="true" ? " mod-blury-image" :"")} src={asset.group_id ? `${asset.name}` : `${asset.content_url}`} alt={`Asset ${asset.id || asset.group_id}`} />
												{/if}
											</a>
										</div>
									</div>
									<div class="col-12 col-lg-6">
										<div class="row">
											<div class="col-12">
												<div class="btn-group w-100">
													<button class="btn btn-success border border-dark" on:click={onClick(true, false, false, asset)}>OK</button>
												
													<button disabled={asset.group_id !== undefined} class="btn border border-dark" on:click={onClick(true, true, false, asset)}>OK, 18+</button>
												</div>
											</div>
											<div class="col-12 mt-4">
												<div class="btn-group w-100">
													<button class="btn border border-dark" on:click={onClick(false, true, false, asset)}>BAD</button>
												
													<button class="btn border border-dark" on:click={onClick(false, true, true, asset)}>BAD + DELETE</button>
												</div>
											</div>
										</div>
									</div>
								</div>
							</div>
						</div>
					{/each}
				</div>
			{/if}
		</div>
	</div>
</Main>
