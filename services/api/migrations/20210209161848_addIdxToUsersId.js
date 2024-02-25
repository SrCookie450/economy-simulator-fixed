/**
 * Add indexes that were lost in MySQL to PG Migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {
    console.time('make user.id index');
    await knex.schema.alterTable('user', (t) => {
        t.index(['id']);
    });
    console.timeEnd('make user.id index');
};

exports.down = async () => {
    // No
};
