using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Traits;
using StardewArchipelago.Archipelago.Gifting.StardewGifts;
using StardewArchipelago.Extensions;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftProcessor
    {
        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private StardewItemManager _itemManager;
        private Dictionary<string, Func<int, ItemAmount>> _specialItems;
        private Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>> _recognizedTraits;
        private Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>> _recognizedTrapTraits;

        public GiftProcessor(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _itemManager = itemManager;
            InitializeSpecialItems();
            InitializeRecognizedTraits();
            InitializeRecognizedTrapTraits();
        }

        public bool TryMakeStardewGift(Gift gift, out ReceivedGift itemName, out int amount)
        {
            var isTrap = gift.Traits.Any(x => x.Trait.Equals(GiftFlag.Trap, StringComparison.InvariantCultureIgnoreCase));
            if (isTrap)
            {
                itemName = null;
                amount = 0;

                return TryMakeGiftTrap(gift, out itemName, out amount);
            }
            
            if (TryMakeGiftFromDirectName(gift, out itemName, out amount)) return true;
            if (TryMakeGiftFromSpecialItems(gift, out itemName, out amount)) return true;
            if (TryMakeGiftFromTraitGroups(gift, out itemName, out amount)) return true;
            if (TryMakeGiftFromSingleTrait(gift, out itemName, out amount)) return true;

            itemName = null;
            amount = 0;
            return false;
        }

        private bool TryMakeGiftTrap(Gift gift, out ReceivedGift itemName, out int amount)
        {
            if (TryMakeGiftFromTrapTraitGroups(gift, out itemName, out amount)) return true;

            itemName = null;
            amount = 0;
            return false;
        }

        private bool TryMakeGiftFromTrapTraitGroups(Gift gift, out ReceivedGift itemName, out int amount)
        {
            foreach (var traitNumber in _recognizedTrapTraits.Keys.OrderByDescending(x => x))
            {
                foreach (var (traits, itemFunction) in _recognizedTrapTraits[traitNumber])
                {
                    if (traits.Any(x => !gift.Traits.Select(t => t.Trait).Contains(x)))
                    {
                        continue;
                    }

                    var traitsByName = gift.Traits.ToDictionary(t => t.Trait, t => t);
                    var itemAmount = itemFunction(gift.Amount, traitsByName);
                    itemName = new ItemGift(itemAmount.ItemName, gift.SenderSlot, _archipelago.GetPlayerName(gift.SenderSlot));
                    amount = itemAmount.Amount;
                    return true;
                }
            }

            itemName = null;
            amount = 0;
            return false;
        }

        private bool TryMakeGiftFromDirectName(Gift gift, out ReceivedGift itemName, out int amount)
        {
            if (_itemManager.ObjectExists(gift.ItemName))
            {
                itemName = new ItemGift(gift.ItemName, gift.SenderSlot, _archipelago.GetPlayerName(gift.SenderSlot));
                amount = gift.Amount;
                return true;
            }

            itemName = null;
            amount = 0;
            return false;
        }

        private bool TryMakeGiftFromSpecialItems(Gift gift, out ReceivedGift itemName, out int amount)
        {
            var capitalizedName = gift.ItemName.ToCapitalized();
            if (_specialItems.ContainsKey(capitalizedName))
            {
                var specialItem = _specialItems[capitalizedName](gift.Amount);
                itemName = new ItemGift(specialItem.ItemName, gift.SenderSlot, _archipelago.GetPlayerName(gift.SenderSlot));
                amount = specialItem.Amount;
                return true;
            }

            itemName = null;
            amount = 0;
            return false;
        }

        private bool TryMakeGiftFromTraitGroups(Gift gift, out ReceivedGift itemName, out int amount)
        {
            foreach (var traitNumber in _recognizedTraits.Keys.OrderByDescending(x => x))
            {
                foreach (var (traits, itemFunction) in _recognizedTraits[traitNumber])
                {
                    if (traits.Any(x => !gift.Traits.Select(t => t.Trait).Contains(x)))
                    {
                        continue;
                    }

                    var traitsByName = gift.Traits.ToDictionary(t => t.Trait, t => t);
                    var itemAmount = itemFunction(gift.Amount, traitsByName);
                    itemName = new ItemGift(itemAmount.ItemName, gift.SenderSlot, _archipelago.GetPlayerName(gift.SenderSlot));
                    amount = itemAmount.Amount;
                    return true;
                }
            }

            itemName = null;
            amount = 0;
            return false;
        }

        private bool TryMakeGiftFromSingleTrait(Gift gift, out ReceivedGift itemName, out int amount)
        {
            foreach (var trait in gift.Traits)
            {
                if (_itemManager.ObjectExists(trait.Trait))
                {
                    itemName = new ItemGift(trait.Trait, gift.SenderSlot, _archipelago.GetPlayerName(gift.SenderSlot));
                    amount = (int)Math.Round(trait.Quality * gift.Amount);
                    return true;
                }
            }

            itemName = null;
            amount = 0;
            return false;
        }

        private void InitializeSpecialItems()
        {
            _specialItems = new Dictionary<string, Func<int, ItemAmount>>();
            _specialItems.Add("Tree", (amount) => ("Wood", amount * 15));
            _specialItems.Add("Lumber", (amount) => ("Hardwood", amount * 15));
            _specialItems.Add("Boulder", (amount) => ("Stone", amount * 15));
            _specialItems.Add("Rock", (amount) => ("Stone", amount * 2));
            _specialItems.Add("Vine", (amount) => ("Fiber", amount * 2));
        }

        private void InitializeRecognizedTraits()
        {
            _recognizedTraits = new Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>>();
            InitializeSingleRecognizedTraits();
            InitializeDualRecognizedTraits();
            InitializeTrioRecognizedTraits();
        }

        private void InitializeTrioRecognizedTraits()
        {
            var trioRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();
            _recognizedTraits.Add(3, trioRecognizedTraits);
        }

        private void InitializeDualRecognizedTraits()
        {
            var dualRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();
            // Speed + Animal = Horse
            _recognizedTraits.Add(2, dualRecognizedTraits);
        }

        private void InitializeSingleRecognizedTraits()
        {
            var singleRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();

            singleRecognizedTraits.Add(new[] {GiftFlag.Speed}, MakeCoffee);
            singleRecognizedTraits.Add(new[] {"Fan"}, (amount, _) => ("Ornamental Fan", amount));

            _recognizedTraits.Add(1, singleRecognizedTraits);
        }

        private ItemAmount MakeCoffee(int amount, Dictionary<string, GiftTrait> traits)
        {
            var speedTrait = traits[GiftFlag.Speed];
            var totalSpeed = speedTrait.Duration + speedTrait.Quality - 1;
            return totalSpeed >= 3 ? ("Triple Shot Espresso", (int)Math.Round(amount * (totalSpeed / 3))) : ("Coffee", (int)Math.Round(amount * totalSpeed));
        }

        private void InitializeRecognizedTrapTraits()
        {
            _recognizedTraits = new Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>>();
            InitializeSingleRecognizedTrapTraits();
        }

        private void InitializeSingleRecognizedTrapTraits()
        {
            var singleRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();

            singleRecognizedTraits.Add(new[] { GiftFlag.Bomb }, MakeCoffee);

            _recognizedTraits.Add(1, singleRecognizedTraits);
        }
    }
}
