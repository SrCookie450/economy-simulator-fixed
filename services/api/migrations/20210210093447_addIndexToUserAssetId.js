/**
 * Add indexes that were lost in MySQL to PG Migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make user_asset.id index');
    await knex.schema.alterTable('user_asset', (t) => {
        t.index(['id']);
    });
    console.timeEnd('make user_asset.id index');
};

exports.down = async () => {
    // No
};
