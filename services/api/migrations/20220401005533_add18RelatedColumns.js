/**
 * Add 18+ columns
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('user', (t) => {
        t.bool('is_18_plus').defaultTo(false).notNullable();
    });
    await knex.schema.alterTable('asset', (t) => {
        t.bool('is_18_plus').defaultTo(false).notNullable();
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user', (t) => {
        t.dropColumn('is_18_plus');
    });
    await knex.schema.alterTable('asset', (t) => {
        t.dropColumn('is_18_plus');
    });
};
