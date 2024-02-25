/**
 * Add various admin log tables
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('moderation_refund_transaction', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('transaction_id').notNullable();
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.bigInteger('user_id_one').notNullable();
        t.bigInteger('user_id_two').notNullable(); // user affected
        t.bigInteger('asset_id').nullable();
        t.bigInteger('user_asset_id').nullable();
        t.bigInteger('amount').notNullable();
        t.integer('currency_type').notNullable();
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('moderation_refund_transaction');
};
