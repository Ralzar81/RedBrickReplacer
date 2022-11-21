// Project:         Red Brick Replacer mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;

public class RedBrickReplacer : MonoBehaviour
{
    static Mod mod;
    static int teleArchive = 094;
    static int teleRecord = 0;
    static int wallArchive = 094;
    static int wallRecord = 0;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        Debug.Log("[RedBrickReplacer] Initializing mod");
        mod = initParams.Mod;
        var go = new GameObject(mod.Title);
        go.AddComponent<RedBrickReplacer>();

        PlayerEnterExit.OnTransitionDungeonInterior += FindAndFixRedBrickWalls;
    }

    private void Awake()
    {
        Debug.Log("[RedBrickReplacer] Awake");
        ModSettings settings = mod.GetSettings();
        teleArchive = 094;
        teleRecord = 0;
        wallArchive = 094;
        wallRecord = 0;

        switch (settings.GetValue<int>("ReplaceTeleporterTexture", "TeleporterTexture"))
        {
            case 0:
                teleArchive = 094;
                teleRecord = 0;
                break;
            case 1:
                teleArchive = 355;
                teleRecord = 0;
                break;
            case 2:
                teleArchive = 356;
                teleRecord = 0;
                break;
        }
        switch (settings.GetValue<int>("ReplaceRedBrickTexture", "WallTexture"))
        {
            case 0:
                wallArchive = 094;
                wallRecord = 0;
                break;
            case 1:
                wallArchive = 351;
                wallRecord = 1;
                break;
            case 2:
                wallArchive = 354;
                wallRecord = 0;
                break;
        }

        mod.IsReady = true;
        Debug.Log("[RedBrickReplacer] mod.IsReady = true");
    }

    private static void FindAndFixRedBrickWalls(PlayerEnterExit.TransitionEventArgs args)
    {
        Debug.Log("[RedBrickReplacer] Running FindAndFixRedBrickWalls");
        MeshCollider[] foundMeshColliders = (MeshCollider[])FindObjectsOfType(typeof(MeshCollider));
        GameObject wallObj;
        Debug.Log("[RedBrickReplacer] meshColliders found = " + foundMeshColliders.Length.ToString());
        foreach (MeshCollider mesh in foundMeshColliders)
        {
            wallObj = mesh.transform.gameObject;
            bool aFlag = wallObj.GetComponent<DaggerfallAction>() != null;
            bool animMat = wallObj.GetComponent<AnimatedMaterial>() != null;
            MeshRenderer wallMR = wallObj.GetComponent<MeshRenderer>();

            if (aFlag && wallObj.GetComponent<DaggerfallAction>().ActionFlag == DFBlock.RdbActionFlags.Teleport && teleArchive != 094)
            {
                Debug.Log("[RedBrickReplacer] Teleporter");
                string materialName = "TEXTURE.094 [Index=0] (Instance)";

                Material[] materials = wallMR.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name == materialName)
                    {
                        Material newMaterial = DaggerfallUnity.Instance.MaterialReader.GetMaterial(teleArchive, teleRecord);
                        materials[i] = newMaterial;
                        wallMR.materials = materials;
                        if (teleArchive == 356)
                        {
                            CachedMaterial cachedMaterial;
                            DaggerfallUnity.Instance.MaterialReader.GetCachedMaterial(teleArchive, teleRecord, 0, out cachedMaterial);
                            CachedMaterial[] cachedMaterialsOut = new CachedMaterial[] { cachedMaterial };
                            GameObjectHelper.AssignAnimatedMaterialComponent(cachedMaterialsOut, wallObj);
                            wallObj.GetComponent<AnimatedMaterial>().FramesPerSecond = 2;
                        }
                        break;
                    }
                }
            }
            else if (wallArchive != 094 && !animMat)
            {
                Debug.Log("[RedBrickReplacer] Not Teleporter");
                string materialName = "TEXTURE.094 [Index=0] (Instance)";

                Material[] notTeleMaterials = wallMR.materials;
                for (int i = 0; i < notTeleMaterials.Length; i++)
                {

                    if (notTeleMaterials[i].name == materialName)
                    {
                        Material newMaterial = DaggerfallUnity.Instance.MaterialReader.GetMaterial(wallArchive, wallRecord);
                        notTeleMaterials[i] = newMaterial;
                        wallMR.materials = notTeleMaterials;
                        break;
                    }
                }
            }
        }
    }
}
