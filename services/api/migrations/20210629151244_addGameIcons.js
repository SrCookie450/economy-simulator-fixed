
exports.up = async (knex) => {
    // asset icons (almost identical to asset_thumbnail table right now)
    await knex.schema.createTable('asset_icon', (t) => {
        t.bigIncrements('id').notNullable(); // roblox actually has this for some reason...
        t.bigInteger('asset_id').notNullable().unsigned();
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
        t.string('content_url', 512).notNullable();
        t.smallint('moderation_status').notNullable().unsigned();

        t.index(['asset_id']);
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_icon');
};
