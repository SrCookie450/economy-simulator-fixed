/**
 * Add 18+ columns
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('user_message', (t) => {
        t.string('body', 8192).notNullable().alter();
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_message', (t) => {
        t.string('body', 1024).notNullable().alter();
    });
};
