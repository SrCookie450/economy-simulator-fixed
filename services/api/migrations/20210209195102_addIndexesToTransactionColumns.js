// used for tracking item sales for cs
const indexName = 'trx_user_asset_id_idx';
/**
 * Add indexes that were lost in MySQL to PG Migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make trx index');
    await knex.raw(`CREATE INDEX "${indexName}" ON "user_transaction" ("user_asset_id") WHERE user_asset_id IS NOT NULL;`);
    console.timeEnd('make trx index');
};

exports.down = async (knex) => {
    await knex.raw(`DROP INDEX "${indexName}"`);
};
