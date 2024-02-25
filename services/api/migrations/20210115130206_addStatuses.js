/**
 * Create status table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_status', (t) => {
        t.bigIncrements('id').notNullable().unsigned();
        // user id
        t.bigInteger('user_id').notNullable().unsigned();
        t.string('status', 255).nullable(); // the status
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id']);
    });
};

/**
 * Drop the status table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_status');
};
