<script lang="ts">
    import Main from "../components/templates/Main.svelte";
    import client from "../lib/request";
    import request from "../lib/request";
    import dayjs from "dayjs";
    import Loader from "../components/misc/Loader.svelte";
    import type { AssetCommentType, ForumPostType, GroupStatus, GroupWallPost, TextPost, UserStatus } from "../lib/posts";
    import TextPostEntryDesktop from "../components/TextPostEntryDesktop.svelte";
    import TextPostMobile from "../components/TextPostMobile.svelte";

    let feedback :string|null = null;

    let stats: Record<string, number|null> = {
        pastMinute: null,
        past5Minutes: null,
        past30Minutes: null,
        pastHour: null,
        pastDay: null,
    };

    let postsLoading = true;
    let posts: TextPost[] = [];

    const getForumPosts = (): ForumPostType[] => {
        return posts.filter(v => v.type === 'ForumPost') as ForumPostType[];
    }
    const getComments = () => {
        return posts.filter(v => v.type === 'AssetComment') as AssetCommentType[];
    }
    const getGroupWallPosts = () => {
        return posts.filter(v => v.type === 'GroupWallPost') as GroupWallPost[];
    }
    const getUserStatusPosts = () => {
        return posts.filter(v => v.type === 'UserStatusPost') as UserStatus[];
    }
    const getGroupStatusPosts = () => {
        return posts.filter(v => v.type === 'GroupStatusPost') as GroupStatus[];
    }

    const getLatestClocks = () => {
        return request.get('/text-moderation/get-latest').then(d => d.data);
    }

    let offset = 0;
    let limit = 25;
    let disabled = false;
    const clockLsKey = `admin_forum_clock_v3`;
    let clock: Record<string, any> = localStorage.getItem(clockLsKey) ? JSON.parse(localStorage.getItem(clockLsKey)) : {};

    const resetClock = () => {
        clock = {};
        localStorage.removeItem(clockLsKey);
        loadContent();
    }

    const setClockToLatest = () => {
        getLatestClocks().then(data => {
            console.log('clocks are',data);
            clock = data;
            localStorage.setItem(clockLsKey, JSON.stringify(clock));
            loadContent();
        })
    }

    const clockLatest = () => {
                if (getForumPosts().length)
                    clock.ForumPost = getForumPosts()[getForumPosts().length - 1].postId;

                if (getComments().length)
                    clock.AssetComment = getComments()[getComments().length - 1].id;

                if (getGroupWallPosts().length)
                    clock.GroupWallPost = getGroupWallPosts()[getGroupWallPosts().length - 1].id;

                if (getUserStatusPosts().length)
                    clock.UserStatusPost = getUserStatusPosts()[getUserStatusPosts().length - 1].id;

                if (getGroupStatusPosts().length)
                    clock.GroupStatusPost = getGroupStatusPosts()[getGroupStatusPosts().length - 1].id;

                clock.UpdatedAt = new Date(Date.now()).toISOString();
                localStorage.setItem(clockLsKey, JSON.stringify(clock));
                loadContent();
                request.post('/text-moderation/request-payment', {}).then(result => {
                    const amount = result.data.robuxAmount;
                    if (amount) {
                        alert('You have been given '+amount+ ' Robux.');
                    }
                }).catch(e => {
                    alert('Error requesting payment: ' + e.message);
                })
    }

    const loadContent = () => {
        disabled = true;
        posts = [];
        Promise.all([
            client.request({
                method: 'GET',
                url: `/apisite/forums/v1/posts/list?limit=${limit}&offset=${offset}&sortOrder=asc&exclusiveStartId=${clock['ForumPost'] || ''}`,
                baseURL: '/',
            }).then(data => {
                let min = clock['ForumPost'] || 0;
                for (const item of data.data as ForumPostType[]) {
                    if (posts.length >= 100) {
                        break;
                    }
                    item.type = 'ForumPost';
                    posts.push(item);
                }
                // remove below minimum
                posts = posts.filter(v => v.type ==='ForumPost' ? v.postId > min : true);
            }),
            request.get(`/assets/comments?limit=${limit}&offset=${offset}&sortOrder=asc&exclusiveStartId=${clock['AssetComment'] || ''}`).then(d => {
                let min = clock['AssetComment'] || 0;
                for (const item of d.data as AssetCommentType[]) {
                    item.type = 'AssetComment';
                    if (item.id > min) {
                        posts.push(item);
                    }
                }
                // remove below minimum
                posts = posts.filter(v => v.type === 'AssetComment' ? v.id > min : true);
            }),
            request.get(`/groups/wall?limit=${limit}&offset=${offset}&sortOrder=asc&exclusiveStartId=${clock['GroupWallPost'] || 0}`).then(d => {
                let min = clock['GroupWallPost'] || 0;
                for (const item of d.data as GroupWallPost[]) {
                    item.type = 'GroupWallPost';
                    if (item.id > min) {
                        posts.push(item);
                    }
                }
                // remove below minimum
                posts = posts.filter(v => v.type === 'GroupWallPost' ? v.id > min : true);
            }),
            request.get(`/users/status?limit=${limit}&offset=${offset}&sortOrder=asc&exclusiveStartId=${clock['UserStatusPost'] || 0}`).then(d => {
                let min = clock['UserStatusPost'] || 0;
                for (const item of d.data as UserStatus[]) {
                    item.type = 'UserStatusPost';
                    if (item.id > min) {
                        posts.push(item);
                    }
                }
                // remove below minimum
                posts = posts.filter(v => v.type === 'UserStatusPost' ? v.id > min : true);
            }),
            request.get(`/groups/status?limit=${limit}&offset=${offset}&sortOrder=asc&exclusiveStartId=${clock['GroupStatusPost'] || 0}`).then(d => {
                let min = clock['GroupStatusPost'] || 0;
                for (const item of d.data as GroupStatus[]) {
                    item.type = 'GroupStatusPost';
                    if (item.id > min) {
                        posts.push(item);
                    }
                }
                // remove below minimum
                posts = posts.filter(v => v.type === 'GroupStatusPost' ? v.id > min : true);
            }),
        ]).finally(() => {
            disabled = false;
            postsLoading = false;
        })
    }
    loadContent();

    $: {
        client.request({
            method: 'GET',
            baseURL: '/',
            url: `/apisite/forums/v1/stats`,
        }).then(data => {
            stats = data.data;
        })
    }
