/**
 * Add created_at to the outfit table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user_outfit', (t) => {
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_outfit', (t) => {
        t.dropColumn('created_at');
    });
};
