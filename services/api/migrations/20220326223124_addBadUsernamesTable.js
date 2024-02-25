// this table is for when mods reset a bypass username - if we just reset the username, anybody can sign up again with the exact same username.
exports.up = async (knex) => {
    await knex.schema.createTable('moderation_bad_username', (t) => {
        t.bigIncrements('id').notNullable();
        t.string('username', 512).notNullable();
        
        t.index(['username']);
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('moderation_bad_username');
};