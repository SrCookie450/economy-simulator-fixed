/**
 * Add comments enabled column to asset
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('asset', (t) => {
        t.boolean('comments_enabled').notNullable().defaultTo(false);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('asset', (t) => {
        t.dropColumn('comments_enabled');
    });
};
