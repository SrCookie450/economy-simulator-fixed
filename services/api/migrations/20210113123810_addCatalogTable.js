/**
 * Create catalog overrides table. This is meant for overriding Roblox data on Catalog items, such as limited status, copy count, etc
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
	await knex.schema.createTable('catalog_overrides', (t) => {
		// the assetId
		t.bigInteger('asset_id').notNullable().unsigned();
		// Is the item limited? Note: Limited U items must be limited AND limited u
		t.boolean('is_limited').notNullable().defaultTo(false);
		// Is the item for sale?
		t.boolean('is_for_sale').notNullable().defaultTo(false);
		// Limit how many copies can be purchased before the item goes offsale (or null if no limit)
		t.bigInteger('copies').nullable().defaultTo(null).unsigned();
		// Is the item limited u? Note: Limited U items must be limited AND limited u
		t.boolean('is_serialed').notNullable().defaultTo(false);
		// The current price, or null
		t.bigInteger('price').nullable().unsigned().defaultTo(0);
		// Date the item goes offsale, or null
		t.dateTime('offsale_at').nullable().defaultTo(null);
		// Date the item was created
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
		// Date the item was updated (e.g. put on sale)
		t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());
		// Index on assetId
		t.unique(['asset_id']);
	});
};

/**
 * Drop the catalog overrides table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
	await knex.schema.dropTable('catalog_overrides');
};
