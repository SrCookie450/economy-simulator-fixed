/**
 * Add various admin log tables
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('moderation_manage_asset', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('asset_id').notNullable(); // asset
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.integer('action').notNullable(); // ModerationStatus

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('moderation_ban', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('user_id').notNullable(); // user affected
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.string('reason', 1024).notNullable();
        t.string('internal_reason', 1024).nullable();

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.dateTime('expired_at').nullable().defaultTo(null);
    });
    let currentBans = await knex('user_ban').select('user_id', 'author_user_id', 'reason', 'internal_reason', 'expired_at', 'created_at');
    for (let ban of currentBans) {
        await knex('moderation_ban').insert({
            user_id: ban.user_id,
            actor_id: ban.author_user_id,
            reason: ban.reason,
            internal_reason: ban.internal_reason,
            expired_at: ban.expired_at,
            created_at: ban.created_at,
        });
    }
    await knex.schema.createTable('moderation_unban', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('user_id').notNullable(); // user affected
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('moderation_admin_message', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('user_id').notNullable(); // user affected
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.string('subject', 1024).notNullable();
        t.string('body', 4096).notNullable();
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('moderation_set_alert', (t) => {
        t.bigIncrements('id').primary().notNullable();
        t.bigInteger('actor_id').notNullable(); // user who performed the action
        t.string('alert', 4096).nullable().defaultTo(null);
        t.string('alert_url', 4096).nullable().defaultTo(null);
        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('moderation_manage_asset');
    await knex.schema.dropTable('moderation_ban');
    await knex.schema.dropTable('moderation_unban');
    await knex.schema.dropTable('moderation_admin_message');
    await knex.schema.dropTable('moderation_set_alert');
};
