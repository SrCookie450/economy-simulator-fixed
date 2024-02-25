<script lang="ts">
	export let cb: (didClickYes: boolean) => void;
	export let title: string;
	export let message: string;
	const clickHandler = (isYes: boolean) => {
		return (e) => {
			e.preventDefault();
			cb(isYes);
		};
	};
</script>

<div class="bg">
	<div class="row modalthing">
		<div class="col-12">
			<div class="card">
				<div class="card-body">
					<h1>{title}</h1>
					<p>{message}</p>
					<div class="btn-group">
						<button class="btn btn-success" on:click={clickHandler(true)}>Yes</button>
						<button class="btn btn-outline-danger" on:click={clickHandler(false)}>No</button>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>
