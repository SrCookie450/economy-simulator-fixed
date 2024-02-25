/**
 * Create badge table.
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_badge', (t) => {
        // the id of the user
        t.bigInteger('user_id').notNullable().unsigned();
        // the id of the badge
        t.bigInteger('badge_id').notNullable().unsigned();
        // Date the badge was created
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        // Index on user
        t.index(['user_id']);
    });
};

/**
 * Drop the badge table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_badge');
};
