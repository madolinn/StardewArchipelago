﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Stardew
{
    public class StardewItemManager
    {
        private Dictionary<int, StardewObject> _objectsById;
        private Dictionary<string, StardewObject> _objectsByName;
        private Dictionary<int, BigCraftable> _bigCraftablesById;
        private Dictionary<string, BigCraftable> _bigCraftablesByName;
        private Dictionary<int, StardewBoots> _bootsById;
        private Dictionary<string, StardewBoots> _bootsByName;
        private Dictionary<int, StardewFurniture> _furnitureById;
        private Dictionary<string, StardewFurniture> _furnitureByName;
        private Dictionary<int, StardewHat> _hatsById;
        private Dictionary<string, StardewHat> _hatsByName;
        private Dictionary<int, StardewWeapon> _weaponsById;
        private Dictionary<string, StardewWeapon> _weaponsByName;

        private List<int> _priorityIds = new List<int>()
        {
            390,
        };

        public Dictionary<int, string> ItemSuffixes = new Dictionary<int, string>()
        {
            {126, " (Green)"},
            {180, " (Brown)"},
            {182, " (Brown)"},
        };

        public StardewItemManager()
        {
            InitializeData();
        }

        public List<StardewItem> GetAllItems()
        {
            var allItems = new List<StardewItem>();
            allItems.AddRange(_objectsByName.Values);
            allItems.AddRange(_bigCraftablesByName.Values);
            allItems.AddRange(_bootsByName.Values);
            allItems.AddRange(_furnitureByName.Values);
            allItems.AddRange(_hatsByName.Values);
            allItems.AddRange(_weaponsByName.Values);
            return allItems;
        }

        public bool ItemExists(string itemName)
        {
            return _objectsByName.ContainsKey(itemName) || 
                   _bigCraftablesByName.ContainsKey(itemName) ||
                   _bootsByName.ContainsKey(itemName) ||
                   _furnitureByName.ContainsKey(itemName) ||
                   _hatsByName.ContainsKey(itemName) ||
                   _weaponsByName.ContainsKey(itemName);
        }

        public bool ItemExists(int itemId)
        {
            return _objectsById.ContainsKey(itemId) ||
                   _bigCraftablesById.ContainsKey(itemId) ||
                   _bootsById.ContainsKey(itemId) ||
                   _furnitureById.ContainsKey(itemId) ||
                   _hatsById.ContainsKey(itemId) ||
                   _weaponsById.ContainsKey(itemId);
        }

        public bool ObjectExists(int itemId)
        {
            return _objectsById.ContainsKey(itemId);
        }

        public bool ObjectExists(string itemName)
        {
            if (_objectsByName.ContainsKey(itemName))
            {
                return true;
            }

            return false; // This is where I should do something about complex items like wines and such
        }

        public StardewItem GetItemByName(string itemName)
        {
            if (_objectsByName.ContainsKey(itemName))
            {
                return _objectsByName[itemName];
            }

            if (_bigCraftablesByName.ContainsKey(itemName))
            {
                return _bigCraftablesByName[itemName];
            }

            if (_bootsByName.ContainsKey(itemName))
            {
                return _bootsByName[itemName];
            }

            if (_furnitureByName.ContainsKey(itemName))
            {
                return _furnitureByName[itemName];
            }

            if (_hatsByName.ContainsKey(itemName))
            {
                return _hatsByName[itemName];
            }

            if (_weaponsByName.ContainsKey(itemName))
            {
                return _weaponsByName[itemName];
            }

            throw new ArgumentException($"Item not found: {itemName}");
        }

        public StardewObject GetObjectById(int itemId)
        {
            if (_objectsById.ContainsKey(itemId))
            {
                return _objectsById[itemId];
            }

            throw new ArgumentException($"Item not found: {itemId}");
        }

        public StardewObject GetObjectByName(string itemName)
        {
            if (_objectsByName.ContainsKey(itemName))
            {
                return _objectsByName[itemName];
            }

            throw new ArgumentException($"Item not found: {itemName}");
        }

        public BigCraftable GetBigCraftableById(int itemId)
        {
            if (_bigCraftablesById.ContainsKey(itemId))
            {
                return _bigCraftablesById[itemId];
            }

            throw new ArgumentException($"Item not found: {itemId}");
        }

        public StardewBoots GetBootsById(int itemId)
        {
            if (_bootsById.ContainsKey(itemId))
            {
                return _bootsById[itemId];
            }

            throw new ArgumentException($"Item not found: {itemId}");
        }

        public StardewBoots[] GetAllBoots()
        {
            return _bootsByName.Values.ToArray();
        }

        public StardewFurniture GetFurnitureById(int itemId)
        {
            if (_furnitureById.ContainsKey(itemId))
            {
                return _furnitureById[itemId];
            }

            throw new ArgumentException($"Item not found: {itemId}");
        }

        public StardewHat GetHatById(int itemId)
        {
            if (_hatsById.ContainsKey(itemId))
            {
                return _hatsById[itemId];
            }

            throw new ArgumentException($"Item not found: {itemId}");
        }

        public StardewWeapon GetWeaponById(int weaponId)
        {
            if (_weaponsById.ContainsKey(weaponId))
            {
                return _weaponsById[weaponId];
            }

            throw new ArgumentException($"Weapon not found: {weaponId}");
        }

        public StardewWeapon GetWeaponByName(string weaponName)
        {
            if (_weaponsByName.ContainsKey(weaponName))
            {
                return _weaponsByName[weaponName];
            }

            throw new ArgumentException($"Weapon not found: {weaponName}");
        }

        public StardewWeapon[] GetAllWeapons()
        {
            return _weaponsByName.Values.ToArray();
        }

        private void InitializeData()
        {
            InitializeObjects();
            InitializeBigCraftables();
            InitializeBoots();
            // InitializeClothing(); var allClothingInformation = Game1.clothingInformation;
            InitializeFurniture();
            InitializeHats();
            // InitializeTools();
            InitializeWeapons();
        }

        private void InitializeObjects()
        {
            _objectsById = new Dictionary<int, StardewObject>();
            _objectsByName = new Dictionary<string, StardewObject>();
            var allObjectData = Game1.objectInformation;
            foreach (var (id, objectInfo) in allObjectData)
            {
                var stardewItem = ParseStardewObjectData(id, objectInfo);

                if (_objectsById.ContainsKey(id))
                {
                    continue;
                }

                if (_objectsByName.ContainsKey(stardewItem.Name))
                {
                    if (_priorityIds.Contains(id))
                    {
                        _objectsById.Add(id, stardewItem);
                        _objectsByName[stardewItem.Name] = stardewItem;
                    }

                    continue;
                }

                _objectsById.Add(id, stardewItem);
                _objectsByName.Add(stardewItem.Name, stardewItem);
                if (stardewItem.Name == "Cookie")
                {
                    _objectsByName.Add("Cookies", stardewItem);
                }
            }
        }

        private void InitializeBigCraftables()
        {
            _bigCraftablesById = new Dictionary<int, BigCraftable>();
            _bigCraftablesByName = new Dictionary<string, BigCraftable>();
            var allBigCraftablesInformation = Game1.bigCraftablesInformation;
            foreach (var (id, bigCraftableInfo) in allBigCraftablesInformation)
            {
                var bigCraftable = ParseStardewBigCraftableData(id, bigCraftableInfo);

                if (_bigCraftablesById.ContainsKey(id) || _bigCraftablesByName.ContainsKey(bigCraftable.Name))
                {
                    continue;
                }

                _bigCraftablesById.Add(id, bigCraftable);
                _bigCraftablesByName.Add(bigCraftable.Name, bigCraftable);
            }
        }

        private void InitializeBoots()
        {
            _bootsById = new Dictionary<int, StardewBoots>();
            _bootsByName = new Dictionary<string, StardewBoots>();
            var allBootsInformation = (IDictionary<int, string>)Game1.content.Load<Dictionary<int, string>>("Data\\Boots");
            foreach (var (id, bootsInfo) in allBootsInformation)
            {
                var boots = ParseStardewBootsData(id, bootsInfo);

                if (_bootsById.ContainsKey(id) || _bootsByName.ContainsKey(boots.Name))
                {
                    continue;
                }

                _bootsById.Add(id, boots);
                _bootsByName.Add(boots.Name, boots);
            }
        }

        private void InitializeFurniture()
        {
            _furnitureById = new Dictionary<int, StardewFurniture>();
            _furnitureByName = new Dictionary<string, StardewFurniture>();
            var allFurnitureInformation = (IDictionary<int, string>)Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
            foreach (var (id, furnitureInfo) in allFurnitureInformation)
            {
                var furniture = ParseStardewFurnitureData(id, furnitureInfo);

                if (_furnitureById.ContainsKey(id) || _furnitureByName.ContainsKey(furniture.Name))
                {
                    continue;
                }

                _furnitureById.Add(id, furniture);
                _furnitureByName.Add(furniture.Name, furniture);
                if (id % 2 == 0 && id >= 1838 && id <= 1854)
                {
                    _furnitureByName.Add($"Lupini: {furniture.Name}", furniture);
                    if (furniture.Name.StartsWith("'"))
                    {
                        _furnitureByName.Add($"Lupini: {furniture.Name.Substring(1, furniture.Name.Length - 2)}", furniture);
                    }
                }
            }
        }

        private void InitializeHats()
        {
            _hatsById = new Dictionary<int, StardewHat>();
            _hatsByName = new Dictionary<string, StardewHat>();
            var allHatsInformation = (IDictionary<int, string>)Game1.content.Load<Dictionary<int, string>>("Data\\hats");
            foreach (var (id, hatInfo) in allHatsInformation)
            {
                var hat = ParseStardewHatData(id, hatInfo);

                if (_hatsById.ContainsKey(id) || _hatsByName.ContainsKey(hat.Name))
                {
                    continue;
                }

                _hatsById.Add(id, hat);
                _hatsByName.Add(hat.Name, hat);
            }
        }

        private void InitializeWeapons()
        {
            _weaponsById = new Dictionary<int, StardewWeapon>();
            _weaponsByName = new Dictionary<string, StardewWeapon>();
            var allWeaponsInformation = (IDictionary<int, string>)Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
            foreach (var (id, weaponsInfo) in allWeaponsInformation)
            {
                var weapon = ParseStardewWeaponData(id, weaponsInfo);

                if (_weaponsById.ContainsKey(id) || _weaponsByName.ContainsKey(weapon.Name))
                {
                    continue;
                }

                _weaponsById.Add(id, weapon);
                _weaponsByName.Add(weapon.Name, weapon);
            }
        }

        private StardewObject ParseStardewObjectData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var sellPrice = int.Parse(fields[1]);
            var edibility = int.Parse(fields[2]);
            var typeAndCategory = fields[3].Split(" ");
            var objectType = typeAndCategory[0];
            var category = typeAndCategory.Length > 1 ? typeAndCategory[1] : "";
            var displayName = fields[4];
            var description = fields[5];

            name = NormalizeName(id, name);

            if (objectType == "Ring")
            {
                return new StardewRing(id, name, sellPrice, edibility, objectType, category, displayName, description);
            }

            return new StardewObject(id, name, sellPrice, edibility, objectType, category, displayName, description);
        }

        public string NormalizeName(int id, string name)
        {
            if (ItemSuffixes.ContainsKey(id))
            {
                name += ItemSuffixes[id];
            }

            return name;
        }

        private static BigCraftable ParseStardewBigCraftableData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var sellPrice = int.Parse(fields[1]);
            var edibility = int.Parse(fields[2]);
            var typeAndCategory = fields[3].Split(" ");
            var objectType = typeAndCategory[0];
            var category = typeAndCategory.Length > 1 ? typeAndCategory[1] : "";
            var description = fields[4];
            var outdoors = bool.Parse(fields[5]);
            var indoors = bool.Parse(fields[6]);
            var fragility = int.Parse(fields[7]);
            var displayName = fields.Last();

            var bigCraftable = new BigCraftable(id, name, sellPrice, edibility, objectType, category, description, outdoors, indoors, fragility, displayName);
            return bigCraftable;
        }

        private static StardewBoots ParseStardewBootsData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var description = fields[1];
            var sellPrice = int.Parse(fields[2]);
            var addedDefense = int.Parse(fields[3]);
            var addedImmunity = int.Parse(fields[4]);
            var colorIndex = int.Parse(fields[5]);
            var displayName = fields.Length > 6 ? fields[6] : name;

            var bigCraftable = new StardewBoots(id, name, sellPrice, description, addedDefense,
                addedImmunity, colorIndex, displayName);
            return bigCraftable;
        }

        private static StardewFurniture ParseStardewFurnitureData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var type = fields[1];
            var tilesheetSize = fields[2];
            var boundingBoxSize = fields[3];
            var price = fields[5];
            var rotations = fields.Length > 6 ? fields[6] : "1";
            var displayName = fields.Length > 7 ? fields[7] : name;
            var placementRestriction = fields.Length > 8 ? fields[8] : "";

            var furniture = new StardewFurniture(id, name, type, tilesheetSize, boundingBoxSize, rotations, price, displayName, placementRestriction);
            return furniture;
        }

        private static StardewHat ParseStardewHatData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var description = fields[1];
            var skipHairDraw = fields[2];
            var ignoreHairstyleOffset = bool.Parse(fields[3]);
            var displayName = fields.Length > 4 ? fields[4] : name;

            var hat = new StardewHat(id, name, description, skipHairDraw, ignoreHairstyleOffset, displayName);
            return hat;
        }

        private static StardewWeapon ParseStardewWeaponData(int id, string objectInfo)
        {
            var fields = objectInfo.Split("/");
            var name = fields[0];
            var description = fields[1];
            var minDamage = int.Parse(fields[2]);
            var maxDamage = int.Parse(fields[3]);
            var knockBack = double.Parse(fields[4]);
            var speed = double.Parse(fields[5]);
            var addedPrecision = double.Parse(fields[6]);
            var addedDefence = double.Parse(fields[7]);
            var type = int.Parse(fields[8]);
            var baseMineLevel = int.Parse(fields[9]);
            var minMineLevel = int.Parse(fields[10]);
            var addedAoe = double.Parse(fields[11]);
            var criticalChance = double.Parse(fields[12]);
            var criticalDamage = double.Parse(fields[13]);
            var displayName = fields.Length > 14 ? fields[14] : name;

            if (type == 4)
            {
                return new StardewSlingshot(id, name, description, minDamage, maxDamage, knockBack, speed,
                    addedPrecision, addedDefence, type, baseMineLevel, minMineLevel, addedAoe, criticalChance,
                    criticalDamage, displayName);
            }

            var meleeWeapon = new StardewWeapon(id, name, description, minDamage, maxDamage, knockBack, speed,
                addedPrecision, addedDefence, type, baseMineLevel, minMineLevel, addedAoe, criticalChance,
                criticalDamage, displayName);
            return meleeWeapon;
        }

        public void ExportAllItemsMatching(Func<Object, bool> condition, string filePath)
        {
            var objectsToExport = new List<string>();

            objectsToExport.AddRange(GetObjectsToExport(condition, _objectsByName));
            objectsToExport.AddRange(GetObjectsToExport(condition, _bigCraftablesByName));
            objectsToExport.AddRange(GetObjectsToExport(condition, _furnitureByName));
            objectsToExport.AddRange(GetObjectsToExport(condition, _hatsByName));
            objectsToExport.AddRange(GetObjectsToExport(condition, _bootsByName));
            objectsToExport.AddRange(GetObjectsToExport(condition, _weaponsByName));

            var objectsAsJson = JsonConvert.SerializeObject(objectsToExport);
            File.WriteAllText(filePath, objectsAsJson);
        }

        private IEnumerable<string> GetObjectsToExport<T>(Func<Object, bool> condition, Dictionary<string, T> objectsByName) where T : StardewItem
        {
            foreach (var (name, svItem) in objectsByName)
            {
                var stardewItem = svItem.PrepareForGivingToFarmer(1);
                if (stardewItem is not Object stardewObject)
                {
                    continue;
                }

                if (!condition(stardewObject))
                {
                    continue;
                }
                
                yield return name;
            }
        }
    }
}
