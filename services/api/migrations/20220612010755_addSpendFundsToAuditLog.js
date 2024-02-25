/**
 * Add group fund stuff to group_audit_log
 * @param {import('knex')} knex 
 */
 exports.up = async (knex) => {
    await knex.schema.alterTable('group_audit_log', (t) => {
        t.bigInteger('fund_recipient_user_id').nullable().defaultTo(null);
        t.bigInteger('currency_amount').nullable().defaultTo(null);
        t.integer('currency_type').nullable().defaultTo(null);
    });
};

exports.down = async () => {
    await knex.schema.alterTable('group_audit_log', (t) => {
        t.dropColumn('fund_recipient_user_id');
        t.dropColumn('currency_amount');
        t.dropColumn('currency_type');
    });
};
