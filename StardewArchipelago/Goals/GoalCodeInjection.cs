﻿using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Locations;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Goals
{
    internal class GoalCodeInjection
    {
        public const string MASTER_ANGLER_LETTER = "CF_Fish";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static void CheckCommunityCenterGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.CommunityCenter)
            {
                return;
            }

            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (!communityCenter.areAllAreasComplete())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckGrandpaEvaluationGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.GrandpaEvaluation)
            {
                return;
            }

            var farm = Game1.getFarm();
            int candlesFromScore = Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore());
            farm.grandpaScore.Value = candlesFromScore;
            for (int index = 0; index < candlesFromScore; ++index)
            {
                DelayedAction.playSoundAfterDelay("fireball", 100 * index);
            }
            farm.addGrandpaCandles();

            if (farm.grandpaScore.Value < 4)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckBottomOfTheMinesGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.BottomOfMines)
            {
                return;
            }

            var lowestMineLevel = Game1.netWorldState.Value.LowestMineLevel;

            if (lowestMineLevel < 120)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckCrypticNoteGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.CrypticNote)
            {
                return;
            }

            if (!Game1.player.mailReceived.Contains("qiCave"))
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckMasterAnglerGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.MasterAngler)
            {
                return;
            }

            CheckMasterAnglerWithoutIslandFish();
            if (!Game1.player.hasOrWillReceiveMail(MASTER_ANGLER_LETTER))
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        private static void CheckMasterAnglerWithoutIslandFish()
        {
            if (!_archipelago.SlotData.ExcludeGingerIsland || Game1.player.hasOrWillReceiveMail(MASTER_ANGLER_LETTER))
            {
                return;
            }

            var uniqueFishCaught = 0;
            var totalFishExist = 0;
            foreach (var (id, information) in Game1.objectInformation)
            {
                var isFish = information.Split('/')[3].Contains("Fish");
                var isTrash = (id >= 167 && id <= 172);
                var isLegendaryFamily = (id >= 898 && id <= 902);
                var isIslandFish = (id >= 836 && id <= 838);
                if (!isFish || isTrash || isLegendaryFamily || isIslandFish)
                {
                    continue;
                }

                ++totalFishExist;
                if (Game1.player.fishCaught.ContainsKey(id))
                {
                    ++uniqueFishCaught;
                }
            }

            if (uniqueFishCaught < totalFishExist)
            {
                return;
            }

            if (!Game1.player.hasOrWillReceiveMail(MASTER_ANGLER_LETTER))
            {
                Game1.addMailForTomorrow(MASTER_ANGLER_LETTER);
            }
        }

        public static void CheckCompleteCollectionGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.CompleteCollection)
            {
                return;
            }

            if (!Game1.player.hasOrWillReceiveMail("museumComplete"))
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckFullHouseGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.FullHouse)
            {
                return;
            }

            if (Game1.player.getChildrenCount() < 2 || !Game1.player.isMarried() || Game1.player.HouseUpgradeLevel < 2)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckWalnutHunterGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.GreatestWalnutHunter)
            {
                return;
            }

            if (Game1.netWorldState.Value.GoldenWalnutsFound.Value < 130)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckProtectorOfTheValleyGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.ProtectorOfTheValley)
            {
                return;
            }

            if (!AdventureGuild.areAllMonsterSlayerQuestsComplete())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckFullShipmentGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.FullShipment)
            {
                return;
            }

            if (!Utility.hasFarmerShippedAllItems())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckGourmetChefGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.GourmetChef)
            {
                return;
            }

            if (!HasCookedAllRecipes())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckCraftMasterGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.CraftMaster)
            {
                return;
            }

            if (!HasCraftedAllRecipes())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckLegendGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.Legend)
            {
                return;
            }

            if (Game1.player.totalMoneyEarned < 10000000)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckMysteryOfTheStardropsGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.MysteryOfTheStardrops)
            {
                return;
            }

            if (!Game1.player.mailReceived.Contains("gotMaxStamina"))
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckAllsanityGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.Allsanity)
            {
                return;
            }

            if (_locationChecker.GetAllMissingLocations().Any())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckPerfectionGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.Perfection)
            {
                return;
            }

            if (Utility.percentGameComplete() < 1.0)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void DoAreaCompleteReward_CommunityCenterGoal_PostFix(CommunityCenter __instance, int whichArea)
        {
            try
            {
                CheckCommunityCenterGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void EnterMine_Level120Goal_PostFix(int whatLevel)
        {
            try
            {
                if (whatLevel != 120)
                {
                    return;
                }

                _archipelago.ReportGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(EnterMine_Level120Goal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public void foundWalnut(int stack = 1)
        public static void FounddWalnut_WalnutHunterGoal_Postfix(Farmer __instance, int stack)
        {
            try
            {
                CheckWalnutHunterGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(FounddWalnut_WalnutHunterGoal_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static bool HasCookedAllRecipes()
        {
            var allRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
            foreach (var (recipeName, recipe) in allRecipes)
            {
                if (!Game1.player.cookingRecipes.ContainsKey(recipeName))
                {
                    return false;
                }

                var recipeId = Convert.ToInt32(recipe.Split('/')[2].Split(' ')[0]);
                if (!Game1.player.recipesCooked.ContainsKey(recipeId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasCraftedAllRecipes()
        {
            var allRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
            foreach (var recipe in allRecipes.Keys)
            {
                if (!Game1.player.craftingRecipes.ContainsKey(recipe) || Game1.player.craftingRecipes[recipe] <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        // private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        public static void ClickCraftingRecipe_CraftMasterGoal_Postfix(CraftingPage __instance, ClickableTextureComponent c, bool playSound)
        {
            try
            {
                CheckCraftMasterGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ClickCraftingRecipe_CraftMasterGoal_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public uint totalMoneyEarned
        public static void TotalMoneyEarned_CheckLegendGoalCompletion_Postfix(Farmer __instance, uint value)
        {
            try
            {
                CheckLegendGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(TotalMoneyEarned_CheckLegendGoalCompletion_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public static bool foundAllStardrops(Farmer who = null)
        public static void FoundAllStardrops_CheckStardropsGoalCompletion_Postfix(Farmer who, ref bool __result)
        {
            try
            {
                if (who.MaxStamina < 508)
                {
                    return;
                }

                who.ClearBuffs();
                if (who.MaxStamina < 508)
                {
                    return;
                }

                _archipelago.ReportGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(FoundAllStardrops_CheckStardropsGoalCompletion_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public static float percentGameComplete()
        public static void PercentGameComplete_PerfectionGoal_Postfix(ref float __result)
        {
            try
            {
                if (__result < 1.0)
                {
                    return;
                }

                _archipelago.ReportGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PercentGameComplete_PerfectionGoal_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static string GetGoalString()
        {
            var goal = _archipelago.SlotData.Goal switch
            {
                Goal.GrandpaEvaluation => "Complete Grandpa's Evaluation with a score of at least 12 (4 candles)",
                Goal.BottomOfMines => "Reach Floor 120 in the Pelican Town Mineshaft",
                Goal.CommunityCenter => "Complete the Community Center",
                Goal.CrypticNote => "Find Secret Note #10 and complete the \"Cryptic Note\" Quest",
                Goal.MasterAngler => "Catch every single one of the 55 fish available in the game",
                Goal.CompleteCollection => "Complete the Museum Collection by donating all 95 items",
                Goal.FullHouse => "Get married and have two children",
                Goal.GreatestWalnutHunter => "Find all 130 Golden Walnuts",
                Goal.ProtectorOfTheValley => "Complete all the monster slaying goals",
                Goal.FullShipment => "Ship every item",
                Goal.GourmetChef => "Cook every recipe",
                Goal.CraftMaster => "Craft every item",
                Goal.Legend => "Earn 10 000 000g",
                Goal.MysteryOfTheStardrops => "Obtain all stardrops",
                Goal.Allsanity => "Complete every Archipelago check",
                Goal.Perfection => "Achieve Perfection",
                _ => throw new NotImplementedException(),
            };
            return goal;
        }

        public static string GetGoalStringGrandpa()
        {
            var goal = _archipelago.SlotData.Goal switch
            {
                Goal.GrandpaEvaluation => "Make the most of this farm, and make me proud",
                Goal.BottomOfMines => "Finish exploring the mineshaft in this town for me",
                Goal.CommunityCenter => "Restore the old Community Center for the sake of all the villagers",
                Goal.CrypticNote => "Meet an old friend of mine on floor 100 of the Skull Cavern",
                Goal.MasterAngler => "Catch and document every specie of fish in the Ferngill Republic",
                Goal.CompleteCollection => "Restore our beautiful museum with a full collection of various artifacts and minerals",
                Goal.FullHouse => "I wish for my bloodline to thrive. Please find a partner and live happily ever after",
                Goal.GreatestWalnutHunter => "Prove your worth to an old friend of mine, and become the greatest walnut hunter",
                Goal.ProtectorOfTheValley => "Make sure the valley is safe for generations to come, by slaying all the monsters",
                Goal.FullShipment => "Contribute to the local economy and market, by shipping as many things as you can",
                Goal.GourmetChef => "Become a world-class chef, learn and cook all the recipes you can find",
                Goal.CraftMaster => "Get used to making things with your hands, and craft as many items as you can",
                Goal.Legend => "Nothing beats cold hard cash. Become rich enough, and buy your happiness",
                Goal.MysteryOfTheStardrops => "A healthy body is a healthy mind. Get in shape by increasing your energy to the maximum.",
                Goal.Allsanity => "You cannot leave anyone stranded in a Burger King. Leave no loose ends",
                Goal.Perfection => "For a fulfilling life, you need to do a lot of everything. Leave no loose ends",
                _ => throw new NotImplementedException(),
            };
            return goal;
        }
    }
}
