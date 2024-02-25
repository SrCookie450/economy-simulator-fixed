/**
 * Add locked_at and locked_by_user_id to apps
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('join_application', (t) => {
        t.dateTime("locked_at").nullable().defaultTo(null);
        t.bigInteger('locked_by_user_id').nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('join_application', (t) => {
        t.dropColumn('locked_at');
        t.dropColumn('locked_by_user_id');
    });
};
