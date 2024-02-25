/**
 * Add recent visits to game
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('asset_play_history', (t) => {
        t.bigIncrements('id').notNullable();
        t.bigInteger('asset_id').unsigned().notNullable();
        t.bigInteger('user_id').unsigned().notNullable();
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("ended_at").nullable().defaultTo(null);
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_play_history');
};
