/**
 * MySQL to PG Migration changed these to numeric, causing inner joins to be slow
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('moderation_give_item', (t) => {
        t.bigInteger('user_id').notNullable().alter(); // user who got the item
        t.bigInteger('author_user_id').notNullable().alter(); // user who gave the item
        t.bigInteger('user_asset_id').notNullable().alter(); // user asset id
        t.bigInteger('user_id_from').nullable().defaultTo(null).alter(); // original owner of the uaid (or null if created)
    });
    await knex.schema.alterTable('moderation_give_robux', (t) => {
        t.bigInteger('user_id').notNullable().alter(); // user who got the robux
        t.bigInteger('author_user_id').notNullable().alter(); // user who gave the robux
        t.bigInteger('amount').notNullable().alter(); // amount of bobux
    });
};

exports.down = async () => {
    // No
};
