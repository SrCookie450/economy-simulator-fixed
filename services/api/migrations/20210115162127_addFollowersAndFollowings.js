/**
 * Create followings table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_following', (t) => {
        // followings id
        t.bigIncrements('id').notNullable().unsigned();
        t.bigInteger('user_id_being_followed').notNullable().unsigned(); // user being followed
        t.bigInteger('user_id_who_is_following').notNullable().unsigned(); // user who made the request
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id_being_followed']);
        t.index(['user_id_who_is_following']);
    });
};

/**
 * Drop the followings table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_following');
};
