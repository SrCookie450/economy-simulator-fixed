namespace Roblox.Models.Users;

public class BadgeEntry
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }

    public BadgeEntry(int id, string name, string description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
    }
}

public class BadgesMetadata
{
    public static readonly List<BadgeEntry> Badges = new List<BadgeEntry>()
    {
        new BadgeEntry(18, "Welcome To The Club", "This badge is awarded to players who have ever belonged to the illustrious Builders Club. These players are part of a long tradition of Roblox greatness."),
        new BadgeEntry(11, "Builders Club", "Members of the illustrious Builders Club display this badge proudly. The Builders Club is a paid premium service. Members receive several benefits: they earn a daily income of 15 Robux, they can sell their creations to others in the Roblox Catalog, they get the ability to browse the web site without external ads, and they receive the exclusive Builders Club construction hat."),
        new BadgeEntry(15, "Turbo Builders Club", "Members of the exclusive Turbo Builders Club are some of the most dedicated Robloxians. The Turbo Builders Club is a paid premium service. Members receive many of the benefits received in the regular Builders Club, in addition to a few more exclusive upgrades: they earn a daily income of 35 Robux, they can sell their creations to others in the Roblox Catalog, they get the ability to browse the web site without external ads, they receive the exclusive Turbo Builders Club red site managers hat, and they receive an exclusive gear item."),
        new BadgeEntry(16, "Outrageous Builders Club", "Members of Outrageous Builders Club are VIP Robloxians. They are the cream of the crop. The Outrageous Builders Club is a paid premium service. Members receive 100 groups, 60 Robux per day, and many other benefits."),
        new BadgeEntry(1, "Administrator", "This badge identifies an account as belonging to a Roblox administrator. Only official Roblox administrators will possess this badge. If someone claims to be an admin, but does not have this badge, they are potentially trying to mislead you. If this happens, please report abuse and we will delete the imposter\'s account."),
        new BadgeEntry(12, "Veteran", "This badge recognizes members who have played Roblox for one year or more. They are stalwart community members who have stuck with us over countless releases, and have helped shape Roblox into the game that it is today. These medalists are the true steel, the core of the Robloxian history ... and its future."),
        new BadgeEntry(2, "Friendship", "This badge is given to players who have embraced the Roblox community and have made at least 20 friends. People who have this badge are good people to know and can probably help you out if you are having trouble."),
        new BadgeEntry(14, "Ambassador", "This badge was awarded during the Ambassador Program, which ran from 2009 to 2012. It has been retired and is no longer attainable."),
        new BadgeEntry(8, "Inviter", "This badge was awarded during the Inviter Program, which ran from 2009 to 2013. It has been retired and is no longer attainable."),
        new BadgeEntry(3, "Combat Initiation", "This badge was granted when a player scored 10 victories in games that use classic combat scripts. It was retired Summer 2015 and is no longer attainable."),
        new BadgeEntry(4, "Warrior", "This badge was granted when a player scored 100 or more victories in games that use classic combat scripts. It was retired Summer 2015 and is no longer attainable."),
        new BadgeEntry(5, "Bloxxer", "This badge was granted when a player scored at least 250 victories, and fewer than 250 wipeouts, in games that use classic combat scripts. It was retired Summer 2015 and is no longer attainable."),
    };
}