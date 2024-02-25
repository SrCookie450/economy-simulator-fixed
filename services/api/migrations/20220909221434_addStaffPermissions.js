/**
 * Add user_permission table
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_permission', (t) => {
        t.bigInteger('user_id').notNullable(); // user
        t.integer('permission').notNullable(); // permission
        
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.unique(['user_id', 'permission']);
        t.index(['user_id']);
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('user_permission');
};
