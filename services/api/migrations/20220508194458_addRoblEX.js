exports.up = async(knex) => {
    await knex.schema.createTable('trade_currency_order', (t) => {
		t.bigIncrements('id').notNullable(); // entry id
		t.bigInteger('user_id').notNullable();
		t.bigInteger('start_amount').notNullable(); // source_currency amount at start
        t.bigInteger('balance').notNullable(); // current balance of source_currency
        // amount of source_currency for destination_currency.
        // for example: exchange_rate of 2000, source currency of tix, destination of robux, would mean:
        // 2000 / 1000 = 2, 2 tix for 1 robux
        t.bigInteger('exchange_rate').notNullable();

        t.integer('source_currency').notNullable();
        t.integer('destination_currency').notNullable();

        t.boolean('is_closed').notNullable().defaultTo(false);

		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());
        t.dateTime('closed_at').nullable().defaultTo(null); // date position was manually closed, or the date it ended (e.g. due to balance being 0)
	});
    await knex.schema.createTable('trade_currency_log', (t) => {
		t.bigIncrements('id').notNullable(); // entry id
        t.bigInteger('order_id').notNullable(); // id of the order
		t.bigInteger('user_id').notNullable(); // user who bought the order

        t.bigInteger('source_amount').notNullable(); // amount of currency source
        t.bigInteger('destination_amount').notNullable(); // amount of currency dest

		t.dateTime('created_at').notNullable().defaultTo(knex.fn.now());
        t.dateTime('updated_at').notNullable().defaultTo(knex.fn.now());
	});
};

exports.down = async(knex)=> {
    await knex.schema.dropTable('trade_currency_order');
    await knex.schema.dropTable('trade_currency_log');
};
