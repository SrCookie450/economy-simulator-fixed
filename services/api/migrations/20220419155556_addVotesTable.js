exports.up = async (knex) => {
    await knex.schema.createTable('asset_vote', (t) => {
        t.bigIncrements('id').notNullable();
        t.bigInteger('user_id').notNullable();
        t.bigInteger('asset_id').notNullable();
        t.integer('type').notNullable(); // 1 = Upvote, 2 = Downvote
        
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_vote');
};