const fs = require('fs');
const path = require('path');
const groupsFolderLocation = path.join(__dirname, '../public/images/groups');
/**
 * Create group tables
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    // create thumbnails folder
    if (!fs.existsSync(groupsFolderLocation)) {
        fs.mkdirSync(groupsFolderLocation);
    }
    await knex.schema.createTable('group', (t) => {
        t.bigIncrements('id').notNullable().unsigned(); // group id
        t.bigInteger('user_id').nullable().unsigned(); // current group owner, or null if no owner
        t.boolean('locked').defaultTo(false).notNullable(); // is the group locked
        t.string('name', 255).notNullable(); // the group name
        t.string('description', 1024).notNullable(); // the group description
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id']);
        t.index(['name']);
    });
    await knex.schema.createTable('group_settings', t => {
        t.bigInteger('group_id').notNullable().unsigned(); // group id
        t.boolean('approval_required').notNullable().defaultTo(false);
        t.boolean('enemies_allowed').notNullable().defaultTo(false);
        t.boolean('funds_visible').notNullable().defaultTo(false);
        t.boolean('games_visible').notNullable().defaultTo(false);

        t.unique(['group_id']); // only one setting per group
    });
    await knex.schema.createTable('group_icon', t => {
        t.bigInteger('group_id').notNullable().unsigned(); // group id
        t.string('name', 255).notNullable(); // group icon filename
        t.integer('is_approved').notNullable().defaultTo(0); // if icon is visible. 0 = no (pending), 1 = yes, 2 = no (moderated)
        t.bigInteger('user_id').nullable().unsigned(); // user id who created the icon

        t.unique(['group_id']); // only one icon per group
    });
    await knex.schema.createTable('group_role', t => {
        t.bigIncrements('id').notNullable().unsigned(); // roleset id
        t.bigInteger('group_id').notNullable().unsigned(); // group id
        t.string('name', 255).notNullable(); // role name
        t.string('description', 255).notNullable(); // role description
        t.integer('rank').notNullable(); // group rank
        t.bigInteger('member_count').notNullable().defaultTo(0); // roleset member count

        t.index(['group_id']);
    });
    await knex.schema.createTable('group_role_permission', t => {
        t.bigInteger('group_role_id').notNullable().unsigned(); // group roleset id
        // group roleset permissions:
        t.boolean('delete_from_wall').notNullable().defaultTo(false);
        t.boolean('post_to_wall').notNullable().defaultTo(false);
        t.boolean('invite_members').notNullable().defaultTo(false);
        t.boolean('post_to_status').notNullable().defaultTo(false);
        t.boolean('remove_members').notNullable().defaultTo(false);
        t.boolean('view_status').notNullable().defaultTo(false);
        t.boolean('view_wall').notNullable().defaultTo(false);
        t.boolean('change_rank').notNullable().defaultTo(false);
        t.boolean('advertise_group').notNullable().defaultTo(false);
        t.boolean('manage_relationships').notNullable().defaultTo(false);
        t.boolean('add_group_places').notNullable().defaultTo(false);
        t.boolean('view_audit_logs').notNullable().defaultTo(false);
        t.boolean('create_items').notNullable().defaultTo(false);
        t.boolean('manage_items').notNullable().defaultTo(false);
        t.boolean('spend_group_funds').notNullable().defaultTo(false);
        t.boolean('manage_clan').notNullable().defaultTo(false);
        t.boolean('manage_group_games').notNullable().defaultTo(false);
    });
    await knex.schema.createTable('group_user', t => {
        t.bigIncrements('id').notNullable().unsigned(); // joined at 
        t.bigInteger('group_role_id').notNullable().unsigned(); // role id
        t.bigInteger('user_id').notNullable().unsigned(); // user id

        // select * from group_user where role  = 1 order by id desc
        t.index(['group_role_id', 'id']);
        // get groups user is in
        t.index(['user_id']);
    });
    await knex.schema.createTable('group_status', t => {
        t.bigIncrements('id').notNullable().unsigned();
        t.bigInteger('group_id').notNullable().unsigned(); // group who posted
        t.bigInteger('user_id').notNullable().unsigned(); // user who posted
        t.string('status', 255).nullable(); // the status
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['user_id']);
    });
    await knex.schema.createTable('group_wall', t => {
        t.bigIncrements('id').notNullable().unsigned(); // wall post id
        t.bigInteger('group_id').notNullable().unsigned(); // group id
        t.bigInteger('user_id').notNullable().unsigned(); // user id
        t.string('content', 1024).notNullable(); // wall post itself
        t.boolean('is_deleted').notNullable().defaultTo(false); // is the post deleted (by a group admin)?
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
    await knex.raw(`CREATE INDEX "group_wall_id_idx" ON "group_wall" ("group_id", "id") WHERE "is_deleted" IS FALSE`);
    await knex.schema.createTable('group_audit_log', t => {
        t.bigIncrements('id').notNullable().unsigned(); // audit log id
        t.bigInteger('group_id').notNullable().unsigned(); // group id
        t.bigInteger('user_id').notNullable().unsigned(); // user id
        t.integer('action').notNullable(); // action that happened

        // owner change
        t.bigInteger('new_owner_user_id').nullable().defaultTo(null);
        // rank change
        t.bigInteger('old_role_id').nullable().defaultTo(null);
        t.bigInteger('new_role_id').nullable().defaultTo(null);
        t.bigInteger('user_id_range_change').unsigned().nullable().defaultTo(null);
        // roleset rank update
        t.bigInteger('role_set_id').unsigned().nullable().defaultTo(null);
        t.integer('old_rank').nullable().defaultTo(null);
        t.integer('new_rank').nullable().defaultTo(null);

        // currently for roleset name updates but may be used for different strings in the future
        // uses role_set_id
        t.string('old_name').nullable().defaultTo(null);
        t.string('new_name').nullable().defaultTo(null);
        t.string('old_description').nullable().defaultTo(null);
        t.string('new_description').nullable().defaultTo(null);

        // post deletion
        t.string('post_desc').nullable().defaultTo(null);
        t.bigInteger('post_user_id').unsigned().nullable().defaultTo(null);

        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
};

/**
 * Drop the group tables
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    // remove old thumbnails
    if (fs.existsSync(groupsFolderLocation)) {
        for (const file of fs.readdirSync(groupsFolderLocation)) {
            let full = path.join(groupsFolderLocation, './' + file);
            fs.unlinkSync(full);
        }
        fs.rmdirSync(groupsFolderLocation);
    }
    // drop tables
    await knex.schema.dropTableIfExists('group');
    await knex.schema.dropTableIfExists('group_icon');
    await knex.schema.dropTableIfExists('group_role');
    await knex.schema.dropTableIfExists('group_role_permission');
    await knex.schema.dropTableIfExists('group_user');
    await knex.schema.dropTableIfExists('group_status');
    await knex.schema.dropTableIfExists('group_settings');
    await knex.schema.dropTableIfExists('group_wall');
    await knex.schema.dropTableIfExists('group_audit_log');
};
