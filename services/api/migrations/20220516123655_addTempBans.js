/**
 * Add expired_at col to user ban
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('user_ban', (t) => {
        t.dateTime("expired_at").nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_ban', (t) => {
        t.dropColumn('expired_at');
    });
};
