/**
 * Add is_verified, remove matrix columns
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('join_application', (t) => {
        t.boolean('is_verified').notNullable().defaultTo(false); // whether the URL was verified
        t.string('verified_url', 512).nullable().defaultTo(null); // the url verified at the time of the app being sent

        // old columns we dont need anymore
        t.dropColumn('matrix_name');
        t.dropColumn('matrix_domain');
    });
};

exports.down = async () => {
    await knex.schema.alterTable('join_application', (t) => {
        t.dropColumn('is_verified');
        t.dropColumn('verified_url');

        t.string('matrix_name', 512).notNullable(); // first part of matrix name
        t.string('matrix_domain', 512).notNullable(); // matrix homeserver
    });
};
