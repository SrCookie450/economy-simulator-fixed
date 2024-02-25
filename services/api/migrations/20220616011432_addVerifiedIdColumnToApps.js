/**
 * Add verified_id column
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('join_application', (t) => {
        t.string('verified_id', 512).nullable().defaultTo(null); // the id verified
    });
};

exports.down = async () => {
    await knex.schema.alterTable('join_application', (t) => {
        t.dropColumn('verified_id');
    });
};
