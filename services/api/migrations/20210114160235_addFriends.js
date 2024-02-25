/**
 * Create friends table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_friend', (t) => {
        // friend id
        t.bigIncrements('id').notNullable().unsigned();
        t.bigInteger('user_id_one').notNullable().unsigned(); // user who made the request
        t.bigInteger('user_id_two').notNullable().unsigned(); // other user
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id_one']);
        t.index(['user_id_two']);
    });
    await knex.schema.createTable('user_friend_request', (t) => {
        // friend request id
        t.bigIncrements('id').notNullable().unsigned();
        t.bigInteger('user_id_one').notNullable().unsigned(); // user who made the request
        t.bigInteger('user_id_two').notNullable().unsigned(); // other user
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id_one']);
        t.index(['user_id_two']);
    });
};

/**
 * Drop the friends table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_friend');
    await knex.schema.dropTable('user_friend_request');
};
