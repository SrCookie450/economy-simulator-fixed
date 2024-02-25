/**
 * Create forums table
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
	await knex.schema.createTable('forum_post', (t) => {
		t.bigIncrements('id').notNullable().unsigned(); // post id
		t.bigInteger('user_id').notNullable().unsigned(); // user who made comment
		t.string('post', 1024).notNullable(); // post itself
		t.string('title', 255).nullable().defaultTo(null); // title, if thread
		t.bigInteger('thread_id').unsigned().nullable().defaultTo(null); // thread id, if is a reply
		t.integer('sub_category_id').notNullable().unsigned(); // the cat the post belongs in
		t.boolean('is_pinned').notNullable().defaultTo(false); // is the post pinned by a forum mod
		t.boolean('is_locked').notNullable().defaultTo(false); // is the post locked
		t.bigInteger('views').notNullable().defaultTo(0).unsigned();
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
		t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());
		// get thread or post
		t.index(['id']);
		// get replies to thread, sorted by id desc
		t.index(['thread_id', 'id']);
		// get posts by user, where created_at newer than x (for floodcheck)
		t.index(['user_id', 'created_at']);
		// get latest posts in specific sub category (sorted by id)
		t.index(['sub_category_id', 'id']);
	});
	// Posts marked as read
	await knex.schema.createTable('forum_post_read', (t) => {
		t.bigInteger('forum_post_id').unsigned().notNullable();
		t.bigInteger('user_id').unsigned().notNullable();
		t.unique(['forum_post_id', 'user_id']); // user can only read a post once
	});
};

/**
 * Drop the forums table
 * @param {import('knex')} knex
 */
exports.down = async (knex) => {
	await knex.schema.dropTable('forum_post');
	await knex.schema.dropTable('forum_post_read');
};
