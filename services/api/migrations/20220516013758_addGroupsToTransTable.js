/**
 * Add group_id column for group items
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('user_transaction', (t) => {
        t.bigInteger('group_id_one').nullable().defaultTo(null);
        t.bigInteger('group_id_two').nullable().defaultTo(null);
    });
    // group_economy
    await knex.schema.createTable('group_economy', (t) => {
        t.bigInteger('group_id').notNullable();
        t.integer('balance_robux').notNullable().unsigned();
        t.integer('balance_tickets').notNullable().unsigned();

        t.unique(['group_id']);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_transaction', (t) => {
        t.dropColumn('group_id_one');
        t.dropColumn('group_id_two');
    });
    await knex.schema.dropTable('group_economy');
};
