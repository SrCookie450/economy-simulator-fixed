<script lang="ts">
	import Footer from "./Footer.svelte";
	import Header from "./Header.svelte";
	import SideNav from "./SideNav.svelte";
</script>


<style>
	main {
		min-height: 90vh;
	}
	div.container-fluid {
		min-height: 100vh;
	}
</style>

<Header />

<div class="container-fluid">
	<SideNav />
	<main class="col-md-9 ms-sm-auto col-lg-10 px-md-4 mt-4 flex-shrink-0">
		<slot />
	</main>
</div>
<div class="col-md-9 ms-sm-auto col-lg-10 mt-4">
	<Footer />
</div>

