﻿using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class ArcadeMachineInjections
    {
        private const string JK_EXTRA_LIFE = "Junimo Kart: Extra Life";
        private const string JK_LEVEL_LOCATION = "Junimo Kart: Level {0}";
        private const string JK_VICTORY = "Junimo Kart Victory";

        private const string JOTPK_BOOTS_1 = "JotPK: Boots 1";
        private const string JOTPK_BOOTS_2 = "JotPK: Boots 2";
        private const string JOTPK_GUN_1 = "JotPK: Gun 1";
        private const string JOTPK_GUN_2 = "JotPK: Gun 2";
        private const string JOTPK_GUN_3 = "JotPK: Gun 3";
        private const string JOTPK_SUPER_GUN = "JotPK: Super Gun";
        private const string JOTPK_AMMO_1 = "JotPK: Ammo 1";
        private const string JOTPK_AMMO_2 = "JotPK: Ammo 2";
        private const string JOTPK_AMMO_3 = "JotPK: Ammo 3";
        private const string JOTPK_DROP_RATE = "JotPK: Increased Drop Rate";
        private const string JOTPK_PROGRESSIVE_BOOTS = "JotPK: Progressive Boots";
        private const string JOTPK_PROGRESSIVE_GUN = "JotPK: Progressive Gun";
        private const string JOTPK_PROGRESSIVE_AMMO = "JotPK: Progressive Ammo";
        private const string JOTPK_EXTRA_LIFE = "JotPK: Extra Life";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static int _bootsLevel;
        private static int _gunLevel;
        private static int _ammoLevel;
        private static int _bootsItemOffered = -1;
        private static int _gunItemOffered = -1;
        private static int _ammoItemOffered = -1;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static void RestartLevel_NewGame_Postfix(MineCart __instance, bool new_game)
        {
            try
            {
                var livesLeftField = _helper.Reflection.GetField<int>(__instance, "livesLeft");
                var livesLeft = livesLeftField.GetValue();
                if (livesLeft != 3 || !new_game)
                {
                    return;
                }
                var numberExtraLives = GetJunimoKartExtraLives();
                livesLeftField.SetValue(livesLeft + numberExtraLives);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(RestartLevel_NewGame_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void UpdateFruitsSummary_ExtraLives_Postfix(MineCart __instance, float time)
        {
            try
            {
                SendJunimoKartLevelsBeatChecks(__instance);

                var livesLeftField = _helper.Reflection.GetField<int>(__instance, "livesLeft");
                var livesLeft = livesLeftField.GetValue();
                var numberExtraLives = GetJunimoKartExtraLives();
                if (livesLeft >= 3 + numberExtraLives)
                {
                    return;
                }
                livesLeftField.SetValue(3 + numberExtraLives);

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(UpdateFruitsSummary_ExtraLives_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static bool EndCutscene_JunimoKartLevelComplete_Prefix(MineCart __instance)
        {
            try
            {
                SendJunimoKartLevelsBeatChecks(__instance);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(EndCutscene_JunimoKartLevelComplete_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void SendJunimoKartLevelsBeatChecks(MineCart __instance)
        {
            var gamemode = _helper.Reflection.GetField<int>(__instance, "gameMode");
            var levelsBeat = _helper.Reflection.GetField<int>(__instance, "levelsBeat");
            if (gamemode.GetValue() != 3 || levelsBeat.GetValue() < 1)
            {
                return;
            }

            if (levelsBeat.GetValue() < 6)
            {
                _locationChecker.AddCheckedLocation(string.Format(JK_LEVEL_LOCATION, levelsBeat.GetValue()));
                return;
            }

            _locationChecker.AddCheckedLocation(JK_VICTORY);
        }

        private static int GetJunimoKartExtraLives()
        {
            var numberExtraLives = 6;
            if (_archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.FullShuffling)
            {
                numberExtraLives = _archipelago.GetReceivedItemCount(JK_EXTRA_LIFE);
            }

            return numberExtraLives;
        }

        public static bool GetLootDrop_ExtraLoot_Prefix(AbigailGame.CowboyMonster __instance, ref int __result)
        {
            try
            {
                if (__instance.type == 6 && __instance.special)
                {
                    __result = -1;
                    return false; // don't run original logic
                }

                var easyMode = _archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.VictoriesEasy;
                var receivedDropRate = _archipelago.HasReceivedItem(JOTPK_DROP_RATE, out _);
                var increasedDropRate = easyMode || receivedDropRate;
                
                var moneyDropChance = increasedDropRate ? 0.1 : 0.05;
                if (Game1.random.NextDouble() < moneyDropChance)
                {
                    var type0Mob5CoinChance = increasedDropRate ? 0.02 : 0.01;
                    var otherMob5CoinChance = increasedDropRate ? 0.2 : 0.1;
                    var mobIsType0 = __instance.type != 0;
                    __result = ((mobIsType0 && Game1.random.NextDouble() < type0Mob5CoinChance) || (Game1.random.NextDouble() < otherMob5CoinChance)) ? 1 : 0;
                    return false; // don't run original logic
                }

                // 90% Chance of dropping nothing (Original: 95%)
                var dropNothingChance = increasedDropRate ? 0.9 : 0.95;
                if (Game1.random.NextDouble() <= dropNothingChance)
                {
                    __result = -1;
                    return false; // don't run original logic
                }

                // 15% Chance of dropping a 6 or 7 item
                if (Game1.random.NextDouble() < 0.15)
                {
                    __result = Game1.random.Next(6, 8);
                    return false; // don't run original logic
                }

                // 7% Chance of dropping a 10 item
                if (Game1.random.NextDouble() < 0.07)
                {
                    __result = 10;
                    return false; // don't run original logic
                }

                // Item from 2 to 9
                var lootDrop = Game1.random.Next(2, 10);
                if (lootDrop == 5 && Game1.random.NextDouble() < 0.4)
                {
                    // 40% of 5s get rolled again
                    lootDrop = Game1.random.Next(2, 10);
                }
                __result = lootDrop;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetLootDrop_ExtraLoot_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool UsePowerup_PrairieKingBossBeaten_Prefix(AbigailGame __instance, int which)
        {
            try
            {
                if (__instance.activePowerups.ContainsKey(which) || which > -1)
                {
                    return true; // run original logic
                }

                if (which == -3)
                {
                    _locationChecker.AddCheckedLocation("Journey of the Prairie King Victory");
                    return true; // run original logic
                }

                var whichCowboyWasBeaten = which == -1 ? 1 : 2;
                _locationChecker.AddCheckedLocation($"JotPK: Cowboy {whichCowboyWasBeaten}");
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(UsePowerup_PrairieKingBossBeaten_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void StartShoppingLevel_ShopBasedOnSentChecks_PostFix(AbigailGame __instance)
        {
            try
            {
                _bootsItemOffered = GetBootsItemToOffer();
                _gunItemOffered = GetGunItemToOffer();
                _ammoItemOffered = GetAmmoItemToOffer();

                __instance.storeItems.Clear();
                __instance.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), _bootsItemOffered);
                __instance.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), _gunItemOffered);
                __instance.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), _ammoItemOffered);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(StartShoppingLevel_ShopBasedOnSentChecks_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void Tick_Shopping_PostFix(AbigailGame __instance, GameTime time, ref bool __result)
        {
            try
            {
                if (__instance.runSpeedLevel != _bootsLevel)
                {
                    switch (_bootsItemOffered)
                    {
                        case 3:
                            _locationChecker.AddCheckedLocation(JOTPK_BOOTS_1);
                            break;
                        case 4:
                            _locationChecker.AddCheckedLocation(JOTPK_BOOTS_2);
                            break;
                    }

                    AssignStartingEquipment(__instance);
                    return;
                }

                var instanceGun = __instance.fireSpeedLevel + (__instance.spreadPistol ? 1 : 0);
                if (instanceGun != _gunLevel)
                {
                    switch (_gunItemOffered)
                    {
                        case 0:
                            _locationChecker.AddCheckedLocation(JOTPK_GUN_1);
                            break;
                        case 1:
                            _locationChecker.AddCheckedLocation(JOTPK_GUN_2);
                            break;
                        case 2:
                            _locationChecker.AddCheckedLocation(JOTPK_GUN_3);
                            break;
                        case 9:
                            _locationChecker.AddCheckedLocation(JOTPK_SUPER_GUN);
                            break;
                    }

                    AssignStartingEquipment(__instance);
                    return;
                }

                if (__instance.ammoLevel != _ammoLevel)
                {
                    switch (_ammoItemOffered)
                    {
                        case 6:
                            _locationChecker.AddCheckedLocation(JOTPK_AMMO_1);
                            break;
                        case 7:
                            _locationChecker.AddCheckedLocation(JOTPK_AMMO_2);
                            break;
                        case 8:
                            _locationChecker.AddCheckedLocation(JOTPK_AMMO_3);
                            break;
                    }

                    AssignStartingEquipment(__instance);
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Tick_Shopping_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void AbigailGameCtor_Equipments_Postfix(AbigailGame __instance, bool playingWithAbby)
        {
            try
            {
                AssignStartingEquipment(__instance);

                var easyMode = _archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.VictoriesEasy;
                var extraLives = easyMode ? 2 : _archipelago.GetReceivedItemCount(JOTPK_EXTRA_LIFE);
                extraLives = Math.Max(0, Math.Min(2, extraLives));
                __instance.lives += extraLives;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AbigailGameCtor_Equipments_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static void AssignStartingEquipment(AbigailGame __instance)
        {
            var easyMode = _archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.VictoriesEasy;

            _bootsLevel = easyMode ? 1 : _archipelago.GetReceivedItemCount(JOTPK_PROGRESSIVE_BOOTS);
            _bootsLevel = Math.Max(0, Math.Min(2, _bootsLevel));

            _gunLevel = easyMode ? 1 : _archipelago.GetReceivedItemCount(JOTPK_PROGRESSIVE_GUN);
            _gunLevel = Math.Max(0, Math.Min(4, _gunLevel));

            _ammoLevel = easyMode ? 1 : _archipelago.GetReceivedItemCount(JOTPK_PROGRESSIVE_AMMO);
            _ammoLevel = Math.Max(0, Math.Min(3, _ammoLevel));

            __instance.runSpeedLevel = _bootsLevel;
            __instance.fireSpeedLevel = _gunLevel == 4 ? 3 : _gunLevel;
            __instance.spreadPistol = _gunLevel == 4;
            __instance.ammoLevel = _ammoLevel;
            __instance.bulletDamage = 1 + _ammoLevel;

            _bootsItemOffered = -1;
            _gunItemOffered = -1;
            _ammoItemOffered = -1;
        }

        private static int GetBootsItemToOffer()
        {
            var missingBoots1 = _locationChecker.IsLocationMissing(JOTPK_BOOTS_1);
            var missingBoots2 = _locationChecker.IsLocationMissing(JOTPK_BOOTS_2);
            var bootsItemOffered = missingBoots1 ? 3 : (missingBoots2 ? 4 : 5);
            return bootsItemOffered;
        }

        private static int GetGunItemToOffer()
        {
            var missingGun1 = _locationChecker.IsLocationMissing(JOTPK_GUN_1);
            var missingGun2 = _locationChecker.IsLocationMissing(JOTPK_GUN_2);
            var missingGun3 = _locationChecker.IsLocationMissing(JOTPK_GUN_3);
            var missingSuperGun = _locationChecker.IsLocationMissing(JOTPK_SUPER_GUN);
            var gunItemOffered = missingGun1 ? 0 : (missingGun2 ? 1 : (missingGun3 ? 2 : (missingSuperGun ? 9 : 10)));
            return gunItemOffered;
        }

        private static int GetAmmoItemToOffer()
        {
            var missingAmmo1 = _locationChecker.IsLocationMissing(JOTPK_AMMO_1);
            var missingAmmo2 = _locationChecker.IsLocationMissing(JOTPK_AMMO_2);
            var missingAmmo3 = _locationChecker.IsLocationMissing(JOTPK_AMMO_3);
            var ammoItemOffered = missingAmmo1 ? 6 : (missingAmmo2 ? 7 : (missingAmmo3 ? 8 : 10));
            return ammoItemOffered;
        }
    }

    public enum Powerup
    {
        SingleCoin = 0,
        FiveCoins = 1,
        ExtraLife = 8,
    }
}
