/**
 * Create group social links table
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    await knex.schema.createTable('group_social_link', t => {
        t.bigIncrements('id').notNullable().unsigned(); // social id
        t.bigInteger('group_id').notNullable().unsigned(); // group id
        t.integer('type').notNullable(); // social link type
        t.string('url').notNullable(); // social link url
        t.string('title').notNullable(); // social link title

        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now());

        t.index(['group_id']);
    });
};

/**
 * Drop the socials table
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
    await knex.schema.dropTableIfExists('group_social_link');
};
