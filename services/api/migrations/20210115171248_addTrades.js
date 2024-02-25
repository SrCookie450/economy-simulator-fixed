/**
 * Create trades tables
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_trade', (t) => {
        t.bigIncrements('id').notNullable().unsigned(); // trade id
        t.bigInteger('user_id_one').notNullable().unsigned(); // user who created the trade
        t.bigInteger('user_id_two').notNullable().unsigned(); // user who was requested for the trade
        t.bigInteger('user_id_one_robux').unsigned().defaultTo(null); // robux user_id_one will give
        t.bigInteger('user_id_two_robux').unsigned().defaultTo(null); // robux user_id_two will give
        t.integer('status').notNullable().unsigned();
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("expires_at").notNullable();

        t.index(['user_id_one']);
        t.index(['user_id_two']);
    });
    await knex.schema.createTable('user_trade_asset', (t) => {
        t.bigInteger('trade_id').notNullable().unsigned(); // id of the trade this item is for
        t.bigInteger('user_asset_id').notNullable().unsigned(); // trade item
        t.bigInteger('user_id').notNullable().unsigned(); // which user id has the item
        t.index(['trade_id']);
    });
};

/**
 * Drop trades tables
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_trade');
    await knex.schema.dropTable('user_trade_asset');
};
