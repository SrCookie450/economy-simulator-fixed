/**
 * Add RAP column to asset
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('asset', (t) => {
        t.bigInteger('recent_average_price').nullable().unsigned().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('asset', (t) => {
        t.dropColumn('recent_average_price');
    });
};
