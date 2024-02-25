/**
 * MySQL to PG Migration changed these to numeric, causing inner joins to be slow
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user_ban', (t) => {
        t.bigInteger('user_id').notNullable().alter(); // id of the user banned
        t.bigInteger('author_user_id').notNullable().alter(); // userid who did the ban
    });
};

exports.down = async () => {
    // No
};
