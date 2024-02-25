/**
 * Add indexes that were lost in MySQL to PG Migration
 * @param {import('knex')} knex 
 */
exports.up = async (knex) => {

    console.time('email index');
    await knex.schema.alterTable('user_email', t => {
        t.index(['user_id']);
        // used for getting users latest verified email. for example:
        // select email where user_id = 1 and status = verified order by id desc limit 1;
        t.index(['user_id', 'status']);
    });
    console.timeEnd('email index');

    console.time('settings index');
    await knex.schema.alterTable('user_settings', t => {
        t.index(['user_id']);
    });
    console.timeEnd('settings index');

    console.time('avatar index');
    await knex.schema.alterTable('user_avatar', t => {
        t.index(['user_id']);
    });
    console.timeEnd('avatar index');

    console.time('user economy idx');
    await knex.schema.alterTable('user_economy', t => {
        t.index(['user_id']);
    });
    console.timeEnd('user economy idx');
};

exports.down = async () => {
    // No
};
