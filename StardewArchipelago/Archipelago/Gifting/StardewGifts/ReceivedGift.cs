using StardewArchipelago.Items.Mail;
using System;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Archipelago.Gifting.StardewGifts
{
    public abstract class ReceivedGift
    {
        public int SenderSlot { get; }
        public string SenderName { get; }

        public ReceivedGift(int senderSlot, string senderName)
        {
            SenderSlot = senderSlot;
            SenderName = senderName;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ReceivedGift otherGift)
            {
                return false;
            }

            return SenderSlot == otherGift.SenderSlot && SenderName.Equals(otherGift.SenderName);
        }

        public override int GetHashCode()
        {
            return SenderSlot ^ SenderName.GetHashCode();
        }
        
        public static bool operator ==(ReceivedGift obj1, ReceivedGift obj2)
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
        
        public static bool operator !=(ReceivedGift obj1, ReceivedGift obj2)
        {
            return !(obj1 == obj2);
        }

        public abstract void SendToPlayer(Mailman mail, StardewItemManager itemManager, string[] relatedGiftIds, string senderGame, int amount);
    }
}
