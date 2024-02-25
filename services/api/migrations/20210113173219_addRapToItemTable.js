/**
 * Create catalog overrides RAP column
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('catalog_overrides', (t) => {
        // the average price
        t.bigInteger('rap').notNullable().unsigned().defaultTo(0);
    });
};

/**
 * Drop the catalog overrides RAP column
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.alterTable('catalog_overrides', t => {
        t.dropColumn('rap');
    });
};
