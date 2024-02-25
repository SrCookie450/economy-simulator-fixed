// user settings table

/**
 * Create user settings table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_settings', (t) => {
        // The userId
        t.bigInteger('user_id').notNullable().unsigned();
        // 1 = 'NoOne', 2 = 'Friends', 3 = 'FriendsAndFollowing', 4 = 'FriendsFollowingAndFollowers', 5 = 'AllAuthenticatedUsers', 6 = 'AllUsers'
        t.integer('inventory_privacy').notNullable().unsigned().defaultTo(1);
        // Light, Dark
        t.integer('theme').notNullable().unsigned().defaultTo(1);
        // 1 = Unknown, 2 = Male, 3 = Female
        t.integer('gender').notNullable().unsigned().defaultTo(3);
        // birthday
        t.dateTime('birthday').notNullable();

        t.unique(['user_id']);
    });
};

/**
 * Drop the settings table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_settings');
};
