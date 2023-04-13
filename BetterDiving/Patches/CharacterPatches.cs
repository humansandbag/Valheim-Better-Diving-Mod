using HarmonyLib;
using UnityEngine;

namespace BetterDiving.Patches
{
    /*

    ===============================================================================


    Patches the Character Awake method to setup values


    ===============================================================================

    */
    [HarmonyPatch(typeof(Character), "Awake")]
    public class Character_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance)
        {
            BetterDiving.DebugLog("---------------------Section Character Awake Start------------------------");
            BetterDiving.DebugLog("__instance.IsPlayer()" + " -> " + __instance.IsPlayer());

            if (__instance.IsPlayer() && Player.m_localPlayer && object.ReferenceEquals(__instance, Player.m_localPlayer))
            {
                __instance.m_swimDepth = 1.6f;
                BetterDiving.DebugLog("__instance.IsPlayer()" + " -> " + __instance.IsPlayer());
            }

            BetterDiving.DebugLog("----------------------Section Character Awake End-------------------------");
        }
    }

    /*

    ===============================================================================


    Patches the Character OnDestroy method to hide the oxygen bar and reset vars


    ===============================================================================

    */
    [HarmonyPatch(typeof(Character), "OnDestroy")]
    public class Character_OnDestroy
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance)
        {

            // check if the character instance is the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!Player.m_localPlayer || !__instance.IsPlayer())
                return;

            BetterDiving.has_created_breathe_bar = false;
            BetterDiving.dive_timer_is_running = false;
            BetterDiving.DebugLog("Better Diving Mod: OnDestroy Character...");
        }
    }

    /*

    ===============================================================================


    Patches the Character UpdateMotion method to determine if the instance has surfaced or not


    ===============================================================================

    */
    [HarmonyPatch(typeof(Character), "UpdateMotion")]
    public class Character_UpdateMovement
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance, ref float ___m_lastGroundTouch, ref float ___m_swimTimer)
        {

            // check if the character instance is the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!__instance.IsPlayer() || !BetterDiving.IsEnvAllowed() || !Player.m_localPlayer)
                return;

            // Bug fix for swimming on land glitch - originally __instance.m_swimDepth > 2.5f
            if (__instance.m_swimDepth > 2.5f && (Mathf.Max(0f, __instance.GetLiquidLevel() - __instance.transform.position.y) > 2.5f))
            {
                __instance.GetComponent<BetterDivingExtension>().is_diving = true;
                __instance.GetComponent<BetterDivingExtension>().isUnderwater = true;
                ___m_lastGroundTouch = 0.3f;
                ___m_swimTimer = 0f;
            }
            else
            {
                __instance.GetComponent<BetterDivingExtension>().is_diving = false;

                // Fix for oxygen bar bug. Remove the bar if full.
                if (__instance.GetComponent<BetterDivingExtension>().remainingDiveTime >= 1f)
                {
                    BetterDiving.breathDelayTimer += Time.deltaTime;

                    // Delay removal of the breathe bar for a number of seconds
                    if (BetterDiving.breathDelayTimer >= BetterDiving.breathBarRemoveDelay)
                    {
                        BetterDiving.loc_breath_bar_bg.SetActive(false);
                        BetterDiving.loc_depleted_breath.SetActive(false);
                        BetterDiving.loc_breath_bar.SetActive(false);
                        BetterDiving.loc_breathe_overlay.SetActive(false);
                        BetterDiving.breathDelayTimer = 0;
                    }
                }
                else
                {
                    // Amount of delay before removing the breath bar
                    BetterDiving.breathDelayTimer = 0;
                }

                if (__instance.GetComponent<BetterDivingExtension>().isUnderwater == true && __instance.GetStandingOnShip() == null)
                {
                    if (!__instance.IsDead() && BetterDiving.showYouCanBreatheMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, "You can breath now.");
                    }

                    __instance.GetComponent<BetterDivingExtension>().toggleDive = false;
                    __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "PlayerSurfaced";

                    if (!__instance.IsDead() && BetterDiving.showSurfacingMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, BetterDiving.surfacingMsg.Value);
                    }

                    __instance.GetComponent<BetterDivingExtension>().isUnderwater = false;
                }
            }
        }
    }

    /*

    ===============================================================================


    Patches the Character FixedUpdate method to update swiming and update character vals for diving


    ===============================================================================

    */
    [HarmonyPatch(typeof(Character), "FixedUpdate")]
    public class Character_FixedUpdate
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance, ref Vector3 ___m_moveDir, ref Vector3 ___m_lookDir, ref float ___m_lastGroundTouch, ref bool ___m_walking, ref bool ___m_wallRunning, ref bool ___m_sliding, ref bool ___m_running, ref float ___m_swimTimer)
        {

            // If the character is not the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!__instance.IsPlayer() || !BetterDiving.IsEnvAllowed() || !Player.m_localPlayer)
                return;

            if (!__instance.InWater())
            {
                __instance.m_swimDepth = 1.6f;
            }

            bool crouchButtonDown = false;

            // Toggle diving when "Crouch" button is pressed
            if ((ZInput.GetButtonDown("Crouch") || ZInput.GetButtonDown("JoyCrouch")) && !crouchButtonDown && __instance.InWater() && !__instance.IsOnGround() && __instance.IsSwiming())
            {
                crouchButtonDown = true;

                if (__instance.GetComponent<BetterDivingExtension>().toggleDive == false)
                {
                    __instance.GetComponent<BetterDivingExtension>().toggleDive = true;
                    __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "None";

                    if (BetterDiving.showDivingMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, BetterDiving.divingMsg.Value);
                    }
                }
                //Cancel diving if button is pressed again and still near the surface
                else if (__instance.GetComponent<BetterDivingExtension>().toggleDive == true && __instance.m_swimDepth <= 2.5f)
                {
                    __instance.GetComponent<BetterDivingExtension>().toggleDive = false;
                    __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "PlayerCancelled";

                    if (BetterDiving.showDivingMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, BetterDiving.divingCancelledMsg.Value);
                    }
                }
            }
            else if ((ZInput.GetButtonUp("Crouch") || ZInput.GetButtonUp("JoyCrouch")))
            {
                crouchButtonDown = false;
            }

            //Cancel diving if player is on land
            if (__instance.IsOnGround() || !__instance.IsSwiming() || !__instance.InWater())
            {
                __instance.GetComponent<BetterDivingExtension>().toggleDive = false;
                __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "PlayerOnLand";
            }

            // If player can dive and has pressed the dive toggle key
            if (__instance.GetComponent<BetterDivingExtension>().toggleDive == true && __instance.InWater() && !__instance.IsOnGround() && __instance.IsSwiming())
            {

                //Diving Skill
                if (__instance.IsPlayer() && __instance.m_swimDepth > 2.5f)
                {
                    BetterDiving.m_diveSkillImproveTimer += Time.deltaTime;

                    if (BetterDiving.m_diveSkillImproveTimer > 1f)
                    {
                        BetterDiving.m_diveSkillImproveTimer = 0f;
                        __instance.RaiseSkill(BetterDiving.DivingSkillType, 0.25f);
                    }
                }

                BetterDiving.char_swim_depth = __instance.m_swimDepth;

                float multiplier = 0f;
                if (___m_lookDir.y < -0.25)
                {
                    BetterDiving.loc_m_diveAaxis = 1;
                }
                if (___m_lookDir.y > 0.15)
                {
                    BetterDiving.loc_m_diveAaxis = 0;
                }

                multiplier = (___m_lookDir.y * ___m_lookDir.y) * 0.25f;

                if (multiplier > 0.025f) multiplier = 0.025f;

                if ((ZInput.GetButton("Forward") || ZInput.GetButton("JoyLStickUp")))
                {

                    if (___m_lookDir.y > -0.25f && ___m_lookDir.y < 0.15f)
                    {
                        multiplier = 0f;
                    }

                    if (BetterDiving.loc_m_diveAaxis == 1)
                    {
                        __instance.m_swimDepth += multiplier;
                    }
                    if (BetterDiving.loc_m_diveAaxis == 0)
                    {
                        if (__instance.m_swimDepth > 1.6f)
                        {
                            __instance.m_swimDepth -= multiplier;
                        }

                        if (__instance.m_swimDepth < 1.6f)
                        {
                            __instance.m_swimDepth = 1.6f;
                        }
                    }

                    if (__instance.m_swimDepth > 2.5f) __instance.SetMoveDir(___m_lookDir);
                }
            }
            else
            {
                if ((__instance.IsOnGround() || __instance.GetComponent<BetterDivingExtension>().is_diving == false) && !__instance.GetComponent<BetterDivingExtension>().isTakeRestInWater)
                {
                    __instance.m_swimDepth = 1.6f;
                }
            }
        }
    }

    /*

    ===============================================================================


    Patches the Character UpdateSwimming method for swim speed and stamina drain


    ===============================================================================

    */
    [HarmonyPatch(typeof(Character), "UpdateSwiming")]
    class Character_UpdateSwiming_Patch
    {
        static void Prefix(Character __instance)
        {

            // check if the character instance is the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!__instance.IsPlayer() || !BetterDiving.allowFastSwimming.Value || !Player.m_localPlayer)
                return;

            //Swim speed
            float swimSkill = __instance.GetSkillFactor(Skills.SkillType.Swim);
            float swimSkillFactor = swimSkill * 100f;
            float sprintSwimEnhancement = (BetterDiving.fastSwimSpeedMultiplier * swimSkillFactor) + 1;
            BetterDiving.fastSwimSpeed = BetterDiving.baseSwimSpeed + sprintSwimEnhancement;

            //Stamina
            float swimStaminaFactor = swimSkill / 100f;
            float maxStaminaDrainReduction = 0.5f;
            float staminaDrainReduction = maxStaminaDrainReduction * swimStaminaFactor;
            float skillDrainMultiplier = 0.1f;
            float skillDrainPenalty = skillDrainMultiplier * swimSkill;
            float totalDrainReduction = staminaDrainReduction + skillDrainPenalty;
            float baseDrainFactor = 0.5f;
            float staminaDrainFactor = baseDrainFactor - totalDrainReduction;
            float staminaDrainRate = (BetterDiving.swimStaminaDrainRate * staminaDrainFactor * 2f) + 5f;
            BetterDiving.fastSwimStamDrain = staminaDrainRate;

            if ((ZInput.GetButton("Run") || ZInput.GetButton("JoyRun")))
            {
                if (!__instance.GetComponent<BetterDivingExtension>().isUnderwater)
                {
                    float staminaCost = Time.deltaTime * staminaDrainRate;
                    __instance.UseStamina(staminaCost);
                }
                __instance.m_swimSpeed = BetterDiving.fastSwimSpeed;
            }
            else
            {
                __instance.m_swimSpeed = BetterDiving.baseSwimSpeed;
            }
        }
    }
}