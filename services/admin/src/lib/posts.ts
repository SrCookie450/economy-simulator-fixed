import request from "./request";

export type ForumPostType = { type: 'ForumPost'; postId: number; userId: number; username: string; post: string; threadId: number | null; title: string | null; subCategoryId: number };
export type AssetCommentType = { type: 'AssetComment'; id: number; name: string; assetId: number; username: string; comment: string; userId: number };
export type GroupWallPost = { type: 'GroupWallPost'; id: number; post: string; groupId: number; userId: number; username: string };
export type UserStatus = { type: 'UserStatusPost'; id: number; post: string; userId: number; username: string; }
export type GroupStatus = { type: 'GroupStatusPost'; id: number; status: string; group_id: number; user_id: number; name: string; username: string; };

export type TextPost = (ForumPostType | AssetCommentType | GroupWallPost | UserStatus | GroupStatus);

export const deletePost = async (post: TextPost, posts: TextPost[]): Promise<TextPost[]> => {
    if (post.type === 'ForumPost') {
        await request
            .request({
                method: "DELETE",
                url: "/apisite/forums/v1/posts/" + post.postId,
                baseURL: "/",
            })
        post.post = "[ Content Deleted ]";
        if (post.title) {
            post.title = "[ Content Deleted ]";
        }
    } else if (post.type === 'AssetComment') {
        await request
            .request({
                method: "DELETE",
                url: "/user/comment?userId=" + post.userId + "&commentId=" + post.id,
            });
        post.comment = '[ Content Deleted ]';
    } else if (post.type === 'GroupWallPost') {
        await request
            .request({
                method: "POST",
                url: "/groups/wall/remove?id=" + post.id,
            })
        post.post = '[ Content Deleted ]';
    } else if (post.type === 'GroupStatusPost') {
        await request
            .request({
                method: "POST",
                url: "/groups/status/delete?id=" + post.id,
            })
        post.status = '[ Content Deleted ]';
    } else if (post.type === 'UserStatusPost') {
        await request
            .request({
                method: "DELETE",
                url: "/user/status?statusId=" + post.id + "&userId=" + post.userId,
            })
        post.post = '[ Content Deleted ]';
    }

    return [...posts];
}