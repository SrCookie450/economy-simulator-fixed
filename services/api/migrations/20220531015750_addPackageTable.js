/**
 * Add package table
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.createTable('asset_package', (t) => {
        t.bigInteger('package_asset_id').notNullable(); // id of the package
        t.bigInteger('asset_id').notNullable(); // asset id

        t.unique(['package_asset_id', 'asset_id']); // asset id cannot appear in a package more than once
    });
};

exports.down = async () => {
    await knex.schema.dropTable('asset_package');
};
