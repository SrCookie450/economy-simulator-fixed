/**
 * Add connection column to the asset server
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('asset_server', (t) => {
        t.string('server_connection', 255).notNullable();
    });
};

exports.down = async () => {
    await knex.schema.alterTable('asset_server', (t) => {
        t.dropColumn('server_connection');
    });
};
