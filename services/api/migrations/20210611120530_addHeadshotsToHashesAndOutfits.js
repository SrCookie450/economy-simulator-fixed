/**
 * Add headshots to outfit entries
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user_outfit', (t) => {
        t.string('headshot_thumbnail_url', 255).nullable();
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_outfit', (t) => {
        t.dropColumn('headshot_thumbnail_url');
    });
};
