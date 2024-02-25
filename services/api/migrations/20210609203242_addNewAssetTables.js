/**
 * Create the new assets table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.dropTable('catalog_overrides');
    // asset table
    await knex.schema.createTable('asset', (t) => {
        // meta
        t.bigInteger('roblox_asset_id').nullable().unsigned().defaultTo(null);
        // general info
        t.bigIncrements('id').notNullable().unsigned();
        t.string('name').notNullable();
        t.string('description', 4096).nullable().defaultTo(null);
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
        t.smallint('asset_type').notNullable().unsigned();
        t.smallint('asset_genre').notNullable().unsigned();
        t.smallint('creator_type').notNullable().unsigned();
        t.bigInteger('creator_id').notNullable().unsigned();
        t.smallint('moderation_status').notNullable().unsigned(); // pending approval, accepted, declined, deleted, etc
        // economy
        t.boolean('is_for_sale').notNullable().defaultTo(false);
        t.bigInteger('price_robux').unsigned().nullable().defaultTo(null);
        t.bigInteger('price_tix').unsigned().nullable().defaultTo(null);
        t.boolean('is_limited').notNullable().defaultTo(false);
        t.boolean('is_limited_unique').notNullable().defaultTo(false);
        t.bigInteger('serial_count').nullable().unsigned().defaultTo(null);
        t.bigInteger('sale_count').notNullable().unsigned().defaultTo(0);
        t.dateTime("offsale_at").nullable().defaultTo(null);

        t.index(['roblox_asset_id']);
        t.index(['asset_type']);
        t.index(['is_limited']);
    });
    // asset versions
    await knex.schema.createTable('asset_version', (t) => {
        t.bigIncrements('id').notNullable().unsigned();
        t.bigInteger('asset_id').notNullable().unsigned();
        t.integer('version_number').notNullable().unsigned();
        t.string('content_url', 512).notNullable();
        t.bigInteger('creator_id').notNullable().unsigned();
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['asset_id']);
    });
    // asset thumbs
    await knex.schema.createTable('asset_thumbnail', (t) => {
        t.bigInteger('asset_id').notNullable().unsigned();
        t.bigInteger('asset_version_id').notNullable().unsigned();
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
        t.string('content_url', 512).notNullable();
        t.smallint('moderation_status').notNullable().unsigned();

        t.index(['asset_id']);
    });
};

/**
 * Drop the catalog overrides table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('asset');
    await knex.schema.dropTable('asset_version');
    await knex.schema.dropTable('asset_thumbnail');
    await require('./20210113123810_addCatalogTable').up(knex);
};
