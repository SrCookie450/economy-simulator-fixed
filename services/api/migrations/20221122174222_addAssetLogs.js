/**
 * Add various admin log tables
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('moderation_migrate_asset', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('asset_id').notNullable(); // asset created
        t.bigInteger('roblox_asset_id').notNullable(); // asset id from roblox
        t.bigInteger('actor_id').notNullable(); // user who performed the action

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('moderation_update_product', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.bigInteger('asset_id').notNullable();
        t.boolean('is_limited').notNullable();
        t.boolean('is_limited_unique').notNullable();
        t.boolean('is_for_sale').notNullable();
        t.integer('price_in_robux').nullable();
        t.integer('price_in_tickets').nullable();
        t.integer('max_copies').nullable();
        t.dateTime('offsale_at').nullable();
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('moderation_migrate_asset');
    await knex.schema.dropTable('moderation_update_product');
};
