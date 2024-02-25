exports.up = async (knex) => {
    await knex.schema.createTable('moderation_bad_username_log', (t) => {
        t.bigIncrements('id').notNullable();
        t.string('username', 512).notNullable(); // the name reported
        t.bigInteger('user_id').notNullable(); // user who had the name
        t.bigInteger('author_id').notNullable(); // staff member who did the name change
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('moderation_bad_username_log');
};