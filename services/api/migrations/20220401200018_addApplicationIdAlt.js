/**
 * Add 18+ columns
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('join_application', (t) => {
        t.string('join_id', 128).nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('join_application', (t) => {
        t.dropColumn('join_id');
    });
};
