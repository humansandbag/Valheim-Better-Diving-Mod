using HarmonyLib;
using System;
using UnityEngine;

namespace BetterDiving.Patches
{
    /*

    ===============================================================================


    Patches the WaterVolume Update method to update watervolume values for diving or walking


    ===============================================================================

    */
    [HarmonyPatch(typeof(WaterVolume), "Update")]
    public class WaterVolume_Update
    {
        public static int RandomDigits(int length)
        {
            var random = new System.Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return int.Parse(s);
        }

        [HarmonyPrefix]
        public static void Prefix(WaterVolume __instance, ref float ___m_waterTime, ref float[] ___m_normalizedDepth, ref Collider ___m_collider)
        {
            if (!Player.m_localPlayer) return;

            if (GameCamera.instance)
            {
                BetterDiving.water_level_camera = __instance.GetWaterSurface(new Vector3(GameCamera.instance.transform.position.x, GameCamera.instance.transform.position.y, GameCamera.instance.transform.position.z));
            }
            if (Player.m_localPlayer)
            {
                BetterDiving.water_level_player = __instance.GetWaterSurface(new Vector3(Player.m_localPlayer.transform.position.x, Player.m_localPlayer.transform.position.y, Player.m_localPlayer.transform.position.z));
            }
            if (BetterDiving.loc_cam_pos_y < BetterDiving.water_level_camera && Player.m_localPlayer.IsSwiming())
            {
                if (__instance.m_waterSurface.GetComponent<MeshRenderer>().transform.rotation.eulerAngles.y != 180f && BetterDiving.IsEnvAllowed())
                {
                    __instance.m_waterSurface.transform.Rotate(180f, 0f, 0f);
                    __instance.m_waterSurface.material = BetterDiving.water_mat;
                    __instance.m_waterSurface.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                    if (__instance.m_forceDepth >= 0f)
                    {
                        __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), new float[]
                        {
                            __instance.m_forceDepth,
                            __instance.m_forceDepth,
                            __instance.m_forceDepth,
                            __instance.m_forceDepth
                        });
                    }
                    else
                    {
                        __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), ___m_normalizedDepth);
                    }
                    __instance.m_waterSurface.material.SetFloat(Shader.PropertyToID("_UseGlobalWind"), __instance.m_useGlobalWind ? 1f : 0f);
                }

                __instance.m_waterSurface.transform.position = new Vector3(__instance.m_waterSurface.transform.position.x, BetterDiving.water_level_camera, __instance.m_waterSurface.transform.position.z);
            }
            else
            {
                if (__instance.m_waterSurface.GetComponent<MeshRenderer>().transform.rotation.eulerAngles.y == 180f && BetterDiving.IsEnvAllowed())
                {

                    __instance.m_waterSurface.transform.Rotate(-180f, 0f, 0f);
                    __instance.m_waterSurface.material = BetterDiving.mai_water_mat;

                    if (__instance.m_forceDepth >= 0f)
                    {
                        __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), new float[]
                        {
                        __instance.m_forceDepth,
                        __instance.m_forceDepth,
                        __instance.m_forceDepth,
                        __instance.m_forceDepth
                        });
                    }
                    else
                    {
                        __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), ___m_normalizedDepth);
                    }
                    __instance.m_waterSurface.transform.position = new Vector3(__instance.m_waterSurface.transform.position.x, 30f, __instance.m_waterSurface.transform.position.z);
                    __instance.m_waterSurface.material.SetFloat(Shader.PropertyToID("_UseGlobalWind"), __instance.m_useGlobalWind ? 1f : 0f);
                }
            }
        }
    }

    /*

    ===============================================================================


    Patches the WaterVolume Awake method to detect water volumes


    ===============================================================================

    */
    [HarmonyPatch(typeof(WaterVolume), "Awake")]
    public class WaterVolume_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(WaterVolume __instance)
        {
            if (__instance == null) return;

            BetterDiving.DebugLog("---------------------Section WaterVolume Awake Prefix Start------------------------");
            BetterDiving.DebugLog("water_volume_awake" + " -> " + "true");
            BetterDiving.DebugLog("----------------------Section WaterVolume Awake Prefix End-------------------------");
        }

        [HarmonyPostfix]
        public static void Postfix(WaterVolume __instance)
        {
            if (__instance == null) return;

            BetterDiving.DebugLog("---------------------Section WaterVolume Awake Postfix Start------------------------");
            BetterDiving.DebugLog("mai_water_mat" + " -> " + (BetterDiving.mai_water_mat == null ? "null" : "not null"));

            if (BetterDiving.mai_water_mat == null)
            {
                BetterDiving.mai_water_mat = __instance.m_waterSurface.material;
                BetterDiving.DebugLog("Info" + " -> " + "set water mat");
            }

            BetterDiving.DebugLog("----------------------Section WaterVolume Awake Postfix End-------------------------");
        }
    }
}