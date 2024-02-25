/**
 * Add session key so that sessions can be invalidated easily
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
	await knex.schema.alterTable('user', (t) => {
		t.integer('session_key').notNullable().unsigned().defaultTo(0);
	});
};

/**
 * Drop the cols added
 * @param {import('knex')} knex 
 */
exports.down = async (knex) => {
	await knex.schema.alterTable('user', t => {
		t.dropColumn('session_key');
	});
};
