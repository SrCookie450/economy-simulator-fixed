/**
 * Add membership table
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    // group_economy
    await knex.schema.createTable('user_membership', (t) => {
        t.bigInteger('user_id').notNullable();
        t.integer('membership_type').notNullable();
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
		t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());

        t.unique(['user_id']);
    });
};

exports.down = async () => {
    await knex.schema.dropTable('user_membership');
};
