/**
 * Create asset comments table
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
	await knex.schema.createTable('asset_comment', (t) => {
		// comment id
		t.bigIncrements('id').notNullable().unsigned();
		t.bigInteger('asset_id').notNullable().unsigned(); // asset the comment is on
		t.bigInteger('user_id').notNullable().unsigned(); // user who made comment
		t.string('comment', 1024).notNullable(); // comment itself
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
		t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());
		// only query will be "select * from asset_comment where asset_id = x order by id desc"
		t.index(['asset_id', 'id']);
		// index for flood check
		t.index(['user_id']);
	});
};

/**
 * Drop the comments table
 * @param {import('knex')} knex
 */
exports.down = async (knex) => {
	await knex.schema.dropTable('asset_comment');
};
