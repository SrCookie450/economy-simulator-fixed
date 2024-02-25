<script lang="ts">
	import dayjs from "dayjs";
    import {link} from "svelte-routing";
    import DropdownButton from "../components/misc/DropdownButton.svelte";
	import Loader from "../components/misc/Loader.svelte";
	import Confirm from "../components/modal/Confirm.svelte";

	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";

	let reports;
	let alert = "";

	let modalBody: string;
	let modalVisible = false;
	let modalCb: (arg1: boolean) => void;

	let mode = "Pending";
	let offset = 0;


	const getReports = async () => {
		const result = await request.get(`/reports/list?status=${mode}&offset=${offset}&sort=${mode === "Pending" ? "Asc" : "Desc"}`);
		return result.data;
	};

	$: {
		getReports().then((d) => {
            reports = d;
		});
	}

	const onAppStatusChange = async () => {
		if (reports && reports.length === 0 && mode === "Pending") {
			reports = null;
			getReports().then(d => {
				reports = d;
			})
		}
	};

	const rejectApp = async (app, mode) => {
		request
			.request({
				method: "POST",
				url: `/reports/${app.id}/${mode}`,
			})
			.then(() => {});
            reports = reports.filter((v) => v.id !== app.id);
		onAppStatusChange();
	};
	const acceptApp = async (app) => {
		request
			.request({
				method: "POST",
				url: `/reports/${app.id}/accept`,
			})
			.then((data) => {
				reports = reports.filter((v) => v.id !== app.id);
				onAppStatusChange();
			});
	};
</script>

<Main>
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
	<div class="row">
		<div class="col-12">
			<h3>Abuse Reports</h3>
		</div>

		{#if alert !== ""}
			<p class="text-center mt-4 mb-4">{alert}</p>
			<button
				class="btn btn-outline-warning mt-4"
				on:click={() => {
					alert = "";
				}}>Dismiss</button
			>
		{:else if reports && reports.length === 0}
			<div class="col-12">
				<p class="text-center">There are zero reports pending.</p>
			</div>
		{:else if reports && reports.length}
			<table class="table min-width-1000">
				<thead>
					<tr>
						<th>Submitted</th>
						<th>Reason</th>
						<th>Message</th>
						<th>Author</th>
						<th>Actions</th>
					</tr>
				</thead>
				<tbody>
					{#each reports as app}
						<tr>
							<td>{dayjs(app.createdAt).fromNow()}</td>
							<td>{app.reportReason}</td>
							<td>{app.reportMessage}</td>
							<td>
                                <a use:link href={`/admin/manage-user/${app.userId}`}>
                                    #{app.userId}
                                </a>
							</td>
							<td>
								<div class="btn-group w-100">
									{#if mode === "Pending"}
										<button
											class="btn btn-sm btn-outline-success w-100"
											on:click={() => {
												acceptApp(app);
											}}>Accept</button
										>
									{/if}

									<DropdownButton title='Reject'>
										<button class="dropdown-item" on:click={() => {
											rejectApp(app, 'decline');
										}}>Invalid</button>
										<button class="dropdown-item" on:click={() => {
											rejectApp(app, 'invalid');
										}}>Bad/Malicious</button>
									</DropdownButton>

								</div>
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
		{:else if !reports}
			<Loader />
		{/if}
	</div>
</Main>
