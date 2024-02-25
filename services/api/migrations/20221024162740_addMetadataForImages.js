/**
 * Add image asset metadata due to recent (potential) vulnerability
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('asset_version_metadata_image', (t) => {
        t.bigInteger('asset_version_id').notNullable().unique();
        t.integer('image_format').notNullable();
        t.integer('resolution_x').notNullable();
        t.integer('resolution_y').notNullable();
        t.integer('size_bytes').notNullable();
        t.string('hash', 64).notNullable();
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now()); // created_at so we can purge old validations if our validator turns out to be broken.
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_version_metadata_image');
};
