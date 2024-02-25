const indexNames = {
    forum_thread_id: 'forum_post_thread_id',
    forum_category: 'forum_post_subcategory_id',
    forum_category_id_desc: 'forum_post_subcategory_id_id_desc',
}
/**
 * Add index for asset searching
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    console.time('make forum_post_thread_id index');
    await knex.raw(`CREATE INDEX IF NOT EXISTS "forum_post_thread_id" ON "forum_post" ("thread_id") WHERE (thread_id IS NOT NULL)`);
    console.timeEnd('make forum_post_thread_id index');


    console.time('make forum_post_subcategory_id index');
    await knex.raw(`CREATE INDEX IF NOT EXISTS "forum_post_subcategory_id" ON "forum_post" ("sub_category_id")`);
    console.timeEnd('make forum_post_subcategory_id index');


    console.time('make forum_post_subcategory_id index');
    // CREATE INDEX IF NOT EXISTS "forum_post_subcategory_id_id_desc" ON "forum_post" ("sub_category_id", id desc);
    await knex.raw(`CREATE INDEX IF NOT EXISTS "forum_post_subcategory_id_id_desc" ON "forum_post" ("sub_category_id", "id" desc)`);
    console.timeEnd('make forum_post_subcategory_id index');
};

exports.down = async (knex) => {
    for (const indexName of Object.values(indexNames)) {
        await knex.raw(`DROP INDEX "${indexName}"`);
    }
};
