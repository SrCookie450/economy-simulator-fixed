/**
 * Add pass reset logs
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_password_reset', (t) => {
        t.string('id', 64).primary().notNullable();
        t.bigInteger('user_id').notNullable();
        t.integer('status').notNullable();
        t.string('social_url', 1024).notNullable();
        t.string('verification_phrase', 1024).notNullable();
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('user_password_reset');
};
