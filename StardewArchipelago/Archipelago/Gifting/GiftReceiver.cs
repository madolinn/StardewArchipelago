using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Service;
using StardewArchipelago.Archipelago.Gifting.StardewGifts;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftReceiver
    {
        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private IGiftingService _giftService;
        private StardewItemManager _itemManager;
        private Mailman _mail;
        private GiftProcessor _giftProcessor;

        public GiftReceiver(IMonitor monitor, ArchipelagoClient archipelago, IGiftingService giftService, StardewItemManager itemManager, Mailman mail)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _giftService = giftService;
            _itemManager = itemManager;
            _mail = mail;
            _giftProcessor = new GiftProcessor(monitor, archipelago, itemManager);
        }

        public void ReceiveAllGifts()
        {
            var gifts = _giftService.GetAllGiftsAndEmptyGiftbox();
            if (!gifts.Any())
            {
                return;
            }

            var giftAmounts = new Dictionary<ReceivedGift, int>();
            var giftIds = new Dictionary<string, ReceivedGift>();
            foreach (var (id, gift) in gifts)
            {
                ParseGift(gift, giftAmounts, giftIds);
            }

            foreach (var (receivedGift, amount) in giftAmounts)
            {
                var relatedGiftIds = giftIds.Where(x => x.Value == receivedGift).Select(x => x.Key).ToArray();
                var senderGame = _archipelago.GetPlayerGame(receivedGift.SenderName);
                receivedGift.SendToPlayer(_mail, _itemManager, relatedGiftIds, senderGame, amount);
            }
        }

        private void ParseGift(Gift gift, Dictionary<ReceivedGift, int> giftAmounts, Dictionary<string, ReceivedGift> giftIds)
        {
            if (!_giftProcessor.TryMakeStardewGift(gift, out var stardewGift, out var amount))
            {
                if (!gift.IsRefund)
                {
                    _giftService.RefundGift(gift);
                }

                return;
            }

            if (!giftAmounts.ContainsKey(stardewGift))
            {
                giftAmounts.Add(stardewGift, 0);
            }

            giftAmounts[stardewGift] += amount;
            giftIds.Add(gift.ID, stardewGift);
        }
    }
}
