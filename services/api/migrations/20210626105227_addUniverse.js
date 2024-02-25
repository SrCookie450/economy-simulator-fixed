/**
 * Add universes table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('universe', (t) => {
        // general info
        t.bigIncrements('id').notNullable();
        t.bigInteger('root_asset_id').unsigned().notNullable(); // aka root place id
        // configuration
        t.boolean('is_public').notNullable().defaultTo(false);
        // creator
        t.bigInteger('creator_id').unsigned().notNullable();
        t.integer('creator_type').notNullable();
        // dates
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());
    });
    await knex.schema.createTable('universe_asset', (t) => {
        t.bigInteger('universe_id').unsigned().notNullable();
        t.bigInteger('asset_id').unsigned().notNullable();
        t.index(['universe_id']);
        t.index(['asset_id']);
    });
    console.log('[info] migrate existing places');
    // add universes to existing places
    const existingPlaces = await knex('asset').select('*').where({ 'asset_type': 9 });
    for (const item of existingPlaces) {
        console.log('[info] give universe to', item.id);
        const uni = {
            root_asset_id: item.id,
            is_public: true,
            creator_id: item.creator_id,
            creator_type: item.creator_type,
        }
        const [id] = await knex('universe').insert(uni).returning('id');
        await knex('universe_asset').insert({
            universe_id: id,
            asset_id: item.id,
        });
    }
};

exports.down = async (knex) => {
    await knex.schema.dropTable('universe');
    await knex.schema.dropTable('universe_asset');
};
