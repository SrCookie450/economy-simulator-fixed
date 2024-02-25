const indexName = 'user_asset_asset_id';
/**
 * Add indexes that were lost in MySQL to PG Migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make user_asset index');
    await knex.raw(`CREATE INDEX "${indexName}" ON "user_asset" ("asset_id")`);
    console.timeEnd('make user_asset index');
};

exports.down = async (knex) => {
    await knex.raw(`DROP INDEX "${indexName}"`);
};
