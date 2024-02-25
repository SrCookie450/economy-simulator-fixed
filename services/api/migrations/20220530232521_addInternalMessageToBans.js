/**
 * Add internal_reason to user_ban
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('user_ban', (t) => {
        t.string('internal_reason', 4096).nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_ban', (t) => {
        t.dropColumn('internal_reason');
        t.dropColumn('verified_url');
    });
};
