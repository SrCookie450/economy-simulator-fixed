<script lang="ts">
    import { deletePost, TextPost } from "../lib/posts";
    import {link} from 'svelte-routing';

	export let post: TextPost;
	export let feedback: string;
	export let posts: TextPost[];
    export let setPosts: (p: TextPost[]) => void;

	const deletePostUi = (post: TextPost) => {
		console.log('[info] deletePostUi', post);
		deletePost(post, posts)
			.then((nPosts) => {
				setPosts(nPosts);
			})
			.catch((e) => {
				feedback = e.message;
			});
	};
    const onDelete = (p: TextPost) => {
        return e => {
            e.preventDefault();
            deletePostUi(p);
        }
    }
    const onIgnore = (p: TextPost) => {
        return e => {
            e.preventDefault();
            console.log('Ignore:',p);
            posts = posts.filter(v => {
                return v !== p;
            });
            setPosts(posts);
        }
    }
</script>

{#if post.type === "ForumPost"}
    <div class="row mb-4">
        <div class="col-12">
            <div class="card card-body">
                <p class="mb-0">{post.post}</p>
                <details>
                    {#if post.title}
                        <p class="fw-bold">Thread: {post.title}</p>
                    {:else}
                        <p class="fw-bold">Reply to <a target='_blank' href={`/Forum/ShowPost.aspx?PostID=${post.threadId}`}>{post.threadId}</a></p>
                    {/if}
                    <p class="mb-0">By <a use:link href={`/admin/manage-user/${post.userId}`}>
                        {post.username}
                    </a>
                    </p>
                    <summary class="pointer opacity-50">Details</summary>
                </details>
                <div>
                    <button class="btn btn-sm btn-danger mt-4" on:click={onDelete(post)}>Delete</button>
                </div>
            </div>
        </div>
    </div>
{:else if post.type === 'AssetComment'}
<div class="row mb-4">
    <div class="col-12">
        <div class="card card-body">
            <p class="mb-0">{post.comment}</p>
            <details>
                <p class="fw-bold">Comment on <a target='_blank' href={`/catalog/${post.assetId}/--`}>{post.name}</a></p>
                <p class="mb-0">By <a use:link href={`/admin/manage-user/${post.userId}`}>
                    {post.username}
                </a>
                </p>
                <summary class="pointer opacity-50">Details</summary>
            </details>
            <div>
                <button class="btn btn-sm btn-danger mt-4" on:click={onDelete(post)}>Delete</button>
            </div>
        </div>
    </div>
</div>
{:else if post.type === 'GroupStatusPost'}
<div class="row mb-4">
    <div class="col-12">
        <div class="card card-body">
            <p class="mb-0">{post.status}</p>
            <details>
                <p class="fw-bold">Group Status Post on <a target='_blank' href={`/My/Groups.aspx?gid=${post.group_id}`}>{post.name}</a></p>
                <p class="mb-0">By <a use:link href={`/admin/manage-user/${post.user_id}`}>
                    {post.username}
                </a>
                </p>
                <summary class="pointer opacity-50">Details</summary>
            </details>
            <div>
                <button class="btn btn-sm btn-danger mt-4" on:click={onDelete(post)}>Delete</button>
            </div>
        </div>
    </div>
</div>
{:else if post.type === 'GroupWallPost'}
<div class="row mb-4">
    <div class="col-12">
        <div class="card card-body">
            <p class="mb-0">{post.post}</p>
            <details>
                <p class="fw-bold">Group Wall Post on #<a target='_blank' href={`/My/Groups.aspx?gid=${post.groupId}`}>{post.groupId}</a></p>
                <p class="mb-0">By <a use:link href={`/admin/manage-user/${post.userId}`}>
                    {post.username}
                </a>
                </p>
                <summary class="pointer opacity-50">Details</summary>
            </details>
            <div>
                <button class="btn btn-sm btn-danger mt-4" on:click={onDelete(post)}>Delete</button>
            </div>
        </div>
    </div>
</div>
{:else if post.type === 'UserStatusPost'}
<div class="row mb-4">
    <div class="col-12">
        <div class="card card-body">
            <p class="mb-0">{post.post}</p>
            <details>
                <p class="fw-bold">User Status Post</p>
                <p class="mb-0">By <a use:link href={`/admin/manage-user/${post.userId}`}>
                    {post.username}
                </a>
                </p>
                <summary class="pointer opacity-50">Details</summary>
            </details>
            <div>
                <button class="btn btn-sm btn-danger mt-4" on:click={onDelete(post)}>Delete</button>
            </div>
        </div>
    </div>
</div>
{:else}
    <p class="fw-600 mt-4 mb-4">Unknown Post: {post ? JSON.stringify(post) : 'undefined'}. Please report this error.</p>
{/if}