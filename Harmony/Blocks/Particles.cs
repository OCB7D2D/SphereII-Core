using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Harmony.Blocks
{
    /**
     * * Adding to new blocks:
     * *
     * <property name="ParticleName" value="#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X" />
     */
    public class Particles
    {
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("Init")]
        public class Init
        {
            public static void Postfix(ref Block __instance)
            {
                if (!__instance.Properties.Values.ContainsKey("ParticleName"))
                    return;

                var strParticleName = __instance.Properties.Values["ParticleName"];
                if (!ParticleEffect.IsAvailable(strParticleName))
                    ParticleEffect.RegisterBundleParticleEffect(strParticleName);
            }
        }

        [HarmonyPatch(typeof(GameManager))]
        [HarmonyPatch("spawnParticleEffect")]
        public class spawnParticleEffect
        {
            static readonly int odds = 4;
            static int count = odds;
            public static void Postfix(ref Transform __result)
            {
                if (__result?.GetComponentInChildren<Light>() is Light light)
                {
                    if (count == odds)
                    {
                        count = 0;
                    }
                    else
                    {
                        light.enabled = false;
                        light.gameObject.transform.parent = null;
                        Object.Destroy(light.gameObject);
                        count += 1;
                    }
                }
            }
        }

    }
}