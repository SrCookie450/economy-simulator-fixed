/**
 * Create previous usernames table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_previous_username', (t) => {
        t.bigIncrements('id').notNullable().unsigned();
        // user id
        t.bigInteger('user_id').notNullable().unsigned();
        t.string('username', 255).notNullable(); // the users name
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id']);
        t.index(['username']);
    });
};

/**
 * Drop the prev names table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_previous_username');
};
