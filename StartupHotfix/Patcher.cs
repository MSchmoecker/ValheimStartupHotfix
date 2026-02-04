using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Splatform;

namespace StartupHotfix {
    public static class Patcher {
        public static IEnumerable<string> TargetDLLs { get; } = Array.Empty<string>();

        public static ManualLogSource logger;
        public static IDistributionPlatform platform;

        public static void Patch(AssemblyDefinition assembly) {
        }

        public static void Finish() {
            logger = BepInEx.Logging.Logger.CreateLogSource("StartupHotfix");
            var harmony = new Harmony("com.maxsch.valheim.StartupHotfix.patcher");
            harmony.PatchAll(typeof(Patcher));
        }

        [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start)), HarmonyPrefix]
        public static void FindPlaform() {
            var steamPlatform = AccessTools.AllTypes().FirstOrDefault(type => type.Name == "SteamPlatform");
            if (steamPlatform != null) {
                logger.LogInfo("Setting Splatform SteamPlatform as distribution platform");
                platform = (IDistributionPlatform)Activator.CreateInstance(steamPlatform);
                PlatformManager.SetDistributionPlatform(platform);
                return;
            }

            var xboxPlatform = AccessTools.AllTypes().FirstOrDefault(type => type.Name == "XboxPlatform");
            if (xboxPlatform != null) {
                logger.LogInfo("Setting Splatform XboxPlatform as distribution platform");
                platform = (IDistributionPlatform)Activator.CreateInstance(xboxPlatform);
                PlatformManager.SetDistributionPlatform(platform);
                return;
            }

            logger.LogWarning("SteamPlatform is not found");
        }

        [HarmonyPatch(typeof(PlatformManager), nameof(PlatformManager.DistributionPlatform), MethodType.Getter), HarmonyPostfix]
        public static void SetDistributionPlatform(ref IDistributionPlatform __result) {
            if (__result == null && platform != null) {
                __result = platform;
            }
        }
    }
}
