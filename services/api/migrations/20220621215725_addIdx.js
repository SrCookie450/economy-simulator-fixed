const indexNames = {
    creator_and_is_for_sale: 'asset_is_for_sale_creator_idx',
    creator_and_is_for_sale_and_type: 'asset_is_for_sale_creator_type_idx',
    transaction_asset_counts: 'user_transaction_asset_type_sub_id',
}
/**
 * Add index for asset searching
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make asset index');
    await knex.raw(`CREATE INDEX "${indexNames.creator_and_is_for_sale}" ON "asset" ("creator_id", "creator_type") WHERE (is_for_sale OR is_limited)`);
    console.timeEnd('make asset index');
    console.time('make '+ indexNames.creator_and_is_for_sale);
    await knex.raw(`CREATE INDEX "${indexNames.creator_and_is_for_sale_and_type}" ON "asset" ("creator_id", "creator_type", "asset_type") WHERE (is_for_sale OR is_limited)`);
    console.timeEnd('make '+ indexNames.creator_and_is_for_sale);

    console.time('make ' + indexNames.transaction_asset_counts);
    await knex.raw(`CREATE INDEX "${indexNames.transaction_asset_counts}" ON "user_transaction" ("asset_id") WHERE (type = 1 AND sub_type = 1)`);
    console.timeEnd('make ' + indexNames.transaction_asset_counts);
    console.time('analyze');
    await knex.raw('ANALYZE asset');
    await knex.raw('ANALYZE user_transaction');
    console.timeEnd('analyze');
};

exports.down = async (knex) => {
    for (const indexName of Object.values(indexNames)) {
        await knex.raw(`DROP INDEX "${indexName}"`);
    }
};
