/**
 * Create user avatar table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_avatar', (t) => {
        // The userId
        t.bigInteger('user_id').notNullable().unsigned();
        // Custom aws bucket Thumbnail (or RBXCDN in development)
        t.string('thumbnail_url', 255).nullable();
        // 1 = r6, 2 = r15
        t.integer('avatar_type').notNullable().unsigned().defaultTo(1);

        // scales
        t.double('scale_height').notNullable().defaultTo(1);
        t.double('scale_width').notNullable().defaultTo(1);
        t.double('scale_head').notNullable().defaultTo(1);
        t.double('scale_depth').notNullable().defaultTo(1);
        t.double('scale_proportion').notNullable().defaultTo(0);
        t.double('scale_body_type').notNullable().defaultTo(0);
        // colors
        t.integer('head_color_id').notNullable().unsigned();
        t.integer('torso_color_id').notNullable().unsigned();
        t.integer('right_arm_color_id').notNullable().unsigned();
        t.integer('left_arm_color_id').notNullable().unsigned();
        t.integer('right_leg_color_id').notNullable().unsigned();
        t.integer('left_leg_color_id').notNullable().unsigned();

        t.unique(['user_id']);
    });
    await knex.schema.createTable('user_avatar_asset', t => {
        t.bigInteger('user_id').notNullable().unsigned();
        t.bigInteger('asset_id').notNullable().unsigned();

        t.index(['user_id']);
    });
};

/**
 * Drop the avatar table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTable('user_avatar');
    await knex.schema.dropTable('user_avatar_asset');
};
