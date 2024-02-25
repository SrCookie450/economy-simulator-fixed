/**
 * Create mod logs tables
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
	await knex.schema.createTable('moderation_give_item', (t) => {
		t.bigIncrements('id').notNullable().unsigned(); // entry id
		t.bigInteger('user_id').notNullable().unsigned(); // user who got the item
		t.bigInteger('author_user_id').notNullable().unsigned(); // user who gave the item
		t.bigInteger('user_asset_id').notNullable().unsigned(); // user asset id
		t.bigInteger('user_id_from').unsigned().nullable().defaultTo(null); // original owner of the uaid (or null if created)
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
	});
	await knex.schema.createTable('moderation_give_robux', (t) => {
		t.bigIncrements('id').notNullable().unsigned(); // entry id
		t.bigInteger('user_id').notNullable().unsigned(); // user who got the robux
		t.bigInteger('author_user_id').notNullable().unsigned(); // user who gave the robux
		t.bigInteger('amount').notNullable().unsigned(); // amount of bobux
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
	});
};

/**
 * Drop the mod logs tables
 * @param {import('knex')} knex
 */
exports.down = async (knex) => {
	await knex.schema.dropTable('moderation_give_item');
	await knex.schema.dropTable('moderation_give_robux');
};
