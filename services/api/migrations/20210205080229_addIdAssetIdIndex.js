/**
 * Add index to user_asset to make pagination faster
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make_index');
    await knex.schema.alterTable('user_asset', (t) => {
        t.index(['id', 'asset_id']);
    });
    console.timeEnd('make_index');
};

/**
 * Drop the index added
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.alterTable('user_asset', t => {
        t.dropIndex(['id', 'asset_id']);
    });
};
