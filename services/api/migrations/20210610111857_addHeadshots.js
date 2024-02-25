/**
 * Add player headshot urls
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('user_avatar', (t) => {
        t.string('headshot_thumbnail_url', 255).nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('user_avatar', (t) => {
        t.dropColumn('headshot_thumbnail_url');
    });
};
