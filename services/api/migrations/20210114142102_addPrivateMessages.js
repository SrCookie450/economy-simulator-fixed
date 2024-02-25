/**
 * Create messages table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_message', (t) => {
        // message id
        t.bigIncrements('id').notNullable().unsigned();

        t.bigInteger('user_id_from').notNullable().unsigned();
        t.bigInteger('user_id_to').notNullable().unsigned();
        t.boolean('is_read'); // Did user_id_to read the message?
        t.string('subject', 255).notNullable();
        t.string('body', 1024).notNullable();
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
        t.boolean('is_archived').notNullable().defaultTo(false);

        t.index(['user_id_from']);
        t.index(['user_id_to']);
    });
};

/**
 * Drop the messages table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_message');
};
