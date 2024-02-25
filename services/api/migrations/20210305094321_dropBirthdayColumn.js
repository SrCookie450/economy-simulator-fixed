/**
 * Drop birthday column for privacy reasons
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user_settings', (t) => {
        t.dropColumn('birthday');
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_settings', t => {
        t.dateTime('birthday').notNullable();
    })
};
