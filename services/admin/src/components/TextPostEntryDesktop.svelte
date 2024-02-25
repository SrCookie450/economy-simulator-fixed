<script lang="ts">
	import { link } from "svelte-routing";
	import { deletePost, TextPost } from "../lib/posts";

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
</script>

{#if post.type === "ForumPost"}
	<tr>
		<td>
			<a href={`/Forum/ShowPost.aspx?PostID=${post.threadId || post.postId}`}>
				{post.postId}
			</a>
		</td>
		<td>{post.title ? "Thread: " + post.title : "Post Reply to " + post.threadId}</td>
		<td>{post.post}</td>
		<td>
			<a use:link href={`/admin/manage-user/${post.userId}`}>
				{post.username}
			</a>
		</td>
		<td>
			<div class="btn-group btn-group-sm">
				<button
					class="btn btn-danger btn-sm"
					on:click={() => {
						deletePostUi(post);
					}}>Delete</button
				>
			</div>
		</td>
	</tr>
{:else if post.type === "AssetComment"}
	<tr>
		<td>
			<a href={`/catalog/${post.assetId}/--`}>
				{post.id}
			</a>
		</td>
		<td>Comment on <a href={`/catalog/${post.assetId}/--`}>{post.name}</a></td>
		<td>{post.comment}</td>
		<td>
			<a use:link href={`/admin/manage-user/${post.userId}`}>
				{post.username}
			</a>
		</td>
		<td>
			<div class="btn-group btn-group-sm">
				<button
					class="btn btn-danger btn-sm"
					on:click={() => {
						deletePostUi(post);
					}}>Delete</button
				>
			</div>
		</td>
	</tr>
{:else if post.type === "GroupWallPost"}
	<tr>
		<td>
			<a href={`/My/Groups.aspx?gid=${post.groupId}`}>
				{post.id}
			</a>
		</td>
		<td>Wall post on <a href={`/My/Groups.aspx?gid=${post.groupId}`}>#{post.groupId}</a></td>
		<td>{post.post}</td>
		<td>
			<a use:link href={`/admin/manage-user/${post.userId}`}>
				{post.username}
			</a>
		</td>
		<td>
			<div class="btn-group btn-group-sm">
				<button
					class="btn btn-danger btn-sm"
					on:click={() => {
						deletePostUi(post);
					}}>Delete</button
				>
			</div>
		</td>
	</tr>
{:else if post.type === "GroupStatusPost"}
	<tr>
		<td>
			<a href={`/My/Groups.aspx?gid=${post.group_id}`}>
				{post.id}
			</a>
		</td>
		<td>Status post on <a href={`/My/Groups.aspx?gid=${post.group_id}`}>{post.name}</a></td>
		<td>{post.status}</td>
		<td>
			<a use:link href={`/admin/manage-user/${post.user_id}`}>
				{post.username}
			</a>
		</td>
		<td>
			<div class="btn-group btn-group-sm">
				<button
					class="btn btn-danger btn-sm"
					on:click={() => {
						deletePostUi(post);
					}}>Delete</button
				>
			</div>
		</td>
	</tr>
{:else if post.type === "UserStatusPost"}
	<tr>
		<td>
			<a href={`/users/${post.userId}/profile`}>
				{post.id}
			</a>
		</td>
		<td>User status post by {post.username}</td>
		<td>{post.post}</td>
		<td>
			<a use:link href={`/admin/manage-user/${post.userId}`}>
				{post.username}
			</a>
		</td>
		<td>
			<div class="btn-group btn-group-sm">
				<button
					class="btn btn-danger btn-sm"
					on:click={() => {
						deletePostUi(post);
					}}>Delete</button
				>
			</div>
		</td>
	</tr>
{/if}
