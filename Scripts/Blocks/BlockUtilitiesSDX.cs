using System;
using System.Linq;
using UnityEngine;

public static class BlockUtilitiesSDX
{
    public static void AddRadiusEffect(string strItemClass, ref Block myBlock)
    {
        var itemClass = ItemClass.GetItemClass(strItemClass);
        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            var strBuff = itemClass.Properties.Values["ActivatedBuff"];
            var array5 = strBuff.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            var list2 = myBlock.RadiusEffects.OfType<BlockRadiusEffect>().ToList();
            foreach (var text4 in array5)
            {
                var num12 = text4.IndexOf('(');
                var num13 = text4.IndexOf(')');
                var item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1));
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radius = 1f;
                    item.variable = text4;
                }

                if (!list2.Contains(item))
                {
                    Debug.Log("Adding Buff for " + item.variable);
                    list2.Add(item);
                }
            }

            myBlock.RadiusEffects = list2.ToArray();
        }
    }

    public static void RemoveRadiusEffect(string strItemClass, ref Block myBlock)
    {
        var itemClass = ItemClass.GetItemClass(strItemClass);
        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            var strBuff = itemClass.Properties.Values["ActivatedBuff"];
            var array5 = strBuff.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            var list2 = myBlock.RadiusEffects.OfType<BlockRadiusEffect>().ToList();
            foreach (var text4 in array5)
            {
                var num12 = text4.IndexOf('(');
                var num13 = text4.IndexOf(')');
                var item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1));
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radius = 1f;
                    item.variable = text4;
                }

                if (list2.Contains(item)) list2.Remove(item);
            }

            myBlock.RadiusEffects = list2.ToArray();
        }
    }

    public static void addParticles(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        var blockValue = GameManager.Instance.World.GetBlock(position);
        GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position,
            new ParticleEffect(strParticleName, position.ToVector3() + Vector3.up, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white));
    }

    public static void addParticlesCentered(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        var centerPosition = EntityUtilities.CenterPosition(position);

            var blockValue = GameManager.Instance.World.GetBlock(position);
            //GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position,
            //    new ParticleEffect(strParticleName, centerPosition, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white));

        var particle = new ParticleEffect(strParticleName, centerPosition, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white);
        GameManager.Instance.SpawnParticleEffectServer(particle, -1);
    }

    public static void addParticlesCenteredNetwork(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        var centerPosition = EntityUtilities.CenterPosition(position);

        var blockValue = GameManager.Instance.World.GetBlock(position);
        var particle = new ParticleEffect(strParticleName, centerPosition, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white);
        if (!GameManager.IsDedicatedServer)
        {
            GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position, particle);        
        }
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particle, -1), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particle, -1), false, -1, -1, -1, -1);

    }

    static readonly ulong Seed1 = OCB.StaticRandom.RandomSeed();
    static readonly ulong Seed2 = OCB.StaticRandom.RandomSeed();

    // Shifts are in absolute values (1 meaning one block)
    static readonly OCB.CrookedVector3 RandomShift = new OCB.CrookedVector3(
        new OCB.CrookedAxis(-0.25f, 0.25f, false),
        new OCB.CrookedAxis(-0.25f, 0.25f, false),
        new OCB.CrookedAxis(-0.25f, 0.25f, false));

    // Rotations are in euler angles (degrees)
    static readonly OCB.CrookedVector3 RandomRotation = new OCB.CrookedVector3(
        new OCB.CrookedAxis(-35f, 35f, false),
        new OCB.CrookedAxis(-35f, 35f, false),
        new OCB.CrookedAxis(-35f, 35f, false));

    public static void addParticlesRandomizedNetwork(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        Vector3 centerPosition = EntityUtilities.CenterPosition(position);

        var blockValue = GameManager.Instance.World.GetBlock(position);
        Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);

        ulong seed1 = Seed1;
        OCB.StaticRandom.HashSeed(ref seed1, position.x);
        OCB.StaticRandom.HashSeed(ref seed1, position.y);
        OCB.StaticRandom.HashSeed(ref seed1, position.z);
        centerPosition += RandomShift.GetVector(seed1);

        ulong seed2 = Seed2;
        OCB.StaticRandom.HashSeed(ref seed2, position.x);
        OCB.StaticRandom.HashSeed(ref seed2, position.y);
        OCB.StaticRandom.HashSeed(ref seed2, position.z);
        rotation *= RandomRotation.GetRotation(seed2);

        // var shif = RandomShift.GetVector(seed1);
        // var rot = RandomRotation.GetVector(seed2);
        // Log.Out("Got shift {0}, rot {1}", shift, rot);

        var particle = new ParticleEffect(strParticleName, centerPosition, rotation, 1f, Color.white);
        if (!GameManager.IsDedicatedServer)
        {
            GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position, particle);
        }
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particle, -1), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particle, -1), false, -1, -1, -1, -1);

    }
    public static void removeParticlesNetPackage(Vector3i position)
    {
        if (!GameManager.IsDedicatedServer)
        {
            removeParticles(position);
        }
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(position, -1), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(position, -1), false, -1, -1, -1, -1);
    }

    public static void removeParticles(Vector3i position)
    {
            GameManager.Instance.World.GetGameManager().RemoveBlockParticleEffect(position);
    }
}