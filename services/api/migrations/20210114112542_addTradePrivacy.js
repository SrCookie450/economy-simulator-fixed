/**
 * Add MOAR SETTINGS (privacy and trade filter) column
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user_settings', (t) => {
        t.integer('trade_privacy').notNullable().unsigned().defaultTo(1);
        t.integer('trade_filter').notNullable().unsigned().defaultTo(1);
        t.integer('private_message_privacy').notNullable().unsigned().defaultTo(1);
    });
};

/**
 * Drop the cols added
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.alterTable('user_settings', t => {
        t.dropColumn('trade_privacy');
        t.dropColumn('trade_filter');
        t.dropColumn('private_message_privacy');
    });
};
