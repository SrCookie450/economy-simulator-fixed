/**
 * Create user economy table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_economy', (t) => {
        // The userId
        t.bigInteger('user_id').notNullable().unsigned();
        // balances
        t.integer('balance_robux').notNullable().unsigned();
        t.integer('balance_tickets').notNullable().unsigned();

        t.unique(['user_id']);
    });
};

/**
 * Drop the economy table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_economy');
};
