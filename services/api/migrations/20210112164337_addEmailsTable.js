// emails table

/**
 * Create user email table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_email', (t) => {
        // The emailId
        t.bigIncrements('id').notNullable().unsigned();
        // The userId
        t.bigInteger('user_id').notNullable().unsigned();
        // The email
        t.string('email', 255).notNullable();
        // Email status
        // 1 = AwaitingVerification, 2 = Verified
        t.integer('status').notNullable().unsigned().defaultTo(1).notNullable();
        // Date the email was created
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        // Date the email status was updated
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
};

/**
 * Drop the email table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_email');
};
