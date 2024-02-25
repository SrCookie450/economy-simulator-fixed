/**
 * Add session_expired_at column
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user', (t) => {
        t.dateTime('session_expired_at').nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user', (t) => {
        t.dropColumn('session_expired_at');
    });
};
