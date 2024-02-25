/**
 * Add asset_favorite table
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.createTable('asset_favorite', (t) => {
        t.bigIncrements('id').notNullable(); // fav id
        t.bigInteger('user_id').notNullable(); // user id who favorited
        t.bigInteger('asset_id').notNullable(); // asset id favorited

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
		t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());

        t.unique(['user_id', 'asset_id']); // asset can only be favorited once. this index is also for getting favorite status for user and asset combo.
        t.index(['asset_id']); // get all favorites for asset
        t.index(['user_id']) ; // get all favorites by user
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_favorite');
};
