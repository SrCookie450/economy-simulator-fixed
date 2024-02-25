/**
 * Add asset media table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('asset_media', (t) => {
        t.bigIncrements('id').notNullable();
        t.integer('asset_type').notNullable();
        t.bigInteger('asset_id').unsigned().notNullable(); // ID of the asset the entry corresponds to
        t.bigInteger('media_asset_id').unsigned().nullable().defaultTo(null); // ID of the media (e.g. image id)
        t.string('media_video_hash', 128).nullable().defaultTo(null); // YouTube video hash (i.e. the "watch=hash_here" part)
        t.string('media_video_title', 128).nullable().defaultTo(null); // YouTube video title
        t.boolean('is_approved').notNullable().defaultTo(false); // Is the media approved (Applies to YouTube videos only, if media type is image then this will be equal to the asset's approval status)
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_media');
};
