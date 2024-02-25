/**
 * Create user assets table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_asset', (t) => {
        // the userAssetId
        t.bigIncrements('id').notNullable().unsigned();
        // The userId
        t.bigInteger('user_id').notNullable().unsigned();
        // The assetId
        t.bigInteger('asset_id').notNullable().unsigned();
        // the serial (if limited u)
        t.bigInteger('serial').nullable().defaultTo(null);
        // The current price (if collectible)
        // 0 = Not For Sale
        t.bigInteger('price').notNullable().unsigned().defaultTo(0);
        // Date the item was created
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        // Date the item was updated (e.g. owner changed)
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        // For getting owners/sellers
        t.index(['asset_id']);
        // For getting owned items
        t.index(['user_id']);
    });
};

/**
 * Drop the user assets table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_asset');
};
