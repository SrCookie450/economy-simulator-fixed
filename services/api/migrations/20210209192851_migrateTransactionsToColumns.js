const TypeStrToId = {
    'ItemPurchase': 1,
    'ItemSale': 2,
    'ItemResalePurchase': 3,
    'UsernameChange': 4,
    'ItemResale': 5,
    1: 'itemPurchase',
    2: 'ItemSale',
    3: 'ItemResalePurchase',
    4: 'UsernameChange',
    5: 'ItemResale',
}

/**
 * The transactions were created with this horrible JSON column idea that is being replaced by columns with this migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    // First, add columns
    await knex.schema.alterTable('user_transaction', (t) => {
        t.bigInteger('asset_id').nullable().unsigned().defaultTo(null);
        t.bigInteger('user_asset_id').nullable().unsigned().defaultTo(null);
        t.integer('sub_type').nullable().unsigned().defaultTo(null);
        t.string('old_username', 255).nullable().defaultTo(null);
        t.string('new_username', 255).nullable().defaultTo(null);
    });
    // Now, parse JSON and add data to columns
    const allTrans = await knex('user_transaction').select('*');
    for (const item of allTrans) {
        const details = JSON.parse(item.details_json);
        const typeId = TypeStrToId[details.type];

        if (typeof typeId !== 'number') {
            throw new Error('Unexpected typeId: "' + typeId + '" for transaction "' + item.id + '". details: ' + item.details_json);
        }
        // update
        await knex('user_transaction').update({
            'asset_id': details.assetId,
            'user_asset_id': details.userAssetId,
            'old_username': details.oldUsername,
            'new_username': details.newUsername,
            'sub_type': typeId,
        }).where({
            'id': item.id,
        })
    }
    // Finally, drop old JSON column
    await knex.schema.alterTable('user_transaction', (t) => {
        t.dropColumn('details_json');
    });
};

/**
 * Drop the cols added, and revert back to JSON
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    // add back json
    await knex.schema.alterTable('user_transaction', (t) => {
        t.string('details_json').notNullable().defaultTo('{}');
    });
    // get details and add to json
    const allTrans = await knex('user_transaction').select('*');
    for (const item of allTrans) {
        // Get the stringified id
        const typeStr = TypeStrToId[item.sub_type];
        if (typeof typeId !== 'string') {
            throw new Error('Unexpected trans type: "' + item.sub_type + '" for transaction "' + item.id + '"');
        }
        const asset = item.asset_id;
        const userAsset = item.user_asset_id;
        let json = null;
        if (item.old_username) {
            json = JSON.stringify({
                oldUsername: item.old_username,
                newUsername: item.old_username,
                type: typeStr,
            })
        } else {
            json = JSON.stringify({
                type: typeStr,
                assetId: asset,
                userAssetId: userAsset,
            });
        }
        // update
        await knex('user_transaction').update({
            'details_json': json,
        }).where({
            'id': item.id,
        })
    }
    // drop old columns
    await knex.schema.alterTable('user_transaction', (t) => {
        t.dropColumn('asset_id');
        t.dropColumn('user_asset_id');
        t.dropColumn('sub_type');
        t.dropColumn('old_username');
        t.dropColumn('new_username');
    });
};
