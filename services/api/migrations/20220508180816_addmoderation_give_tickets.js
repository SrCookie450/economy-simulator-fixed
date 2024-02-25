
exports.up = async(knex) => {
    await knex.schema.createTable('moderation_give_tickets', (t) => {
		t.bigIncrements('id').notNullable().unsigned(); // entry id
		t.bigInteger('user_id').notNullable().unsigned(); // user who got the tix
		t.bigInteger('author_user_id').notNullable().unsigned(); // user who gave the tix
		t.bigInteger('amount').notNullable().unsigned(); // amount of tix
		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
	});
};

exports.down = async(knex)=> {
    await knex.schema.dropTable('moderation_give_tickets');

};
