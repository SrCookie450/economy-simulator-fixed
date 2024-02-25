const indexName = 'user_asset_lowest_price_assetid';
/**
 * Add indexes that were lost in MySQL to PG Migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make user_asset index');
    await knex.raw(`CREATE INDEX "${indexName}" ON "user_asset" ("asset_id", "price") WHERE "price" > 0 AND "price" IS NOT NULL`);
    console.timeEnd('make user_asset index');
};

exports.down = async (knex) => {
    await knex.raw(`DROP INDEX "${indexName}"`);
};
