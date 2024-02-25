/**
 * Add content id to asset versions - this will save space when repeated xml files are not needed (e.g. for shirts or pants)
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('asset_version', (t) => {
        t.string('content_url', 512).nullable().defaultTo(null).alter();
        t.bigInteger('content_id').nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('asset_version', (t) => {
        t.string('content_url', 512).notNullable().alter();
        t.dropColumn('content_id');
    });
};
