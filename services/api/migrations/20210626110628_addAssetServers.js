/**
 * Add place data to asset
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('asset_server', (t) => {
        t.uuid('id').notNullable();
        t.bigInteger('asset_id').unsigned().notNullable();
        t.string('ip').notNullable();
        t.integer('port').notNullable().unsigned();
        t.index(['id']); // delete server by id, get server by id, etc
        t.index(['asset_id']); // get all game servers for place
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('asset_server_player', (t) => {
        t.uuid('server_id').notNullable(); // uuid of the server
        t.bigInteger('user_id').unsigned().notNullable();
        t.bigInteger('asset_id').unsigned().notNullable(); // place being played. this is only required to make counts fast
        t.index(['asset_id']); // count all players in asset_id
        t.index(['server_id']); // update/delete players
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_server');
    await knex.schema.dropTable('asset_server_player');
};