</script>

<svelte:head>
	<title>Text Posts</title>
</svelte:head>

<Main>
	<div class="row">
        <div class="col-12 col-lg-6">
            <h3>Text Posts</h3>
        </div>
        <div class="col-12 col-lg-6 d-none d-lg-block">
            <p class='text-right mt-2 mb-0'><span class='text-success cursor-pointer' on:click={() => {
                clockLatest();
            }}>Done</span> | <span class="text-danger cursor-pointer" on:click={() => {
                resetClock();
            }}>Reset</span> | <span class="text-warning cursor-pointer" on:click={() => {
                setClockToLatest();
            }}>Set To Latest</span></p>
        </div>
        <div class="col-12">
            {#if feedback !== null}
                <p class='text-danger'>{feedback}</p>
            {/if}
        </div>
        <div class="col-12 d-lg-none">
            {#each posts as post}
                <TextPostMobile post={post} feedback={feedback} posts={posts} setPosts={(p) => {
                    posts = p;
                }} />
            {/each}
            <div class="mt-4 mb-4">
                {#if posts.length === 0}
                    <p class="mb-4">There are zero posts to show.</p>
                {:else}
                    <button class="btn btn-success" on:click={() => {
                        clockLatest();
                    }}>Done</button>
                {/if}
                <button class="btn btn-outline-danger" on:click={() => {
                    resetClock();
                }}>Reset</button>
                <button class="btn btn-outline-warning" on:click={() => {
                    setClockToLatest();
                }}>Set To Latest</button>
            </div>
        </div>
        <div class="col-12 d-lg-block d-none">
            <table class="table min-width-1000">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Title</th>
                        <th>Post</th>
                        <th>User</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {#each posts as post}
                        <TextPostEntryDesktop post={post} feedback={feedback} posts={posts} setPosts={(p) => {
                            posts = p;
                        }} />
                    {/each}
                </tbody>
            </table>
            {#if postsLoading}
                <Loader />
            {:else if posts.length === 0}
                <p class="text-center mt-4">There are no posts to show</p>
            {/if}
        </div>
        <div class="col-12">
			<nav aria-label="Page navigation example">
				<ul class="pagination">
					<li class={`page-item${disabled || !offset ? " disabled" : ""}`}>
						<a
							class="page-link"
							href="#!"
							on:click={(e) => {
								e.preventDefault();
								if (offset >= limit) {
									offset -= limit;
                                    loadContent();
								}
							}}>Previous</a
						>
					</li>
					<li class="page-item active">
						<a
							class="page-link"
							href="#!"
							on:click={(e) => {
								e.preventDefault();
							}}>{(offset / limit + 1).toLocaleString()}</a
						>
					</li>
					<li class={`page-item${disabled || (posts && posts.length < limit) ? " disabled" : ""}`}>
						<a
							class="page-link"
							href="#!"
							on:click={(e) => {
								e.preventDefault();
								offset += limit;
                                loadContent();
							}}>Next</a
						>
					</li>
				</ul>
			</nav>
		</div>
    </div>
</Main>

