exports.up = async (knex) => {
    await knex.schema.createTable('user_invite', (t) => {
        t.string('id', 128).notNullable(); // app guid
        t.bigInteger('user_id').nullable().defaultTo(null); // null if app hasn't been used yet
        t.bigInteger('author_id').nullable().defaultTo(null); // staff member who accepted/declined the app
        
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('user_invite');
};