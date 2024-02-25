/**
 * Add verification_phrase column
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('join_application', (t) => {
        t.string('verification_phrase', 512).nullable().defaultTo(null); // the phrase used to verify
    });
};

exports.down = async () => {
    await knex.schema.alterTable('join_application', (t) => {
        t.dropColumn('verification_phrase');
    });
};
