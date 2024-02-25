// interesting notes:
// - bidding used to be done in tix until tix were removed. I added both "robux" and "tix" columns in case we decide to add tix support in the future (and so we don't break existing ads by adding tix support).
// - roblox doesn't seem to store transactions for ad spending, or if they do, it's stored internally somewhere and not visible to normal users. I think we'll just store it in transactions table under a special type so it's not visible normally, but can be visible to mods
exports.up = async (knex) => {
    await knex.schema.createTable('asset_advertisement', (t) => {
        // is_active = (updated_at >= (current_time() - 1 day))
        t.bigIncrements('id').notNullable();
        t.bigInteger('target_id').notNullable().unsigned(); // id of the thing being advertised
        t.smallint('target_type').notNullable(); // type of thing being advertised
        t.dateTime("created_at").notNullable().defaultTo(knex.fn.now());
        t.dateTime("updated_at").notNullable().defaultTo(knex.fn.now()); // when ad was started
        // t.boolean('is_running').notNullable().defaultTo(false);
        t.smallint('advertisement_type').notNullable(); // ad type, e.g. banner or square or whatever
        t.bigInteger('advertisement_asset_id').notNullable(); // assetId of the Image for the advertisement. get moderation status by inner joining asset and reading that status for the image
        t.string('name', 512).notNullable();

        // i know it's ugly to put all the below stuff in one table, but it might be how Roblox does it?
        // you can't actually view historical data for Roblox ads, just all time and previous (or current) run
        t.bigInteger('impressions_all').notNullable().defaultTo(0);
        t.bigInteger('clicks_all').notNullable().defaultTo(0);
        t.bigInteger('bid_amount_tix_all').notNullable().defaultTo(0);
        t.bigInteger('bid_amount_robux_all').notNullable().defaultTo(0);
        // last_run can also refer to the current run, if the ad is active
        t.bigInteger('impressions_last_run').notNullable().defaultTo(0);
        t.bigInteger('clicks_last_run').notNullable().defaultTo(0);
        t.bigInteger('bid_amount_robux_last_run').notNullable().defaultTo(0);
        t.bigInteger('bid_amount_tix_last_run').notNullable().defaultTo(0);
        // finally, indexes
        t.index(['target_id', 'target_type']);
        t.index(['updated_at']); // web will just "select * from ad where updated_at >= (now() - 1 day)", then sort everything in memory, right?
    });
};

exports.down = async (knex) => {
    await knex.schema.dropTable('asset_advertisement');
};
