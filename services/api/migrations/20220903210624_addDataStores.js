/**
 * Add asset_favorite table
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.createTable('asset_datastore', (t) => {
        t.bigIncrements('id').notNullable(); // fav id
        t.bigInteger('asset_id').notNullable(); // place id
        t.bigInteger('universe_id').notNullable(); // uni id
        
        t.string('scope', 255).notNullable();
        t.string('key', 255).notNullable();
        t.string('name', 255).notNullable();
        t.string('value', 1024*1024).notNullable();

        t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_datastore');
};
