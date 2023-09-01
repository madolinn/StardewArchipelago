﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Archipelago.Gifting
{
    internal class CrossGiftHandler : IGiftHandler
    {
        private static readonly string[] _desiredTraits = new[]
        {
            GiftFlag.Speed, GiftFlag.Wood, GiftFlag.Stone, GiftFlag.Consumable, GiftFlag.Food, GiftFlag.Drink,
            GiftFlag.Fish, GiftFlag.Heal, GiftFlag.Metal, GiftFlag.Seed
        };

        private static IMonitor _monitor;
        private StardewItemManager _itemManager;
        private Mailman _mail;
        private ArchipelagoClient _archipelago;
        private IGiftingService _giftService;
        private GiftSender _giftSender;
        private GiftReceiver _giftReceiver;

        public CrossGiftHandler()
        {
        }

        public void Initialize(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail)
        {
            if (!archipelago.SlotData.Gifting)
            {
                return;
            }

            _monitor = monitor;
            _itemManager = itemManager;
            _mail = mail;
            _archipelago = archipelago;
            _giftService = new GiftingService(archipelago.Session);
            _giftSender = new GiftSender(_monitor, _archipelago, _itemManager, _giftService);
            _giftReceiver = new GiftReceiver(_monitor, _archipelago, _giftService, _itemManager, _mail);

            _giftService.OpenGiftBox(true, _desiredTraits);
        }

        public bool HandleGiftItemCommand(string message)
        {
            if (_archipelago == null || !_archipelago.SlotData.Gifting)
            {
                return false;
            }

            var giftPrefix = $"{ChatForwarder.COMMAND_PREFIX}gift";
            var giftPrefixWithSpace = $"{giftPrefix} ";
            if (!message.StartsWith(giftPrefixWithSpace))
            {
                if (message.StartsWith(giftPrefix))
                {
                    Game1.chatBox?.addMessage($"Usage: !!gift [slotName]", Color.Gold);
                    return true;
                }
                return false;
            }

            var receiverSlotName = message.Substring(giftPrefixWithSpace.Length);
#if RELEASE
            if (receiverSlotName == _archipelago.SlotData.SlotName)
            {
                Game1.chatBox?.addMessage($"You cannot send yourself a gift", Color.Gold);
                return true;
            }
#endif
            _giftSender.SendGift(receiverSlotName);
            return true;
        }

        public void ReceiveAllGiftsTomorrow()
        {
            if (_archipelago == null || !_archipelago.SlotData.Gifting || !_archipelago.MakeSureConnected())
            {
                return;
            }

            _giftReceiver.ReceiveAllGifts();
        }

        public void ExportAllGifts(string filePath)
        {
            var allItems = _itemManager.GetAllItems();

            var items = new Dictionary<string, GiftTrait[]>();
            foreach (var item in allItems)
            {
                var stardewItem = item.PrepareForGivingToFarmer();
                if (stardewItem is not Object stardewObject)
                {
                    continue;
                }

                if (!_giftSender.GiftGenerator.TryCreateGiftItem(stardewObject, out var giftItem, out var traits))
                {
                    continue;
                }

                if (items.ContainsKey(giftItem.Name))
                {
                    continue;
                }
                items.Add(giftItem.Name, traits);
            }

            var objectsAsJson = JsonConvert.SerializeObject(items);
            File.WriteAllText(filePath, objectsAsJson);
        }
    }
}