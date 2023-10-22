using System.Collections.Generic;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Archipelago.Gifting.StardewGifts
{
    internal class ItemGift: ReceivedGift
    {
        public string ItemName { get; }

        public ItemGift(string itemName, int senderSlot, string senderName) : base(senderSlot, senderName)
        {
            ItemName = itemName;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ItemGift otherGift)
            {
                return false;
            }

            return ItemName.Equals(otherGift.ItemName) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ItemName.GetHashCode() ^ base.GetHashCode();
        }

        public static bool operator ==(ItemGift obj1, ItemGift obj2)
        {
            if (obj1 is null && obj2 is null)
            {
                return true;
            }

            if (obj1 is null || obj2 is null)
            {
                return false;
            }

            return obj1.Equals(obj2);
        }
        
        public static bool operator !=(ItemGift obj1, ItemGift obj2)
        {
            return !(obj1 == obj2);
        }

        public override void SendToPlayer(Mailman mail, StardewItemManager itemManager, string[] relatedGiftIds, string senderGame, int amount)
        {
            var mailKey = GetMailKey(relatedGiftIds);
            var item = itemManager.GetItemByName(ItemName);
            var embed = GetEmbed(item, amount);
            mail.SendArchipelagoGiftMail(mailKey, ItemName, SenderName, senderGame, embed);
        }

        private string GetEmbed(StardewItem item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return "";
            }

            return $"%item object {item.Id} {amount} %%";
        }

        private string GetMailKey(IEnumerable<string> ids)
        {
            return $"APGift;{string.Join(";", ids)}";
        }
    }
}
