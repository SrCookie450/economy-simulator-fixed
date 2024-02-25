/**
 * Create transactions table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_transaction', (t) => {
        // trx id
        t.bigIncrements('id').notNullable().unsigned();
        // type, like "Sale" or "Purchase"
        t.integer('type').notNullable().unsigned();
        // currency type
        t.integer('currency_type').notNullable().unsigned();
        // The amount of currency
        t.bigInteger('amount').notNullable();
        // json object containing further details regarding the purchase, such as asset ids or user asset ids involved
        t.string('details_json').notNullable().defaultTo('{}');
        // the id of the user
        // aka the user who initiated the money transfer
        t.bigInteger('user_id_one').notNullable().unsigned();
        // the id of the user who was on the other end
        t.bigInteger('user_id_two').notNullable().unsigned();
        // Date the trx was created
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        // Index on users
        t.index(['user_id_one']);
        t.index(['user_id_two']);
    });
    await knex.schema.createTable('collectible_sale_logs', (t) => {
        // trx id
        t.bigIncrements('id').notNullable().unsigned();
        // id of the item that sold
        t.bigInteger('asset_id').notNullable().unsigned();
        // The amount of currency
        t.bigInteger('amount').notNullable().unsigned();
        // date the sale happened
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        // Index on asset
        t.index(['asset_id']);
    });
};

/**
 * Drop the transactions table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_transaction');
    await knex.schema.dropTable('collectible_sale_logs');
};
