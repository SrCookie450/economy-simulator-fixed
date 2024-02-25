/**
 * Add chat table
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('user_conversation', (t) => {
        t.bigIncrements('id').notNullable();
        t.string('title', 255).nullable().defaultTo(null);
        t.bigInteger('creator_id').notNullable(); // user who created/initiated the chat
        t.integer('conversation_type').notNullable(); // 1=OneToOneConversation

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('user_conversation_participant', (t) => {
        t.bigInteger('conversation_id').notNullable();
        t.bigInteger('user_id').notNullable();

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.unique(['conversation_id', 'user_id']);
        t.index(['conversation_id']);
        t.index(['user_id']);
    });
    await knex.schema.createTable('user_conversation_message', (t) => {
        t.string('id', 64).notNullable();
        t.bigInteger('conversation_id').notNullable();
        t.bigInteger('user_id').notNullable();
        t.text('message').notNullable();

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        
        t.unique(['id']);
        t.index(['conversation_id']);
        t.index(['conversation_id', 'created_at']);
    });
    await knex.schema.createTable('user_conversation_message_read', (t) => {
        t.bigInteger('conversation_id').notNullable();
        t.bigInteger('user_id').notNullable();

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());

        t.unique(['conversation_id', 'user_id']);
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('user_conversation');
    await knex.schema.dropTable('user_conversation_participant');
    await knex.schema.dropTable('user_conversation_message');
    await knex.schema.dropTable('user_conversation_message_read');
};
