/**
 * Add application logs
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.createTable('moderation_change_join_app', (t) => {
        t.bigIncrements('id').notNullable();
        t.string('application_id').notNullable();
        t.bigInteger('author_user_id').notNullable(); // staff id
        t.integer('new_status').notNullable();

		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async () => {
    await knex.schema.dropTable('moderation_change_join_app');
};
