/**
 * Add abuse report table
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.createTable('abuse_report', (t) => {
        t.string('id', 128).notNullable(); // report guid
        t.bigInteger('user_id').notNullable(); // user id who reported
        t.bigInteger('author_id').nullable().defaultTo(null); // user id who edited
        t.integer('report_reason').notNullable(); // report reason enum
        t.integer('report_status').notNullable(); // report status (default "Pending")
        t.string('report_message', 1024).notNullable(); // reason the user put
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
		t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());

        t.unique(['id']);
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('abuse_report');
};
