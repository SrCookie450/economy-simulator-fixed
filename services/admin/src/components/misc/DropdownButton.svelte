<script lang="ts">
    let isOpen = false;
    export let title: string;

</script>


<div class="btn-group">
    <button type="button" class="btn btn-outline-danger dropdown-toggle" data-toggle="dropdown" aria-expanded="false" on:click={() => {
        isOpen = !isOpen;
    }}>
      {title}
    </button>
    {#if isOpen}
        <div class="dropdown-menu dropdown-menu-open">
            <slot />
        </div>
    {:else}
        <div class="dropdown-menu">
            <slot />
        </div>
    {/if}
</div>