﻿using DiIiS_NA.Core.Helpers.Math;
using DiIiS_NA.Core.MPQ;
using DiIiS_NA.D3_GameServer.Core.Types.SNO;
using DiIiS_NA.GameServer.Core.Types.Math;
using DiIiS_NA.GameServer.Core.Types.SNO;
using DiIiS_NA.GameServer.Core.Types.TagMap;
using DiIiS_NA.GameServer.GSSystem.ActorSystem;
using DiIiS_NA.GameServer.GSSystem.ActorSystem.Implementations.Minions;
using DiIiS_NA.GameServer.GSSystem.PowerSystem.Payloads;
using DiIiS_NA.GameServer.GSSystem.TickerSystem;
using DiIiS_NA.GameServer.MessageSystem;
using DiIiS_NA.GameServer.MessageSystem.Message.Definitions.ACD;
using DiIiS_NA.GameServer.MessageSystem.Message.Definitions.Effect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiIiS_NA.GameServer.GSSystem.PlayerSystem;
using DiIiS_NA.GameServer.GSSystem.AISystem.Brains;
using DiIiS_NA.GameServer.GSSystem.ObjectsSystem;

namespace DiIiS_NA.GameServer.GSSystem.PowerSystem.Implementations
{
    //Done
    #region BoneSpikes

    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.MainSkills.BoneSpikes)]
    public class BoneSpikes : ChanneledSkill
    {
        private static readonly float[] rangeDividers = new float[] { 8, 7, 6, 5, 4, 3, 2, 1.5f, 1.25f };
        private Actor _beamEnd;

        private Vector3D _calcBeamEnd(float length)
        {
            return PowerMath.TranslateDirection2D(User.Position, TargetPosition,
                                                  new Vector3D(User.Position.X, User.Position.Y, TargetPosition.Z),
                                                  length);
        }

        public override void OnChannelOpen()
        {
            WaitForSpawn = true;
            WaitSeconds = 0.75f / User.Attributes[GameAttributes.Attacks_Per_Second_Total];
            EffectsPerSecond = 0.75f / User.Attributes[GameAttributes.Attacks_Per_Second_Total];
        }

        public override void OnChannelClose()
        {
            if (_beamEnd != null)
                _beamEnd.Destroy();
        }

        public override void OnChannelUpdated()
        {
            User.TranslateFacing(TargetPosition);

            

            AttackPayload attack = new AttackPayload(this);
            var defaultEff = ActorSno._p6_necro_bonespikes;
            if (Rune_B < 1)
            {
                if (Rune_D == 1)
                    defaultEff = ActorSno._p6_necro_bonespikes_d_ice_actorparticle; 
                else if (Rune_E == 1)
                    defaultEff = ActorSno._p6_necro_bonespikes_e_blood_actorparticle;

                if (User.World.CheckLocationForFlag(TargetPosition, DiIiS_NA.Core.MPQ.FileFormats.Scene.NavCellFlags.AllowProjectile))
                {
                    EffectActor Explosion = SpawnEffect(defaultEff, TargetPosition, 0, WaitSeconds(0.4f));

                    Explosion.PlayEffect(Effect.PlayEffectGroup, RuneSelect(462185, 470458, 471513, 472538, 472598, 472762));
                    var targets = GetEnemiesInRadius(TargetPosition, 5f);

                    if (targets.Actors.Count > 0)
                        GeneratePrimaryResource(24f);

                    if (Rune_A > 0)
                        foreach (var target in targets.Actors)
                            if (FastRandom.Instance.Chance(50))
                                AddBuff(target, new DebuffStunned(WaitSeconds(1f)));
                    if (Rune_D == 1)
                    {
                        WeaponDamage(targets, 1.50f, DamageType.Cold);
                        EffectActor Explosion1 = SpawnEffect(defaultEff, TargetPosition, 0, WaitSeconds(2f));
                        Explosion1.PlayEffect(Effect.PlayEffectGroup, 471410);
                        foreach (var target in targets.Actors)
                            AddBuff(target, new DebuffChilled(0.4f, WaitSeconds(0.5f)));
                    }
                    else if (Rune_C == 1)
                    {
                        var target = GetEnemiesInRadius(TargetPosition, 5f);
                        if (target.Actors.Count > 0)
                            WeaponDamage(target.Actors[0], 1.50f, DamageType.Physical);
                        if (target.Actors.Count > 1)
                        {
                            for (int i = 0; i < 2; i++)
                                if (target.Actors.Count >= i)
                                {
                                    EffectActor ExplosionAdd = SpawnEffect(defaultEff, target.Actors[i].Position, 0, WaitSeconds(0.4f));
                                    ExplosionAdd.PlayEffect(Effect.PlayEffectGroup, RuneSelect(462185, 470458, 471513, 472538, 472598, 472762));
                                    WeaponDamage(target.Actors[i], 1.50f, DamageType.Physical);
                                }
                        }
                        else
                        {
                            //for (int i = 0; i < 2; i++)
                            {
                                SpawnEffect(defaultEff, RandomDirection(TargetPosition, 3f, 6f), 0, WaitSeconds(0.4f)).PlayEffect(Effect.PlayEffectGroup, RuneSelect(462185, 470458, 471513, 472538, 472598, 472762));
                            }
                        }
                    }
                    else if (Rune_E == 1)
                    {
                        WeaponDamage(targets, 1.00f, DamageType.Physical);
                        foreach (var target in targets.Actors)
                            ((Player)User).AddPercentageHP(0.5f);
                    }
                    else
                        WeaponDamage(targets, 1.50f, DamageType.Physical);
                }
            }
            else
            {
                Vector3D range = TargetPosition - User.Position;

                bool regen = false;

                foreach (var divider in rangeDividers)
                {
                    var explosion = SpawnEffect(ActorSno._p6_necro_bonespikes, new Vector3D(User.Position.X + range.X / divider, User.Position.Y + range.Y / divider, TargetPosition.Z), 0, WaitSeconds(0.4f));
                    explosion.PlayEffect(Effect.PlayEffectGroup, 471513);
                    var targets = GetEnemiesInRadius(explosion.Position, 5f);
                    if (targets.Actors.Count > 0)
                        regen = true;
                    WeaponDamage(targets, 1.00f, DamageType.Physical);
                }

                if (regen)
                    GeneratePrimaryResource(30f);
            }
            //yield return WaitSeconds(1f);

        }

        public override IEnumerable<TickTimer> Main()
        {

            yield break;
        }
    }

    #endregion
    //Done
    #region GrimScythe
    #region Content
    /*
        [467149] [Actor] P6_necro_GrimScythe_Base_Swipe_Cleave
        [467152] [Actor] P6_necro_GrimScythe_Base_Swipe_Cleave_02
        [471736] [Actor] P6_necro_GrimScythe_BLOOD_weapon
        [471740] [Actor] P6_necro_GrimScythe_BLOOD_Swipe_Cleave
        [471744] [Actor] P6_necro_GrimScythe_BLOOD_Swipe_Cleave_02
        [471746] [Actor] P6_necro_GrimScythe_BLOOD_Arch_RightLeft
        [471750] [Actor] P6_necro_GrimScythe_BLOOD_Arch
        [471942] [Actor] P6_Necro_GrimScythe_E_SweepFX_A_Cleave
        [471990] [Actor] P6_necro_GrimScythe_Base_Corona
        [472181] [Actor] P6_necro_GrimScythe_Blood_Swipe_Right_Left
        [472192] [Actor] P6_necro_GrimScythe_Blood_Swipe_Left_Right
        [472258] [Actor] P6_necro_GrimScythe_Blood_Corona
        [472305] [Actor] P6_necro_GrimScythe_Decay_Swipe_Right_Left
        [472307] [Actor] P6_necro_GrimScythe_Decay_Swipe_Cleave
        [472309] [Actor] P6_necro_GrimScythe_Decay_Arch_RightLeft
        [472312] [Actor] P6_necro_GrimScythe_Decay_Corona
        [472317] [Actor] P6_necro_GrimScythe_Base_Decay_Left_Right
        [472319] [Actor] P6_necro_GrimScythe_Decay_Swipe_Cleave_02
        [472321] [Actor] P6_necro_GrimScythe_Decay_Arch
        [472323] [Actor] P6_necro_GrimScythe_Decay_weapon
        [472353] [Actor] P6_necro_GrimScythe_C_weapon
        [472467] [Actor] P6_necro_GrimScythe_D_Swipe_Right_Left
        [472469] [Actor] P6_necro_GrimScythe_D_Swipe_Cleave
        [472472] [Actor] P6_necro_GrimScythe_D_weapon
        [472479] [Actor] P6_necro_GrimScythe_Base_D_Left_Right
        [472481] [Actor] P6_necro_GrimScythe_D_Swipe_Cleave_02
        [472483] [Actor] P6_necro_GrimScythe_D_Arch
        [472486] [Actor] P6_necro_GrimScythe_D_Corona
        [472556] [Actor] P6_necro_GrimScythe_D_Arch_RightLeft
        [472621] [Actor] P6_Necro_GrimScythe_F_Left_Right
        [472623] [Actor] P6_Necro_GrimScythe_F_Swipe_Cleave_02
        [472625] [Actor] P6_Necro_GrimScythe_F_weapon
        [472627] [Actor] P6_Necro_GrimScythe_F_Corona
        [472633] [Actor] P6_Necro_GrimScythe_F_Swipe_Right_Left
        [472635] [Actor] P6_Necro_GrimScythe_F_Swipe_Cleave
        [472637] [Actor] P6_Necro_GrimScythe_F_Arch_RightLeft
        [472707] [Actor] P6_necro_GrimScythe_F_Arch
        [472975] [Actor] P6_Necro_GrimScythe_E_Swipe_B
        [472979] [Actor] P6_Necro_GrimScythe_E_Arch
        [472981] [Actor] P6_necro_GrimScythe_E_weapon
        [472988] [Actor] P6_Necro_GrimScythe_E_SweepFX_B_Cleave
        [473004] [Actor] P6_necro_GrimScythe_E_Corona
        [462730] [Actor] P6_necro_GrimScythe_Base_weapon
        [462765] [Actor] P6_necro_GrimScythe_Base_Swipe_Left_Right
        [462989] [Actor] P6_necro_GrimScythe_Base_Arch
        [464076] [Actor] P6_necro_GrimScythe_Base_Swipe_Right_Left
        [465355] [Actor] P6_necro_GrimScythe_Base_Arch_RightLeft
        [471162] [Anim] p6_Necro_Female_HTH_Cast_GrimScythe_LeftRight
        [471163] [Anim] p6_Necro_Female_HTH_Cast_GrimScythe_RightLeft
        [471752] [Anim] p6_Necro_Male_HTH_Cast_GrimScytheDW
        [473131] [Anim] P6_necro_GrimScythe_E_Arch_idle
        [475386] [Anim] P6_necro_GrimScythe_Base_Weapon_Dual_idle
        [462271] [Anim] p6_Necro_Male_HTH_Cast_GrimScythe
        [462316] [Anim] p6_Necro_Female_HTH_Cast_GrimScythe
        [462742] [Anim] P6_necro_GrimScythe_Base_weapon_idle
        [462990] [Anim] P6_necro_GrimScythe_Base_Arch_idle
        [463800] [Anim] p6_Necro_Male_HTH_Cast_GrimScythe_RightLeft
        [463801] [Anim] p6_Necro_Male_HTH_Cast_GrimScythe_LeftRight
        [465357] [Anim] P6_necro_GrimScythe_Base_Arch_RightLeft_idle
        [475385] [AnimSet] P6_necro_GrimScythe_E_weapon_Dual
        [462749] [AnimSet] P6_necro_GrimScythe_Base_weapon_A
        [462991] [AnimSet] P6_necro_GrimScythe_Base_Arch
        [473132] [AnimSet] P6_Necro_GrimScythe_E_Arch
        [465356] [AnimSet] P6_necro_GrimScythe_Base_Arch_RightLeft
        [462199] [EffectGroup] P6_Necro_GrimScythe_SweepFXLeftRight
        [471730] [EffectGroup] P6_Necro_GrimScythe_BASE_SweepFXLeftRight
        [471733] [EffectGroup] P6_Necro_GrimScythe_BASE_SweepFX_RightLeft
        [471734] [EffectGroup] P6_Necro_GrimScythe_BLOOD_SweepFXLeftRight
        [471735] [EffectGroup] P6_Necro_GrimScythe_BLOOD_SweepFX_RightLeft
        [471943] [EffectGroup] P6_Necro_GrimScythe_E_SweepFX_A
        [472303] [EffectGroup] P6_Necro_GrimScythe_Decay_SweepFX_RightLeft
        [472315] [EffectGroup] P6_Necro_GrimScythe_Decay_SweepFXLeftRight
        [472465] [EffectGroup] P6_Necro_GrimScythe_D_SweepFX_RightLeft
        [472477] [EffectGroup] P6_Necro_GrimScythe_D_SweepFXLeftRight
        [472619] [EffectGroup] P6_Necro_GrimScythe_F_SweepFXLeftRight
        [472631] [EffectGroup] P6_Necro_GrimScythe_F_SweepFX_RightLeft
        [472971] [EffectGroup] P6_Necro_GrimScythe_E_SweepFX_B
        [464069] [EffectGroup] P6_Necro_GrimScythe_SweepFX_RightLeft
        [462198] [Power] P6_Necro_GrimScythe
    //*/
    #endregion
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.MainSkills.GrimScythe)]
    public class GrimScythe : Skill
    {
        public override IEnumerable<TickTimer> Main()
        {
            var powerData = (DiIiS_NA.Core.MPQ.FileFormats.Power)MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;

            TargetPosition = PowerMath.TranslateDirection2D(User.Position, TargetPosition, User.Position, 7f);
            DamageType damageType = DamageType.Physical;
            if (Rune_E > 0) damageType = DamageType.Poison;
            else if (Rune_C > 0) damageType = DamageType.Cold;

            AttackPayload attack = new(this)
            {
                Targets = GetEnemiesInRadius(TargetPosition, 7f)
            };
            attack.AddWeaponDamage(1.50f, damageType);
            attack.OnHit = hit =>
            {
                GeneratePrimaryResource(12f);
                if (Rune_B > 0)//Execution
                {
                    if (hit.Target.Attributes[GameAttributes.Hitpoints_Cur] < (hit.Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 5))
                        if (FastRandom.Instance.Chance(95))
                            WeaponDamage(hit.Target, 99999f, DamageType.Physical);
                }
                else if (Rune_D > 0)//Pare 
                {
                    WeaponDamage(hit.Target, 1.50f, DamageType.Physical);
                }
                else if (Rune_E > 0)//Cursed Scythe
                {
                    if (RandomHelper.Next(1, 100) >= 85)
                    {
                        //Random debuff
                        AddBuff(Target, new DebuffChilled(0.75f, WaitSeconds(30f)));
                    }
                }
                else if (Rune_C > 0) //Frost Harvest
                {
                    
                    AddBuff(User, new FBuff());
                    //AddBuff(User, new FBuff());
                    //AddBuff(User, new SBuff());
                    //AddBuff(User, new TBuff());
                    //*/
                }
                else if (Rune_A > 0) //Dark Harvest
                {
                    ((Player) User).AddPercentageHP(1);
                }
            };
            attack.Apply();

            yield break;
        }

        public override float GetContactDelay()
        {
            // seems to need this custom speed for all attacks
            return 0.5f;
        }
        [ImplementsPowerBuff(0, true)]
        public class ZBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(1, true)]
        public class FBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(2, true)]
        public class SBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(3, true)]
        public class TBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }

    }

    #endregion
    //Done
    #region SiphonBlood
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.MainSkills.SiphonBlood)]
    public class SiphonBlood : ChanneledSkill
    {
        const float MaxBeamLength = 40f;
        private Actor _beamEnd;
        private Actor Effect;
        private Vector3D _calcBeamEnd(float length)
        {
            return PowerMath.TranslateDirection2D(User.Position, TargetPosition,
                                                  new Vector3D(User.Position.X, User.Position.Y, TargetPosition.Z),
                                                  length);
        }

        public override void OnChannelOpen()
        {
            EffectsPerSecond = 0.5f;

            {
                _beamEnd = SpawnEffect(ActorSno._p6_necro_siphonblood_a_target_attractchunks, User.Position, 0, WaitInfinite());

            }
        }

        private bool _channelClosed;
        public override void OnChannelClose()
        {
            if (_channelClosed)
                return; 
            _beamEnd?.Destroy();
            Effect?.Destroy();
            _channelClosed = true;
        }

        public override void OnChannelUpdated()
        {
            User.TranslateFacing(TargetPosition);
        }

        public override IEnumerable<TickTimer> Main()
        {
            var PowerData = (DiIiS_NA.Core.MPQ.FileFormats.Power)MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;
            AttackPayload attack = new AttackPayload(this);
            {
                attack.Targets ??= new TargetList();
                attack.Targets.Actors ??= new List<Actor>();
                if (Target != null)
                    attack.Targets.Actors.Add(Target);
                DamageType DType = DamageType.Physical;
                if (Rune_A > 0) DType = DamageType.Cold;
                else if (Rune_D > 0) DType = DamageType.Poison;
                attack.AddWeaponDamage(3f, DType);
                attack.OnHit = hit =>
                {
                    Effect = SpawnProxy(hit.Target.Position);
                    Effect.AddComplexEffect(RuneSelect(467461, 467557, 467500, 467643, 469460, 469275), _beamEnd);
                    //Effect.AddComplexEffect(baseEffectSkill, _beamEnd); 
                    AddBuff(hit.Target, new DebuffChilled(0.3f, WaitSeconds(0.5f)));
                    ((Player) User).AddPercentageHP(2);
                    if (Rune_C < 1)
                        GeneratePrimaryResource(15f);

                };

                if (Rune_E > 0)//Bloodsucker
                {
                    //we are looking for all actors in a radius of 40
                    ((Player) User).VacuumPickupHealthOrb(40f);
                }
                else if (Rune_A > 0)//energy shift
                {
                    //75% slow on movement
                    AddBuff(Target, new DebuffChilled(0.75f, WaitSeconds(1f)));
                }
                else if (Rune_D > 0)//energy shift
                {
                    //10 stacks of 10% damage amplification
                    AddBuff(User, new BustBuff());
                }
                else if (Rune_B > 0)//Pure Essence
                {
                    //If HP is 100% - we restore more essence.
                    if (User.Attributes[GameAttributes.Hitpoints_Cur] == User.Attributes[GameAttributes.Hitpoints_Max_Total])
                        GeneratePrimaryResource(5f);
                }
                else if (Rune_C > 0)//Theft of life
                {
                    ((Player) User).AddPercentageHP(4);
                    //10 stacks of 10% damage amplification.
                }
            }
            attack.Apply();
            yield break;
        }
        [ImplementsPowerBuff(6, true)]
        public class BustBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += 0.1f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += 0.1f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= StackCount * 0.1f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }

    }

    #endregion
    //Done
    #region BoneSpear
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.SecondrySkills.BoneSpear)]
    public class BoneSpear : Skill
    {
        #region Content
        /*
            [468487] [Actor] p6_necro_boneSpear01_blood
            [468495] [Actor] p6_necro_boneSpear01_blood_cylinderMesh
            [468568] [Actor] p6_necro_boneSpear_blood_spawn_castMagic
            [468573] [Actor] p6_necro_boneSpear_blood_spawn
            [468815] [Actor] p6_necro_boneSpear01_blood_cylinderMesh_dark
            [469253] [Actor] p6_necro_boneSpear01_decay
            [469260] [Actor] p6_necro_boneSpear01_decay_cylinderMesh_add
            [469264] [Actor] p6_necro_boneSpear01_decay_cylinderMesh_blend
            [469347] [Actor] p6_necro_boneSpear_decay_spawn
            [469350] [Actor] p6_necro_boneSpear_decay_spawn_castMagic
            [469502] [Actor] p6_necro_boneSpear_death_runeA
            [469503] [Actor] p6_necro_boneSpear_death_runeC
            [469504] [Actor] p6_necro_boneSpear_death_runeD
            [469505] [Actor] p6_necro_boneSpear_death_runeE
            [471682] [Actor] p6_necro_boneSpear_e_teeth_death_distortionSphere
            [471685] [Actor] p6_necro_boneSpear_e_teeth_sphereActor_blood
            [471692] [Actor] p6_necro_boneSpear_e_teeth_cast_spikeActor
            [471725] [Actor] p6_necro_boneSpear01_teeth_01
            [471727] [Actor] p6_necro_boneSpear01_teeth_02
            [471728] [Actor] p6_necro_boneSpear01_teeth_03
            [471732] [Actor] p6_necro_boneSpear_e_teeth_fakeProjectile_actorParent
            [452802] [Actor] p6_necro_boneSpear01
            [454564] [Actor] p6_necro_boneSpear01_spawn
            [455955] [Actor] p6_necro_boneSpear_death_base
            [456021] [Actor] P6_Necro_BoneSpear_Death_distortionSphere
            [457892] [Actor] p6_necro_boneSpear_wakeLong01
            [458206] [Actor] p6_necro_boneSpear_castMagic01
            [460136] [Actor] p6_necro_boneSpear01_ghostly
            [460145] [Actor] p6_necro_boneSpear_ghostly_cylinderMesh
            [460156] [Actor] p6_necro_boneSpear_ghostly_spawn
            [460159] [Actor] p6_necro_boneSpear_ghostly_castMagic01
            [465211] [Actor] p6_necro_boneSpear01_shatter_explosion
            [465233] [Actor] P6_Necro_BoneSpear_shatter_explosion_distortionSphere
            [471569] [Anim] p6_necro_boneSpear_e_teeth_idle_0
            [452804] [Anim] boneSpear01_RC_idle_0
            [454575] [Anim] p6_necro_boneSpear01_intro_idle_02
            [454906] [Anim] p6_Necro_Male_HTH_Cast_BoneSpear
            [455948] [Anim] p6_necro_boneSpear01_death_idle_0
            [462323] [Anim] p6_Necro_Female_HTH_Cast_BoneSpear
            [452807] [AnimSet] p6_nm_boneSpear02
            [471568] [AnimSet] p6_necro_boneSpear01_teeth
            [454566] [AnimSet] p6_nm_boneSpear01_spawn
            [455947] [AnimSet] p6_necro_boneSpear01_death
            [456994] [EffectGroup] p6_necro_boneSpear_hitEffect
            [468566] [EffectGroup] p6_necro_boneSpear_blood_castFX
            [460154] [EffectGroup] P6_Necro_BoneSpear_ghostly_castFX
            [460173] [EffectGroup] p6_necro_boneSpear_hitEffect_flash_master
            [469345] [EffectGroup] P6_Necro_BoneSpear_decay_castFX
            [469550] [EffectGroup] p6_necro_boneSpear_decay_death
            [469564] [EffectGroup] p6_necro_boneSpear_c_impact_00
            [469565] [EffectGroup] p6_necro_boneSpear_c_impact_01
            [469566] [EffectGroup] p6_necro_boneSpear_c_impact_02
            [469567] [EffectGroup] p6_necro_boneSpear_c_impact_03
            [469568] [EffectGroup] p6_necro_boneSpear_c_impact_04
            [469569] [EffectGroup] p6_necro_boneSpear_c_impact_05
            [469571] [EffectGroup] p6_necro_boneSpear_blood_death
            [470160] [EffectGroup] p6_necro_boneSpear_c_impact
            [470162] [EffectGroup] p6_necro_boneSpear_c_impact_XX
            [470202] [EffectGroup] p6_necro_boneSpear_default_impact
            [470294] [EffectGroup] p6_necro_boneSpear_ice_death
            [471567] [EffectGroup] p6_necro_boneSpear_e_teeth_castFx
            [471674] [EffectGroup] p6_necro_boneSpear_e_teeth_death
            [454559] [EffectGroup] P6_Necro_BoneSpear_castFX
            [455943] [EffectGroup] P6_Necro_BoneSpear_Death
            [465209] [EffectGroup] p6_necro_boneSpear_shatter_explosion
            [465222] [EffectGroup] p6_necro_boneSpear_castEffect_picker
            [465271] [EffectGroup] p6_necro_boneSpear_hitEffect_switch
            [451490] [Power] P6_Necro_BoneSpear
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            var PowerData = (DiIiS_NA.Core.MPQ.FileFormats.Power)MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;
           
            UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));
            if (Rune_E > 0)
            {
                TargetPosition = PowerMath.TranslateDirection2D(User.Position, TargetPosition, User.Position, 7f);
                AttackPayload attack = new AttackPayload(this);
                attack.Targets = GetEnemiesInRadius(TargetPosition, 7f);
                attack.AddWeaponDamage(3.00f, DamageType.Physical);
                attack.OnHit = hit =>
                {};
                attack.Apply();
            }
            else
            {
                var actor = ActorSno._p6_necro_bonespear01_ghostly;
                if (Rune_C > 0)
                    actor = ActorSno._p6_necro_bonespear01_decay;
                if (Rune_A > 0) //Crystallization
                    actor = ActorSno._p6_necro_bonespear01;//452802
                if (Rune_D > 0) //Blood Spear
                {
                    actor = ActorSno._p6_necro_bonespear01_blood;
                    ((Player) User).AddPercentageHP(-10);
                }
                var projectile = new Projectile(this, actor, User.Position);
                projectile.Position.Z += 5f;  // fix height
                float percentExtraDamage = 0;
                DamageType damageType = DamageType.Physical;

                projectile.OnCollision = (hit) =>
                {
                    if (Rune_B > 0)
                    {
                        //var Targs = GetEnemiesInRadius(hit.Position, 15f);
                        foreach (var target in GetEnemiesInRadius(hit.Position, 15f).Actors)
                        {
                            WeaponDamage(target, 5.0f, DamageType.Physical);
                        }

                        hit.PlayEffect(Effect.PlayEffectGroup, 465209);
                        projectile.Destroy();
                    }
                    else
                    {
                        if (Rune_C > 0)
                        {
                            percentExtraDamage += 0.15f;
                            damageType = DamageType.Poison;
                        }
                        if (Rune_A > 0)
                        {
                            damageType = DamageType.Cold;
                            AddBuff(hit, new SBuff());
                            AddBuff(User, new FBuff());
                        }

                        hit.PlayEffect(Effect.PlayEffectGroup, 456994);
                        WeaponDamage(hit, Rune_D > 0 ? 6.5f : 5f + percentExtraDamage, damageType);
                    }

                };


                projectile.Launch(TargetPosition, 1.8f);

                projectile.OnUpdate = () =>
                {
                    //if (!User.World.CheckLocationForFlag(projectile.Position, DiIiS_NA.Core.MPQ.FileFormats.Scene.NavCellFlags.AllowProjectile))
                        // projectile.Destroy();
                };

                yield return WaitSeconds(5f);
                //if (projectile.World != null)
                //    projectile.Destroy();
            }
            yield break;
        }

        [ImplementsPowerBuff(0)]
        public class FirstBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(3f);
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }
        }

        [ImplementsPowerBuff(1, true)]
        public class FBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;
               
                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Attacks_Per_Second_Percent] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(2, true)]
        public class SBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {

                if (!base.Apply())
                    return false;
                Target.Attributes[GameAttributes.Attacks_Per_Second_Percent] -= 0.2f;
                Target.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                }
                return true;
            }
            public override void Remove()
            {
                Target.Attributes[GameAttributes.Attacks_Per_Second_Percent] += 0.2f;
                Target.Attributes.BroadcastChangedIfRevealed();

                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
    }
    #endregion
    //WIP: Rune D and Rune E
    #region SummonSkeletalMage
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.SecondrySkills.SkeletalMage)]
    public class SummonSkeletalMage : Skill
    {
        public float Count = 0;
        #region Content
        /*
            
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_A_death", 472584);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_A_rangedAttack", 472442);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_A_spawn", 472276);

            DictSNOEffectGroup.Add("p6_necro_skeletonMage_B_death", 472595);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_B_rangedAttack", 472754);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_B_spawn", 472596);

            DictSNOEffectGroup.Add("p6_necro_skeletonMage_C_death", 472613);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_C_rangedAttack", 472652);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_C_spawn", 472614);

            DictSNOEffectGroup.Add("p6_necro_skeletonMage_D_death", 472717);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_D_rangedAttack", 472719);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_D_spawn", 472718);

            DictSNOEffectGroup.Add("p6_necro_skeletonMage_E_aoeAttack", 472876);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_E_death", 472780);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_E_spawn", 472781);

            DictSNOEffectGroup.Add("p6_necro_skeletonMage_F_death", 472804);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_F_spawn", 472803);

            DictSNOEffectGroup.Add("p6_necro_skeletonMage_runeSwitch_attack", 472651);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_runeSwitch_death", 472594);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_runeSwitch_spawn", 472597);
            DictSNOEffectGroup.Add("p6_necro_skeletonMage_runeSwitch_spawnAttack", 472661);

            DictSNOActor.Add("p6_necro_skeletonMage_A", 472275);
            DictSNOActor.Add("p6_necro_skeletonMage_B", 472588);
            DictSNOActor.Add("p6_necro_skeletonMage_C", 472606);
            DictSNOActor.Add("p6_necro_skeletonMage_D", 472715);
            DictSNOActor.Add("p6_necro_skeletonMage_E", 472769);
            DictSNOActor.Add("p6_necro_skeletonMage_F_archer", 472801);
            DictSNOActor.Add("p6_necro_skeletonMage_F_archer_projectile", 472884);

        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            
            if (Rune_B > 0)
            {
                Count = User.Attributes[GameAttributes.Resource_Cur, (int)((Player) User).Toon.HeroTable.PrimaryResource];
                UsePrimaryResource(Count);
            }
            else if (Rune_C > 0)
            {
                ((Player)User).AddPercentageHP(-10f);
                UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));
            }
            else
                UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));
            
            var dataOfSkill = MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;
            
            var mage = new SkeletalMage(
                World,
                this,
                0,
                RuneSelect(
                    ActorSno._p6_necro_skeletonmage_a,
                    ActorSno._p6_necro_skeletonmage_b,
                    ActorSno._p6_necro_skeletonmage_c,
                    ActorSno._p6_necro_skeletonmage_d,
                    ActorSno._p6_necro_skeletonmage_e,
                    ActorSno._p6_necro_skeletonmage_f_archer
                )
            );
            mage.Brain.DeActivate();
            
            mage.Scale = 1.2f;
            mage.Position = RandomDirection(TargetPosition, 3f, 8f);
            mage.Attributes[GameAttributes.Untargetable] = true;
            mage.EnterWorld(mage.Position);
            yield return WaitSeconds(0.05f);

            mage.Brain.Activate();
            mage.PlayEffectGroup(RuneSelect(472276, 472596, 472614, 472718, 472781, 472803));
            ((MinionBrain)mage.Brain).PresetPowers.Clear();
            if (Rune_D > 0)//Infection
                AddBuff(mage, new BustBuff7());
            else if (Rune_E > 0)//archer frosty
                ((MinionBrain)mage.Brain).AddPresetPower(30499);
            else
                ((MinionBrain)mage.Brain).AddPresetPower(466879);

            mage.Attributes[GameAttributes.Untargetable] = false;
            mage.Attributes.BroadcastChangedIfRevealed();
            yield break;
        }
        [ImplementsPowerBuff(1)]
        public class BustBuff1 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(2)]
        public class BustBuff2 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(3, true)]
        class SkeletalMageBuff : PowerBuff
        {
            public SkeletalMageBuff()
            {
            }

            public override void OnPayload(Payload payload)
            {

            }
        }
        [ImplementsPowerBuff(4)]
        public class BustBuff4 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }

        [ImplementsPowerBuff(5)]
        public class BustBuff5 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }

        [ImplementsPowerBuff(6)]
        public class BustBuff6 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        } //Вызванный скелет-маг

        [ImplementsPowerBuff(7)]
        public class BustBuff7 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        } //Угасающая жизнь - TODO: на него повесить жизнь скелета.

        [ImplementsPowerBuff(8)]
        public class BustBuff8 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(3);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }


    }
    #endregion
    //Done
    #region DeathNova
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.SecondrySkills.DeathNova)]
    public class DeathNova : Skill
    {
        #region Content
        /*
            [467107] [Actor] p6_necro_bloodNova_B_boneNova
            [470658] [Actor] p6_necro_bloodNova_tendril_handVeins_01
            [470690] [Actor] p6_necro_bloodNova_tendril_headVeins01
            [470790] [Actor] p6_necro_bloodNova_tendril_footVeins
            [471415] [Actor] p6_necro_bloodNova_d_decay_distortionSphere
            [474299] [Actor] p6_necro_bloodNova_noRune_decay_distortionSphere
            [462392] [Actor] p6_necro_bloodNova_wave01
            [470678] [Anim] p6_necro_bloodNova_tendril_handVeins_idle_0
            [470692] [Anim] p6_necro_bloodNova_tendril_headVeins01_idle_0
            [470792] [Anim] p6_necro_bloodNova_tendril_footVeins_idle_0
            [462237] [Anim] p6_Necro_Male_HTH_Cast_BloodNova
            [462314] [Anim] p6_Necro_Female_HTH_Cast_BloodNova
            [462477] [Anim] p6_necro_bloodNova_circleWave_small_idle_0
            [467108] [AnimSet] p6_necro_bloodNova_B_boneNova
            [470677] [AnimSet] p6_necro_bloodNova_tendril_handVeins
            [470691] [AnimSet] p6_necro_bloodNova_tendril_headVeins
            [470791] [AnimSet] p6_necro_bloodNova_tendril_footVeins
            [462394] [AnimSet] p6_necro_bloodNova_circleWave
            [466321] [EffectGroup] p6_necro_bloodNova_aTendril
            [466324] [EffectGroup] p6_necro_bloodNova_bBone
            [471115] [EffectGroup] p6_necro_bloodNova_D_groundAoE
            [462662] [EffectGroup] p6_necro_bloodNova_02
            [472863] [EffectGroup] p6_necro_bloodNova_d_decay02
            [474290] [EffectGroup] p6_necro_bloodNova_noRune
            [474410] [EffectGroup] p6_necro_bloodNova_e_Small
            [474421] [EffectGroup] p6_necro_bloodNova_e_large
            [474432] [EffectGroup] p6_necro_bloodNova_e_medium
            [474457] [EffectGroup] p6_necro_bloodNova_castEffect_picker
            [474458] [EffectGroup] p6_necro_bloodNova_castEffect_noRune
            [474459] [EffectGroup] p6_necro_bloodNova_castEffect_a
            [474460] [EffectGroup] p6_necro_bloodNova_castEffect_b
            [474461] [EffectGroup] p6_necro_bloodNova_castEffect_c
            [474462] [EffectGroup] p6_necro_bloodNova_castEffect_d
            [474463] [EffectGroup] p6_necro_bloodNova_castEffect_e
            [462243] [Power] P6_Necro_BloodNova
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {

            UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));
            //462392
            //var Point = SpawnEffect(462194, TargetPosition, 0, WaitSeconds(0.2f));
            //Point.PlayEffect(Effect.PlayEffectGroup, 459954);
            float Radius = 25f;
            float Dmg = 3.5f;
            DamageType DType = DamageType.Poison;
            User.PlayEffectGroup(RuneSelect(474458, 474459, 474460, 474461, 474462, 474463));
            int boomEffect = 474290;
            if (Rune_E > 0)
                switch (((Player) User).SpecialComboIndex)
                {
                    case 0:
                        ((Player) User).SpecialComboIndex++;
                        boomEffect = 474410;
                        break;
                    case 1:
                        Radius = 30f;
                        ((Player) User).SpecialComboIndex++;
                        boomEffect = 474432;
                        break;
                    case 2:
                        Radius = 35f;
                        ((Player) User).SpecialComboIndex = 0;
                        boomEffect = 474421;
                        break;
                }
            else ((Player) User).SpecialComboIndex = 0;
            if (Rune_A > 0)
            {
                Dmg = 2.25f;
                boomEffect = 466321;
                DType = DamageType.Physical;
            }
            else if (Rune_B > 0)
            {
                Dmg = 4.75f;
                boomEffect = 466324;
                DType = DamageType.Physical;
            }
            else if (Rune_C > 0)
            {
                ((Player) User).AddPercentageHP(-10);
                Dmg = 4.5f;
                boomEffect = 462662;
                DType = DamageType.Physical;
            }
            else if (Rune_D > 0)
            {
                boomEffect = 472863;
                DType = DamageType.Poison;
                var Proxy = SpawnProxy(User.Position, new TickTimer(User.World.Game, 300));
                Proxy.PlayEffectGroup(471115);
                foreach (var act in GetEnemiesInRadius(TargetPosition, 25f).Actors)
                {
                    AddBuff(act, new DebuffChilled(0.60f, WaitSeconds(1f)));
                }
            }
            User.PlayEffectGroup(boomEffect);
            AttackPayload attack = new AttackPayload(this)
            {
                Targets = GetEnemiesInRadius(User.Position, Radius)
            };
            attack.AddWeaponDamage(Dmg, DType);
            attack.OnHit = hit =>
            {
                if (Rune_A > 0)
                    ((Player) User).AddPercentageHP(1);
            };

            attack.Apply();
            yield break;
        }
    }
    #endregion
    //Done - testing, apparently Rune_A not working.
    #region CorpseExlosion

    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.CorpseExlosion)]
    public class CorpseExplosion : Skill
    {
        public override IEnumerable<TickTimer> Main()
        {
            // Initializing main variables for Bonespikes ability.
            float radius = 20f;
            float damage = 10.5f;
            DamageType damageType = DamageType.Physical;

            // Fetching the data for the respective Power from the MPQ cache.
            var powerData = (DiIiS_NA.Core.MPQ.FileFormats.Power)MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;

            // Creating a point effect on the target position, playing various effect groups depending on the selected Rune.
            var point = SpawnEffect(ActorSno._p6_necro_bonespikes, TargetPosition, 0, WaitSeconds(0.2f));
            point.PlayEffect(Effect.PlayEffectGroup, RuneSelect(459954, 473926, 459954, 473907, 459954, 473864));

            // Depending on a specific game attribute, either spawn a new monster at the target position, or select up to five existing corpses.
            var actors = User.Attributes[GameAttributes.Necromancer_Corpse_Free_Casting]
                ? new List<uint> { User.World.SpawnMonster(ActorSno._p6_necro_corpse_flesh, TargetPosition).GlobalID }
                : User.GetActorsInRange(TargetPosition, 11).Where(x => x.SNO == ActorSno._p6_necro_corpse_flesh)
                    .Select(x => x.GlobalID).Take(5).ToList();

            // Modifying main parameters of the ability depending on the selected Rune.
            if (Rune_D > 0)
            {
                radius = 25f;
            }
            else if (Rune_C > 0) // Licking action.
            {
                damage = 15.75f;
                damageType = DamageType.Poison;
            }
            else if (Rune_A > 0)
            {
                damageType = DamageType.Poison;
            }

            // Applying the effects of the Bonespikes ability on the selected corpses.
            foreach (var actor in actors)
            {
                if (Rune_B > 0)
                {
                    var bomb = World.GetActorByGlobalId(actor);
                    var nearestEnemy = bomb.GetActorsInRange(20f).First();
                    if (nearestEnemy != null)
                        bomb.Teleport(nearestEnemy.Position);
                }

                // Spawning explosion effect.
                var explosionEffect = SpawnEffect(
                    ActorSno._p6_necro_corpseexplosion_projectile_spawn,
                    World.GetActorByGlobalId(actor).Position,
                    ActorSystem.Movement.MovementHelpers.GetFacingAngle(User, World.GetActorByGlobalId(actor)),
                    WaitSeconds(0.2f)
                );
                explosionEffect.PlayEffect(Effect.PlayEffectGroup,
                    RuneSelect(457183, 471539, 471258, 471249, 471247, 471236));
                explosionEffect.UpdateDelay = 0.1f;

                explosionEffect.OnUpdate = () =>
                {
                    // Creating the attack payload.
                    AttackPayload attack = new(this)
                    {
                        Targets = GetEnemiesInRadius(User.Position, radius)
                    };

                    if (Rune_E > 0)
                        damageType = DamageType.Cold;

                    // Applying weapon damage.
                    attack.AddWeaponDamage(damage, damageType);
                    attack.OnHit = hitPayload =>
                    {
                        if (Rune_E > 0)
                            AddBuff(hitPayload.Target, new DebuffFrozen(WaitSeconds(2f)));
                    };
                    // Applying the attack.
                    attack.Apply();
                };
                // Destroying the selected corpse.
                World.GetActorByGlobalId(actor).Destroy();
            }

            yield break;
        }
    }

    #endregion
    //Done
    #region CorpseLance

    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.CorpseLance)]
    public class CorpseLance : ChanneledSkill
    {
        private Actor _beamEnd;

        private bool WaitTo(TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }

        private Vector3D _calcBeamEnd(float length)
        {
            return PowerMath.TranslateDirection2D(User.Position, TargetPosition,
                                                  new Vector3D(User.Position.X, User.Position.Y, TargetPosition.Z),
                                                  length);
        }

        public override void OnChannelOpen()
        {
            WaitForSpawn = true;
            WaitSeconds = 0.75f;
            EffectsPerSecond = 0.75f;
        }

        public override void OnChannelClose()
        {

        }

        public override void OnChannelUpdated()
        {
            User.TranslateFacing(TargetPosition);
            Actor Flesh = null;
            if (User.Attributes[GameAttributes.Necromancer_Corpse_Free_Casting] == true)
            {
                Flesh = User;
            }
            else
                Flesh = User.GetActorsInRange<ActorSystem.Implementations.NecromancerFlesh>(60f).First();
            var PowerData = (DiIiS_NA.Core.MPQ.FileFormats.Power)MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;
            DamageType DType = DamageType.Physical;
            var Explosion = SpawnEffect(ActorSno._p6_necro_corpseexplosion_projectile, Flesh.Position, 0, WaitSeconds(0.2f));
            Explosion.PlayEffect(Effect.PlayEffectGroup, 457183);
            var Proxy = SpawnProxy(Flesh.Position, new TickTimer(User.World.Game, 300));
            if (User.Attributes[GameAttributes.Necromancer_Corpse_Free_Casting] == false)
                Flesh.Destroy();
            //1, 2, 3, 4
            if (Rune_E > 0 || Rune_A > 0)
                DType = DamageType.Poison;
            if (Rune_B > 0)
                DType = DamageType.Cold;

            foreach (var plr in User.World.Players.Values)
            {
                plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                {
                    //A, D, E?
                    EffectSNOId = RuneSelect(468032, 468032, 468240, 467966, 468032, 474474),//468032,
                    ActorID = Proxy.DynamicID(plr),
                    TargetID = Target.DynamicID(plr)
                });
                plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                {
                    EffectSNOId = 474690,
                    ActorID = Target.DynamicID(plr),
                    TargetID = Proxy.DynamicID(plr)
                });
            }

            if (Rune_C > 0)
            {
                var newProxy = SpawnProxy(User.Position, new TickTimer(User.World.Game, 300));
                
                foreach (var plr in User.World.Players.Values)
                {
                    plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                    {
                        //A, D, E?
                        EffectSNOId = RuneSelect(468032, 468032, 468240, 467966, 468032, 474474),//468032,
                        ActorID = newProxy.DynamicID(plr),
                        TargetID = Target.DynamicID(plr)
                    });
                    plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                    {
                        EffectSNOId = 474690,
                        ActorID = Target.DynamicID(plr),
                        TargetID = newProxy.DynamicID(plr)
                    });
                }

                TickTimer timeout = new SecondsTickTimer(Target.World.Game, 0.4f);
                var boom = Task<bool>.Factory.StartNew(() => WaitTo(timeout));
                boom.ContinueWith(delegate
                {
                    Target.PlayEffect(Effect.PlayEffectGroup, 456994);
                    WeaponDamage(Target, 5.25f, DType);
                });

            }

            TickTimer Timeout = new SecondsTickTimer(Target.World.Game, 0.4f);
            var Boom = Task<bool>.Factory.StartNew(() => WaitTo(Timeout));
            Boom.ContinueWith(delegate
            {
                Target.PlayEffect(Effect.PlayEffectGroup, 456994);
                WeaponDamage(Target, 17.5f, DType);

                if (Rune_A > 0)// && RandomHelper.Next(1,100) > 80) 
                {
                    var NewProxy = SpawnProxy(Target.Position, new TickTimer(User.World.Game, 300));
                    Actor NewTarget = null;
                    var NearTargets = GetEnemiesInRadius(Target.Position, 24f).Actors;

                    foreach (var en in NearTargets) if (en is Monster) { NewTarget = en; break; }

                    if (NewTarget != null)
                    {
                        foreach (var plr in User.World.Players.Values)
                        {
                            plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                            {
                                //A, D, E?
                                EffectSNOId = RuneSelect(468032, 468032, 468240, 467966, 468032, 474474),//468032,
                                ActorID = NewProxy.DynamicID(plr),
                                TargetID = NewTarget.DynamicID(plr)
                            });
                            plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                            {
                                EffectSNOId = 474690,
                                ActorID = NewTarget.DynamicID(plr),
                                TargetID = NewProxy.DynamicID(plr)
                            });
                        }

                        TickTimer Timeout1 = new SecondsTickTimer(Target.World.Game, 0.3f);
                        var Boom = Task<bool>.Factory.StartNew(() => WaitTo(Timeout1));
                        Boom.ContinueWith(delegate
                        {
                            NewTarget.PlayEffect(Effect.PlayEffectGroup, 456994);
                            WeaponDamage(NewTarget, 17.5f, DType);
                        });
                    }
                }
                else if (Rune_D > 0)
                {
                    AddBuff(Target, new DebuffStunned(WaitSeconds(3f)));
                }
                else if (Rune_B > 0)
                {
                    AddBuff(Target, new BustBuff2());
                }
                else if (Rune_E > 0)
                {
                    AddBuff(Target, new BustBuff3());
                }
            });
        }

        public override IEnumerable<TickTimer> Main()
        {

            yield break;
        }

        [ImplementsPowerBuff(2)] //Хрупкость
        public class BustBuff2 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(5);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Bonus_Chance_To_Be_Crit_Hit] += 0.05f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Bonus_Chance_To_Be_Crit_Hit] -= 0.05f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(3, true)] //Яд
        public class BustBuff3 : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(5);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= 0.10f;
                User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= 0.06f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= 0.10f;
                    User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= 0.06f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] += StackCount * 0.10f;
                User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += StackCount * 0.06f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
    }
    #endregion
    //Done
    #region Devour
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.Devour)]
    public class Devour : Skill
    {
        #region Content
        /*  [461751] [Actor] p6_necro_devour_A_attractor_geo
            [462752] [Actor] p6_necro_devour_B_attractor_geo
            [467226] [Actor] p6_necro_devour_C_attractor_geo
            [470538] [Actor] p6_necro_devour_D_attractor_geo
            [470565] [Actor] p6_necro_devour_E_attractor_geo
            
            [473774] [AmbientSound] P6_Skill_Necro_Devour_Rune_Aura

            [467186] [EffectGroup] p6_necro_devour_attractor_runeSwitch
            [467188] [EffectGroup] p6_necro_devour_cast_runeSwitch
            [467190] [EffectGroup] p6_necro_devour_corpseEat_runeSwitch

            [467193] [EffectGroup] p6_necro_devour_A_attractor
            [467196] [EffectGroup] p6_necro_devour_A_cast
            [467200] [EffectGroup] p6_necro_devour_A_corpseEat

            [462750] [EffectGroup] p6_necro_devour_B_attractor
            [462756] [EffectGroup] p6_necro_devour_B_corpseEat
            [462759] [EffectGroup] p6_necro_devour_B_cast

            [467228] [EffectGroup] p6_necro_devour_C_attractor
            [467229] [EffectGroup] p6_necro_devour_C_cast
            [467230] [EffectGroup] p6_necro_devour_C_corpseEat

            [470480] [EffectGroup] p6_necro_devour_D_attractor
            [470481] [EffectGroup] p6_necro_devour_D_cast
            [470482] [EffectGroup] p6_necro_devour_D_corpseEat
            [472217] [EffectGroup] p6_necro_devour_D_Aura
            
            [470547] [EffectGroup] p6_necro_devour_E_attractor
            [470548] [EffectGroup] p6_necro_devour_E_cast
            [470549] [EffectGroup] p6_necro_devour_E_corpseEat

            [470573] [EffectGroup] p6_necro_devour_F_attractor
            [470574] [EffectGroup] p6_necro_devour_F_corpseEat
            [470575] [EffectGroup] p6_necro_devour_F_cast

        
            [474325] [Power] P6_Necro_Devour_Aura
            [460757] [Power] P6_Necro_Devour
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            var DataOfSkill = MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;
            //454066
            var Flesh = User.GetActorsInRange<ActorSystem.Implementations.NecromancerFlesh>(60f);
            foreach (var act in Flesh)
            {
                var Proxy = SpawnProxy(User.Position, new TickTimer(User.World.Game, 300));

                foreach (var plr in User.World.Players.Values)
                {
                    plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                    {
                        EffectSNOId = RuneSelect(467193, 462750, 467228, 470480, 470547, 470573),
                        TargetID = Proxy.DynamicID(plr),
                        ActorID = act.DynamicID(plr)
                    });
                }
                act.PlayEffectGroup(RuneSelect(467200, 462756, 467230, 470482, 470549, 470574));
                act.Destroy();
                User.Attributes[GameAttributes.Resource_Cur, (int)((Player) User).Toon.HeroTable.PrimaryResource] += 10f;

                if (Rune_A > 0)
                    ((Player) User).AddPercentageHP(3);
                else if (Rune_E > 0)
                    AddBuff(User, new SBuff()); //satiety
                else if (Rune_C > 0)
                    AddBuff(User, new TBuff()); //Gluttony
                else if (Rune_B > 0) //Inhumanity
                    foreach (var minion in User.GetActorsInRange<Minion>(60f))
                    {
                        if (((Player) User).FindFollowerIndex(minion.SNO) == 0)
                            break;
                        else
                        {
                            minion.Destroy();
                            User.Attributes[GameAttributes.Resource_Cur, (int)((Player) User).Toon.HeroTable.PrimaryResource] += 10f;
                        }
                    }
                User.Attributes.BroadcastChangedIfRevealed();

            }

            yield break;
        }

        [ImplementsPowerBuff(0, true)]
        public class FBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Hitpoints_Max_Percent_Bonus_Multiplicative] += 0.3f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Hitpoints_Max_Percent_Bonus_Multiplicative] += 0.3f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Hitpoints_Max_Percent_Bonus_Multiplicative] -= StackCount * 0.3f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(1, true)] //satiety
        public class SBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Hitpoints_Max_Percent_Bonus_Multiplicative] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Hitpoints_Max_Percent_Bonus_Multiplicative] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Hitpoints_Max_Percent_Bonus_Multiplicative] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(2, true)] //Ненасытность
        public class TBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(5);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                ((Player) User).DecreaseUseResourcePercent += 0.02f;

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes.BroadcastChangedIfRevealed();
                    ((Player) User).DecreaseUseResourcePercent += 0.02f;
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                ((Player) User).DecreaseUseResourcePercent -= StackCount * 0.02f;
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
    }
    #endregion
    //Done
    #region Revive
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.Revive)]
    public class Revive : Skill
    {
        #region Content
        
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            var skillData = MPQStorage.Data.Assets[SNOGroup.Power][PowerSNO].Data;
            //454066
            var Proxy = SpawnProxy(TargetPosition, new TickTimer(User.World.Game, 300));
            var Flesh = Proxy.GetActorsInRange<ActorSystem.Implementations.NecromancerFlesh>(20f);
            bool Resurrected = false;

            if (Rune_B > 0)
                ((Player) User).AddPercentageHP(-3);

            Proxy.PlayEffectGroup(RuneSelect(465009, 465021, 465016, 465027, 465011, 465026));
            foreach (var act in Flesh)
            {
                if (((Player) User).Revived.Count < 10)
                {
                    var Temp = User.World.SpawnMonster((ActorSno)act.Attributes[GameAttributes.Necromancer_Corpse_Source_Monster_SNO], act.Position);
                    var RevivedTemp = new Minion(User.World, Temp.SNO, User, Temp.Tags, false, true);
                    Temp.Destroy();

                    RevivedTemp.EnterWorld(act.Position);
                    if (Rune_A < 1)
                        act.Destroy();
                    if (Rune_D > 0)
                    {
                        RevivedTemp.LifeTime = TickTimer.WaitSeconds(User.World.Game, 10f);
                        RevivedTemp.Attributes[GameAttributes.Damage_Weapon_Min, 0] *= 1.25f;
                        RevivedTemp.Attributes.BroadcastChangedIfRevealed();
                    }
                    if (Rune_B > 0)
                    {
                        RevivedTemp.Attributes[GameAttributes.Damage_Weapon_Min, 0] *= 1.2f;
                        RevivedTemp.Attributes.BroadcastChangedIfRevealed();
                    }
                    RevivedTemp.SetBrain(new AISystem.Brains.MinionBrain(RevivedTemp));
                    RevivedTemp.Brain.Activate();
                    RevivedTemp.PlayEffectGroup(RuneSelect(464739, 464900, 464872, 464954, 464859, 464746));
                    ((Player) User).Revived.Add(RevivedTemp);
                    Resurrected = true;
                    RevivedTemp.Attributes[GameAttributes.Team_Override] = 1;
                    RevivedTemp.Attributes.BroadcastChangedIfRevealed();
                }
                else
                    break;
            }
            if (Resurrected)
                if (Rune_E > 0)
                {
                    var enemies = Proxy.GetActorsInRange<Monster>(20f);
                    foreach (var enemy in enemies)
                        AddBuff(enemy, new DebuffFeared(WaitSeconds(3f)));
                }

            yield break;
        }

    }
    #endregion
    //TODO: Rune_E and overall buff check
    #region CommandSkeleton
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.CommandSkeleton)]
    public class CommandSkeleton : Skill
    {
        #region Content
        /*
                [473147] [Actor] p6_necro_commandSkeletons_A
                [473151] [Actor] p6_necro_commandSkeletons_A_buffAttack_02
                [473158] [Actor] p6_necro_commandSkeletons_A_buffAttack_01
                [473214] [Actor] p6_necro_commandSkeletons_A_spawnTrail_emitter
                [473237] [Actor] p6_necro_commandSkeletons_A_attack_01_swipe
                [473243] [Actor] p6_necro_commandSkeletons_A_attack_02_swipe
                [473417] [Actor] p6_necro_commandSkeletons_F
                [473418] [Actor] p6_necro_commandSkeletons_D
                [473420] [Actor] p6_necro_commandSkeletons_B
                [473426] [Actor] p6_necro_commandSkeletons_C
                [473428] [Actor] p6_necro_commandSkeletons_E
                [473474] [Actor] p6_necro_commandSkeletons_B_spawnTrail_emitter
                [473525] [Actor] p6_necro_commandSkeletons_F_spawnTrail_emitter
                [473559] [Actor] p6_necro_commandSkeletons_D_spawnTrail_emitter
                [473606] [Actor] p6_necro_commandSkeletons_C_spawnTrail_emitter
                [473763] [Actor] p6_necro_commandSkeletons_E_spawnTrail_emitter
                [453835] [Actor] p6_necro_commandSkeleton_Base_Melee

                [474750] [EffectGroup] p6_necro_commandSkeletons_B_target_large
                [474751] [EffectGroup] p6_necro_commandSkeletons_B_target_small
                [474755] [EffectGroup] p6_necro_commandSkeletons_C_target_large
                [474756] [EffectGroup] p6_necro_commandSkeletons_D_target_large
                [474758] [EffectGroup] p6_necro_commandSkeletons_E_target_large
                [474759] [EffectGroup] p6_necro_commandSkeletons_F_target_large
                [474760] [EffectGroup] p6_necro_commandSkeletons_C_target_small
                [474761] [EffectGroup] p6_necro_commandSkeletons_D_target_small
                [474762] [EffectGroup] p6_necro_commandSkeletons_E_target_small
                [474763] [EffectGroup] p6_necro_commandSkeletons_F_target_small
                [474767] [EffectGroup] p6_necro_commandSkeletons_B_target_fat
                [474768] [EffectGroup] p6_necro_commandSkeletons_B_target_boss
                [474787] [EffectGroup] p6_necro_commandSkeletons_C_target_fat
                [474788] [EffectGroup] p6_necro_commandSkeletons_D_target_fat
                [474789] [EffectGroup] p6_necro_commandSkeletons_E_target_fat
                [474791] [EffectGroup] p6_necro_commandSkeletons_F_target_fat
                [474792] [EffectGroup] p6_necro_commandSkeletons_C_target_boss
                [474793] [EffectGroup] p6_necro_commandSkeletons_D_target_boss
                [474794] [EffectGroup] p6_necro_commandSkeletons_E_target_boss
                [474795] [EffectGroup] p6_necro_commandSkeletons_F_target_boss
                [471423] [EffectGroup] p6_necro_commandSkeleton_D_AOE
                [473103] [EffectGroup] p6_necro_commandSkeletons_A_buff
                [473199] [EffectGroup] p6_necro_commandSkeletons_A_spawnTrail
                [473308] [EffectGroup] p6_necro_commandSkeletons_A_spawn
                [473309] [EffectGroup] p6_necro_commandSkeletons_runeSwitch_spawn
                [473365] [EffectGroup] p6_necro_commandSkeletons_runeSwitch_spawnTrail
                [473445] [EffectGroup] p6_necro_commandSkeletons_B_spawn
                [473467] [EffectGroup] p6_necro_commandSkeletons_B_spawnTrail
                [473478] [EffectGroup] p6_necro_commandSkeletons_B_buff
                [473482] [EffectGroup] p6_necro_commandSkeletons_runeSwitch_buff
                [473505] [EffectGroup] p6_necro_commandSkeletons_F_buff
                [473512] [EffectGroup] p6_necro_commandSkeletons_F_spawn
                [473523] [EffectGroup] p6_necro_commandSkeletons_F_spawnTrail
                [473537] [EffectGroup] p6_necro_commandSkeletons_D_buff
                [473542] [EffectGroup] p6_necro_commandSkeletons_D_spawn
                [473557] [EffectGroup] p6_necro_commandSkeletons_D_spawnTrail
                [473580] [EffectGroup] p6_necro_commandSkeletons_C_buff
                [473590] [EffectGroup] p6_necro_commandSkeletons_C_spawn
                [473604] [EffectGroup] p6_necro_commandSkeletons_C_spawnTrail
                [473712] [EffectGroup] p6_commandSkeletons_runeSwitch_deathLooks
                [473742] [EffectGroup] p6_necro_commandSkeletons_E_buff
                [473750] [EffectGroup] p6_necro_commandSkeletons_E_spawn
                [473761] [EffectGroup] p6_necro_commandSkeletons_E_spawnTrail
                [473770] [EffectGroup] p6_commandSkeletons_runeSwitch_deathFlash
                [473787] [EffectGroup] p6_necro_commandSkeletons_A_death
                [473802] [EffectGroup] p6_commandSkeletons_runeSwitch_death
                [473803] [EffectGroup] p6_necro_commandSkeletons_B_death
                [473810] [EffectGroup] p6_necro_commandSkeletons_C_death
                [473817] [EffectGroup] p6_necro_commandSkeletons_D_death
                [473832] [EffectGroup] p6_necro_commandSkeletons_E_death
                [473842] [EffectGroup] p6_necro_commandSkeletons_F_death
                [473879] [EffectGroup] p6_necro_commandSkeletons_A_target_meduim
                [473979] [EffectGroup] p6_necro_commandSkeletons_runeSwitch_target
                [473980] [EffectGroup] p6_necro_commandSkeletons_A_topoSwitch_target
                [473981] [EffectGroup] p6_necro_commandSkeletons_B_target_meduim
                [473988] [EffectGroup] p6_necro_commandSkeletons_B_topoSwitch_target
                [473994] [EffectGroup] p6_necro_commandSkeletons_E_target_meduim
                [474003] [EffectGroup] p6_necro_commandSkeletons_E_topoSwitch_target
                [474043] [EffectGroup] p6_necro_commandSkeletons_C_target_meduim
                [474053] [EffectGroup] p6_necro_commandSkeletons_C_topoSwitch_target
                [474140] [EffectGroup] p6_necro_commandSkeletons_F_target_meduim
                [474161] [EffectGroup] p6_necro_commandSkeletons_F_topoSwitch_target
                [474162] [EffectGroup] p6_necro_commandSkeletons_D_target_meduim
                [474171] [EffectGroup] p6_necro_commandSkeletons_D_topoSwitch_target
                [474172] [EffectGroup] p6_necro_commandSkeletons_A_target_small
                [474185] [EffectGroup] p6_necro_commandSkeletons_A_target_large
                [474227] [EffectGroup] p6_necro_commandSkeletons_A_target_fat
                [474243] [EffectGroup] p6_necro_commandSkeletons_A_target_Boss
        //*/
        #endregion
        

        public override IEnumerable<TickTimer> Main()
        {
            User.PlayEffect(Effect.PlayEffectGroup, 466026);
            if (Target != null)
            {
                DamageType damageType = DamageType.Physical;
                bool greaterDamage = false;
                
                /*
                 * Enforcer: Reduces the active Essence cost to 25.
                 * Frenzy: Commanded skeletons go into a frenzy, gaining 25% increased attack speed as long as they attacked the Commanded target (in addition to damage bonus).
                 * Dark Mending: Skeletal minions will heal the Necromancer for 0.5% of total Life per hit while being Commanded (i.e. as long as the skill is activated).
                 * Freezing Grasp: Damage type is changed to Cold, and the target of Command is frozen for 3 seconds.
                 * Kill Command: Damage type changes to Poison, and Command activation will instead make each Skeleton explode, killing them and dealing 215% damage as Poison to enemies within 15 yards each. They will still rush to their target before exploding.
                 */
                bool enforcer = Rune_A > 0,
                     frenzy = Rune_B > 0,
                     darkMending = Rune_C > 0,
                     freezingGrasp = Rune_D > 0,
                     killCommand = Rune_E > 0;
                
                // Enforcer
                if (enforcer)
                {
                    UsePrimaryResource(25);
                }
                else
                {
                    UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));
                    if (darkMending)
                    {
                        ((Player)User).AddPercentageHP(25f); // add per hit : TODO: Life per hit while being Commanded (i.e. as long as the skill is activated).
                    }
                    else if (freezingGrasp)
                    {
                        damageType = DamageType.Cold;
                    }
                    else if (killCommand)
                    {
                        damageType = DamageType.Poison;
                        greaterDamage = true; // TODO: Implement Kill Command to Explode instead of attacking
                    }
                }
                

                foreach (var skeleton in ((Player)User).NecromancerSkeletons)
                {
                    //User.PlayEffectGroup(474172);
                    ActorMover mover = new ActorMover(skeleton);
                    mover.MoveArc(Target.Position, 6, -0.1f, new ACDTranslateArcMessage
                    {
                        //Field3 = 303110, // used for male barb leap, not needed?
                        FlyingAnimationTagID = AnimationSetKeys.Attack.ID,
                        LandingAnimationTagID = -1,
                        Gravity = 0.6f,
                        PowerSNO = PowerSNO
                    });
                    skeleton.Position = Target.Position;
                    skeleton.SetVisible(true);
                    skeleton.Hidden = false;
                    skeleton.PlayEffectGroup(474172);
                    
                    // Commanded skeletons go into a frenzy, gaining 25% increased attack speed as long as they attacked the Commanded target (in addition to damage bonus).
                    if (frenzy)
                    {
                        if (!skeleton.Attributes.FixedMap.Contains(FixedAttribute.AttackSpeed))
                        {
                            var originalAttackSpeed = skeleton.Attributes[GameAttributes.Attacks_Per_Second];
                            skeleton.Attributes.FixedMap.Add(FixedAttribute.AttackSpeed, 
                                attr => attr[GameAttributes.Attacks_Per_Second] = originalAttackSpeed * 1.25f,
                                () => skeleton.Attributes[GameAttributes.Attacks_Per_Second] = originalAttackSpeed);
                            skeleton.Attributes.BroadcastChangedIfRevealed();
                        }
                    }
                    else
                    {
                        if (skeleton.Attributes.FixedMap.Contains(FixedAttribute.AttackSpeed))
                        {
                            skeleton.Attributes.FixedMap.Remove(FixedAttribute.AttackSpeed);
                            skeleton.Attributes.BroadcastChangedIfRevealed();
                        }
                    }
                    AttackPayload attack = new AttackPayload(this)
                    {
                        Target = Target
                    };
                    
                    attack.AddWeaponDamage(greaterDamage ? 2.15f : 1.0f, damageType);
                    attack.OnHit = hit =>
                    {
                        if (freezingGrasp)
                        {
                            if (!HasBuff<DebuffFrozen>(hit.Target))
                            {
                                hit.Target.PlayEffect(Effect.IcyEffect);
                                AddBuff(hit.Target, new DebuffFrozen(WaitSeconds(3.0f)));
                            }
                        }
                    };
                    attack.Apply();


                    // WeaponDamage(Target, 0.50f, DamageType.Physical);
                    //AddBuff(Target, new DebuffStunned(WaitSeconds(0.3f)));
                }
            }
            yield break;
        }
    }
    #endregion
    //Done
    #region RaiseGolem
    
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.CommandGolem)]
    public class RaiseGolem : Skill
    {
        private bool WaitTo(TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }
        private bool WaitToPosition(Actor user, Vector3D TPosition)
        {
            while (user.Position != TPosition)
            {

            }
            return true;
        }
        public override IEnumerable<TickTimer> Main()
        {
            
            //AddBuff(User, new ZBuff());
            //AddBuff(User, new FBuff());
            //AddBuff(User, new SxBuff());
            //AddBuff(User, new SvBuff());
            //AddBuff(User, new TBuff());
            //AddBuff(User, new FrBuff());
            //*/
            var Golem = ((Player) User).ActiveGolem;
            int countofFlesh = 5;
            float cooldown = 5f;
            if (Rune_D > 0)
                countofFlesh = 8;

            float targetDistance = PowerMath.Distance2D(TargetPosition, Golem.Position);
            if (Rune_E > 0)
            {
                (((Player)this.User).ActiveGolem as Minion).Brain.DeActivate();
                ((Player) User).ActiveGolem.PlayActionAnimation(AnimationSno.p6_icegolem_generic_cast);
                var proxy = SpawnProxy(TargetPosition, WaitSeconds(3f));
                proxy.PlayEffectGroup(474839);

                AttackPayload attack = new AttackPayload(this);
                attack.Targets = GetEnemiesInRadius(User.Position, 25f);

                attack.OnHit = hitPayload =>
                {
                    AddBuff(hitPayload.Target, new DebuffFrozen(WaitSeconds(3f)));
                };
                attack.Apply();
                yield return WaitSeconds(1f);
                (((Player) User).ActiveGolem as Minion).Brain.Activate();
            }
            else if (Rune_A > 0)
            {
                cooldown = 0f;
                (Golem as Minion).Brain.DeActivate();
                #region Сам вихрь
                Golem.WalkSpeed *= 2;
                

                //Пыль на голема
                Golem.PlayEffectGroup(475352);
                Golem.Move(TargetPosition, ActorSystem.Movement.MovementHelpers.GetFacingAngle(Golem, TargetPosition));
                yield return WaitSeconds(targetDistance * 0.024f);
                
                //Индикация зоны
                ((Player) User).ActiveGolem.PlayActionAnimation(AnimationSno.p6_bonegolem_active_01);
                var proxy = SpawnProxy(TargetPosition, WaitSeconds(2f));
                //Рывок
                proxy.PlayEffectGroup(466735); //[466735] p6_necro_golem_bone_areaIndicator
                foreach (var plr in Golem.World.Players.Values)
                    Golem.Unreveal(plr);
                Golem.SetVisible(false);
                //Месиво
                var proxy1 = SpawnProxy(TargetPosition, WaitSeconds(3f));
                proxy1.PlayEffectGroup(466557); ////[466557] p6_necro_golem_bone_tornadoStart02 - 0
                Golem.WalkSpeed /= 2;
                AttackPayload attack = new AttackPayload(this);
                attack.Targets = GetEnemiesInRadius(TargetPosition, 15f);

                attack.OnHit = hitPayload =>
                {
                    AddBuff(hitPayload.Target, new DebuffStunned(WaitSeconds(2f)));
                    WeaponDamage(hitPayload.Target, 20.0f, DamageType.Physical);
                };
                attack.Apply();
                yield return WaitSeconds(2f);

                //Вовзрат в нормальное состояние
                var proxy2 = SpawnProxy(Golem.Position, WaitSeconds(2f));
                proxy2.PlayEffectGroup(466618); //[466618] p6_necro_golem_bone_onGolemRootStart - 0
                yield return WaitSeconds(2.5f);
                foreach (var plr in Golem.World.Players.Values)
                    Golem.Reveal(plr);
                Golem.SetVisible(true);
                (Golem as Minion).Brain.Activate();
                #endregion

            }
            else if (Rune_B > 0)
            {
                cooldown = 0f;
                ((Player) User).AddPercentageHP(25f);

                if (User.World.CheckLocationForFlag(TargetPosition, DiIiS_NA.Core.MPQ.FileFormats.Scene.NavCellFlags.AllowWalk))
                {
                    (Golem as Minion).Brain.DeActivate();
                    var baseeffect = SpawnEffect(ActorSno._p6_bloodgolem_b_despawn, Golem.Position, User, WaitSeconds(1f));
                    baseeffect.PlayEffectGroup(463811);
                    Golem.Teleport(TargetPosition);
                    //var effect = SpawnEffect(351638, TargetPosition, User, WaitSeconds(3f));
                    var proxy = SpawnProxy(TargetPosition, WaitSeconds(1f));
                    proxy.PlayEffectGroup(463891);
                    proxy.UpdateDelay = 0.2f;
                    proxy.OnUpdate = () =>
                    {
                        AttackPayload attack = new AttackPayload(this);
                        attack.Targets = GetEnemiesInRadius(User.Position, 13f);

                        attack.AddWeaponDamage(0.9f, DamageType.Physical);
                        attack.OnHit = hitPayload =>
                        {
                        };
                        attack.Apply();
                    };
                    yield return WaitSeconds(2.6f);

                    (Golem as Minion).Brain.Activate();
                }


                //var Explosion = SpawnEffect(475370, Golem.Position, ActorSystem.Movement.MovementHelpers.GetFacingAngle(User, Golem), WaitSeconds(1f));
                //Explosion.PlayEffect(Effect.PlayEffectGroup, RuneSelect(457183, 471539, 471258, 471249, 471247, 471236));
                //Explosion.PlayEffectGroup(463891);
                /*
                [475371] [EffectGroup] p6_necro_bloodGolem_B_golemDeathFX
                [475372] [EffectGroup] p6_necro_bloodGolem_D_golemDeathFX
                [475373] [EffectGroup] p6_necro_bloodGolem_base_golemDeathFX
                [463712] [EffectGroup] p6_necro_bloodGolem_blood_actorToActor
                [463811] [EffectGroup] p6_necro_bloodGolem_blood_golemDeath_attractor
                [463891] [EffectGroup] p6_necro_bloodGolem_blood_ground
                [464193] [EffectGroup] p6_necro_bloodGolem_blood_actorToActor_redVein
                [464541] [EffectGroup] p6_necro_bloodGolem_blood_actorToActor_pickOne
                [465156] [EffectGroup] p6_necro_bloodGolem_blood_actorToActor_blueVein02
                */

            }
            else if (Rune_C > 0)
            {
                var Actors = User.GetActorsInRange(TargetPosition, 11).Where(x => x.SNO == ActorSno._p6_necro_corpse_flesh).Take(5).ToList();

                AddBuff(Golem, new SBuff());

                foreach (var actor in Actors)
                {
                    AddBuff(Golem, new SBuff());
                    actor.Destroy();
                }
            }
            else
            {
                if (Golem != null)
                {
                    (Golem as Minion).Brain.DeActivate();
                    Golem.WalkSpeed *= 2;

                    Golem.Move(TargetPosition, ActorSystem.Movement.MovementHelpers.GetFacingAngle(Golem, TargetPosition));
                    //Время на обычной скорости targetDistance * 0.012

                    var Boom = Task<bool>.Factory.StartNew(() => WaitTo(WaitSeconds(targetDistance * 0.024f)));
                    Boom.ContinueWith(delegate
                    {
                        for (int i = 0; i < countofFlesh; i++)
                            User.World.SpawnMonster(ActorSno._p6_necro_corpse_flesh, RandomDirection(TargetPosition, 3f, 9f));

                        (Golem as Minion).Kill(this);
                        Golem.Destroy();
                        ((Player) User).ActiveGolem = null;


                    });
                }
            }
            StartCooldown(cooldown);
            yield break;
        }
        class PlayerHasGolemBuff : PowerBuff
        {
            public List<Actor> golem;

            public PlayerHasGolemBuff(List<Actor> dogs)
            {
                golem = dogs;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Skill_Toggled_State, SkillsSystem.Skills.Necromancer.ExtraSkills.CommandGolem] = true;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload is DeathPayload)
                {

                }
            }

            public override void Remove()
            {
                base.Remove();
            }
        }

        [ImplementsPowerBuff(0, true)]
        public class ZBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(1, true)]
        public class FBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(2, true)]
        public class SBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 10;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += 0.3f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes.BroadcastChangedIfRevealed();
                    User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += 0.3f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                User.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= StackCount * 0.3f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(3, true)]
        public class TBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(4, true)]
        public class FrBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(5, true)]
        public class SxBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(6, true)]
        public class SvBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(2);
                MaxStackCount = 20;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
    }
    //*/
    #endregion
    //Done
    #region ArmyofDead
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.ArmyofDead)]
    public class DeadArmy : Skill
    {
        #region Content
        /*
                P6_Necro_SkeleBomb,460358
        AotD
                [466460] [Actor] necro_AotD_B_north_emitter
                [466464] [Actor] necro_AotD_B_south_emitter
                [466469] [Actor] necro_AotD_B_west_emitter
                [466472] [Actor] necro_AotD_B_east_emitter
                [466664] [Actor] necro_AotD_B_northWest_emitter
                [466665] [Actor] necro_AotD_B_northEast_emitter
                [466666] [Actor] necro_AotD_B_southWest_emitter
                [466667] [Actor] necro_AotD_B_southEast_emitter
                [471764] [Actor] necro_AotD_A_emitter
                [471848] [Actor] necro_AotD_C_emitter
                [474357] [Actor] necro_AotD_D_emitter
                [474842] [Actor] necro_AotD_F_emitter
                [460322] [Actor] p6_necro_AotD_impact_geo
                [465663] [Actor] p6_necro_AotD_A_audioProj
                [471802] [EffectGroup] p6_necro_aotd_c_skeleAttack
                [472197] [EffectGroup] p6_necro_AotD_e_windup
                [472198] [EffectGroup] p6_necro_AotD_e_spinning
                [474400] [EffectGroup] p6_necro_AotD_C_skeleAttack_left
                [474402] [EffectGroup] p6_necro_AotD_C_skeleAttack_right
                [474435] [EffectGroup] p6_necro_AotD_C_skeleAttack_top
                [474437] [EffectGroup] p6_necro_AotD_C_skeleAttack_bottom
                [474453] [EffectGroup] p6_necro_AotD_C_skeleAttack_bottomLeft
                [474455] [EffectGroup] p6_necro_AotD_C_skeleAttack_bottomRight
                [474464] [EffectGroup] p6_necro_AotD_C_skeleAttack_topRight
                [474466] [EffectGroup] p6_necro_AotD_C_skeleAttack_topLeft
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            StartCooldown(EvalTag(PowerKeys.CooldownTime));
            var skillData = MPQStorage.Data.Assets[SNOGroup.Power][460358].Data;

            var effectSno = ActorSno._necro_aotd_a_emitter;
            float range = 15f;
            float damage = 120.0f;
            var damageType = DamageType.Physical;
            float time = 1.0f;
            //Морозная шняга
            /*
            [466460] [Actor] necro_AotD_B_north_emitter
            [466464] [Actor] necro_AotD_B_south_emitter
            [466469] [Actor] necro_AotD_B_west_emitter
            [466472] [Actor] necro_AotD_B_east_emitter
            [466664] [Actor] necro_AotD_B_northWest_emitter
            [466665] [Actor] necro_AotD_B_northEast_emitter
            [466666] [Actor] necro_AotD_B_southWest_emitter
            [466667] [Actor] necro_AotD_B_southEast_emitter 
            */
            if (Rune_B > 0)
            {
                damageType = DamageType.Cold;
                damage = 5.2f;
                var Angle = ActorSystem.Movement.MovementHelpers.GetFacingAngle(User, TargetPosition);
                var E = SpawnEffect(ActorSno._necro_aotd_b_north_emitter, TargetPosition, Angle);

                E.UpdateDelay = 0.2f;
                E.OnUpdate = () =>
                {
                    AttackPayload attack = new AttackPayload(this);
                    attack.Targets = GetEnemiesInRadius(E.Position, range);

                    attack.AddWeaponDamage(damage, damageType);
                    attack.OnHit = hitPayload =>
                    {

                    };
                    attack.Apply();
                };
            }
            else
            {


                if (Rune_C > 0)
                {
                    effectSno = ActorSno._necro_aotd_c_emitter;
                    range = 20f;
                    damage = 500.0f;
                }
                else if (Rune_E > 0)
                {
                    ((Player) User).AddPercentageHP(-20f);
                    effectSno = ActorSno._necro_aotd_f_emitter;
                    time = 5.0f;
                    damage = 6.2f;
                }
                var Point = SpawnEffect(effectSno, TargetPosition, 0, WaitSeconds(time));
                yield return WaitSeconds(0.7f);

                if (Rune_A > 0) { damage = 140.0f; damageType = DamageType.Poison; }
                foreach (var Tar in Point.GetMonstersInRange(range))
                {
                    if (Rune_C > 0)
                    {
                        int[] Effects = new[] { 47400, 474402, 474435, 474437, 474453, 474455, 474464, 474466 };
                        Tar.PlayEffectGroup(Effects[RandomHelper.Next(0, 7)]);
                        yield return WaitSeconds(0.5f);
                        WeaponDamage(Tar, damage, damageType);

                    }
                    else if (Rune_E > 0)
                    {
                        Point.UpdateDelay = 0.2f;
                        Point.OnUpdate = () =>
                        {
                            AttackPayload attack = new AttackPayload(this)
                            {
                                Targets = GetEnemiesInRadius(Point.Position, range)
                            };

                            attack.AddWeaponDamage(damage, damageType);
                            attack.OnHit = hitPayload =>
                            {
                            };
                            attack.Apply();
                        };
                    }
                    else
                    {
                        if (Rune_D > 0)
                            Knockback(Tar, 5f);
                        WeaponDamage(Tar, damage, damageType);
                    }
                }
            }

            yield break;
        }
    }
    #endregion
    //TODO: Rune_E
    #region Land Of Dead
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.LandOfDead)]
    public class LandOfDead : Skill
    {
        #region Content
        /*
        
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            StartCooldown(EvalTag(PowerKeys.CooldownTime));
            var DataOfSkill = MPQStorage.Data.Assets[SNOGroup.Power][465839].Data;

            AddBuff(User, new ZBuff());
            if (Rune_A > 0)
                AddBuff(User, new ABuff());
            else if (Rune_B > 0) foreach (var enemy in User.GetMonstersInRange(120f))
                AddBuff(enemy, new FBuff());
            else if (Rune_D > 0)
                AddBuff(User, new DBuff());
            else if(Rune_C > 0) //Чумные земли 
                AddBuff(User, new PBuff());
            yield break;
        }
        [ImplementsPowerBuff(0, true)]
        public class ZBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(10);
                MaxStackCount = 1;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Necromancer_Corpse_Free_Casting] = true;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Necromancer_Corpse_Free_Casting] = false;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(1, true)]
        public class FBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(10);
                MaxStackCount = 1;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                //Necromancer_Corpse_Free_Casting
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                if (RandomHelper.Next(1, 100) > 65)
                    AddBuff(Target, new DebuffFrozen(WaitSeconds(2f)));

                WaitSeconds(1f);
                return false;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(2, true)]
        public class ABuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(10);
                MaxStackCount = 1;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Free_Cast_All] = true;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Free_Cast_All] = false;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(3, true)]
        public class DBuff : PowerBuff
        {
            public override void Init()
            {
                Timeout = WaitSeconds(10);
                MaxStackCount = 1;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Hitpoints_On_Kill_Reduction_Percent] -= 0.2f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                base.Stack(buff);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Hitpoints_On_Kill_Reduction_Percent] += 0.2f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
            private void _AddAmp()
            {
            }
        }
        [ImplementsPowerBuff(4)]
        public class PBuff : PowerBuff
        {
            SecondsTickTimer Ticker;
            public override void Init()
            {
                Timeout = WaitSeconds(10);
                Ticker = new SecondsTickTimer(User.World.Game, 1.0f);
                MaxStackCount = 1;
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                
                return true;
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                if (Ticker.TimedOut)
                {
                    foreach (var enemy in User.GetMonstersInRange(120f))
                        WeaponDamage(enemy, 1.0f, DamageType.Poison);
                    Ticker = new SecondsTickTimer(User.World.Game, 1.0f);
                }

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
    }
    #endregion
    //TODO: Rune_C
    #region Decrepify
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.Decrepify)]
    public class Decrepify : Skill
    {
        #region Content
        /*
            [466156] [Actor] p6_necro_decrepify_B_cast_swipe
            [470150] [Actor] p6_necro_decrepify_F_cast_swipe
            [457752] [Actor] p6_necro_decrepify_A_cast_swipe
            
            [455738] [Anim] p6_Necro_Male_HTH_Cast_Decrepify
            [462313] [Anim] p6_Necro_Female_HTH_Cast_Decrepify
            
            
            [472011] [EffectGroup] p6_necro_frailty_A_death
            [476465] [EffectGroup] p6_necro_frailty_B_death
            [476468] [EffectGroup] p6_necro_frailty_C_death
            [476471] [EffectGroup] p6_necro_frailty_E_death
            
            
            [471057] [EffectGroup] p6_necro_frailty_debuff_runeSwitch
            [461659] [EffectGroup] p6_necro_frailty_A_debuff
            [470925] [EffectGroup] p6_necro_frailty_B_debuff
            [470928] [EffectGroup] p6_necro_frailty_C_debuff
            [470932] [EffectGroup] p6_necro_frailty_D_debuff
            [470934] [EffectGroup] p6_necro_frailty_E_debuff
            
            [471124] [EffectGroup] p6_necro_frailty_F_Aura
            [471144] [EffectGroup] p6_necro_frailty_C_AOE
            
            [470715] [EffectGroup] p6_necro_frailty_cast_runeSwitch
            [466333] [EffectGroup] p6_necro_frailty_A_cast
            [473628] [EffectGroup] p6_necro_frailty_C_cast
            [471394] [EffectGroup] p6_necro_frailty_E_cast
            [471806] [EffectGroup] p6_necro_frailty_F_cast
            
            [451491] [Power] P6_Necro_Decrepify
            [471738] [Power] P6_Necro_PassiveManager_Decrepify

            [471244] [EffectGroup] p6_necro_frailty_indi_runeSwitch
            [461651] [EffectGroup] p6_necro_frailty_A_indi
            [470786] [EffectGroup] p6_necro_frailty_B_indi
            [470824] [EffectGroup] p6_necro_frailty_C_indi
            [470838] [EffectGroup] p6_necro_frailty_D_indi
            [470892] [EffectGroup] p6_necro_frailty_E_indi
            
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            var DataOfSkill = MPQStorage.Data.Assets[SNOGroup.Power][451491].Data;
            
            UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));
            User.PlayEffect(Effect.PlayEffectGroup, 466026);
            //457752
            var Explosion = SpawnEffect(ActorSno._p6_necro_decrepify_a_cast_swipe, TargetPosition, 0, WaitSeconds(0.2f));
            Explosion.PlayEffect(Effect.PlayEffectGroup, RuneSelect(470087, 466027, 466107, 470087, 470087, 470147));

            var Targets = GetEnemiesInRadius(TargetPosition, 20f);
            if (Rune_B > 0)
                AddBuff(User, new DecrippySpeedBuff(Targets.Actors.Count));

            foreach (var Target in Targets.Actors)
            {
                //Target.PlayEffect(Effect.PlayEffectGroup, 466040);
                if (Rune_E > 0)
                {
                    AddBuff(Target, new DecrippyStunBuff());
                    AddBuff(Target, new DecrippyBuff());
                }
                else if (Rune_A > 0)
                    AddBuff(Target, new DecrippyMaxBuff());
                else if (Rune_D > 0)
                    AddBuff(Target, new DecrippyDmgBuff());
                else
                    AddBuff(Target, new DecrippyBuff());

            }

            yield break;
        }

        [ImplementsPowerBuff(0)]
        public class DecrippyBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            float PercentageSlow = 0.75f;
            float PercentageDamage = 0.3f;
            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                Target.WalkSpeed *= (1f - PercentageSlow);
                Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] += PercentageSlow;
                Target.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= PercentageDamage;
                Target.Attributes.BroadcastChangedIfRevealed();
                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                Target.WalkSpeed /= (1f - PercentageSlow);
                Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= PercentageSlow;
                Target.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += PercentageDamage;
                Target.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(1)]
        public class DecrippyStunBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            float PercentageSlow = 0.75f;
            float PercentageDamage = 0.3f;
            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(1)]//0
        public class DecrippyMaxBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            float PercentageSlow = 0.75f;
            float PercentageMax = 0.99f;
            float PercentageDamage = 0.3f;
            int Count = 0;
            SecondsTickTimer Ticker;
            public override void Init()
            {
                Timeout = WaitSeconds(30f);
                Ticker = new SecondsTickTimer(User.World.Game, 1.0f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                Target.WalkSpeed *= (1f - PercentageMax);
                Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] += PercentageMax;
                Target.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= PercentageDamage;
                Target.Attributes.BroadcastChangedIfRevealed();
                return true;
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;
                if (Count < 5)
                    if (Ticker.TimedOut)
                    {
                        //Target.WalkSpeed *= (1f - PercentageSlow);
                        Target.WalkSpeed /= (1f - (PercentageMax - (0.05f * Count)));
                        Count++;
                        Target.WalkSpeed *= (1f - (PercentageMax - (0.05f * Count)));
                        Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= 0.05f;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        Ticker = new SecondsTickTimer(User.World.Game, 1.0f);
                    }
                return false;
            }

            public override void Remove()
            {
                Target.WalkSpeed /= (1f - PercentageSlow);
                Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= PercentageSlow;
                Target.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += PercentageDamage;
                Target.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class DecrippyDmgBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            float PercentageSlow = 0.75f;
            float PercentageDamage = 0.4f;
            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                Target.Attributes[GameAttributes.Damage_Weapon_Percent_Total] -= PercentageDamage;
                Target.Attributes.BroadcastChangedIfRevealed();
                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                Target.Attributes[GameAttributes.Damage_Weapon_Percent_Total] += PercentageDamage;
                Target.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(2)]
        public class DecrippySpeedBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            float PercentageSlow = 0.03f;
            float PercentageDamage = 0.4f;
            int Count = 0;

            public DecrippySpeedBuff(int C)
            {
                if (C > 10)
                    Count = 10;
                else
                    Count = C;
            }
            public override void Init()
            {
                Timeout = WaitSeconds(5f);

            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                Target.WalkSpeed /= (1f + PercentageSlow);
                Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= PercentageSlow;
                Target.Attributes.BroadcastChangedIfRevealed();
                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                Target.WalkSpeed /= (1f + PercentageSlow);
                Target.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] += PercentageSlow;
                Target.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }

    }
    #endregion
    //Done
    #region Leech
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.Leech)]
    public class Leech : Skill
    {
        public override IEnumerable<TickTimer> Main()
        {
            UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));

            if (Rune_D > 0) //Проклятая земля
            {
                foreach (var alr in World.GetActorsBySNO(ActorSno._p6_necro_leech_e_proxyactor))
                    if (alr.Attributes[GameAttributes.Summoner_ID] == ((Player) User).PlayerIndex)
                        alr.Destroy();

                var proxy = SpawnEffect(ActorSno._p6_necro_leech_e_proxyactor, TargetPosition,
                ActorSystem.Movement.MovementHelpers.GetFacingAngle(User, TargetPosition),
                WaitSeconds(30f));
                proxy.Attributes[GameAttributes.Summoner_ID] = ((Player) User).PlayerIndex;
                AddBuff(User, new Rune_DBuff(proxy));
            }
            else
            {
                var Explosion = SpawnEffect(
                    RuneSelect(
                        ActorSno._p6_necro_leech_base_groundarea,
                        ActorSno._p6_necro_leech_b_groundarea,
                        ActorSno._p6_necro_leech_c_groundarea,
                        ActorSno._p6_necro_leech_d_groundarea,
                        ActorSno._p6_necro_leech_e_groundarea,
                        ActorSno._p6_necro_leech_f_groundarea
                    ),
                    TargetPosition,
                    ActorSystem.Movement.MovementHelpers.GetFacingAngle(User, TargetPosition),
                    WaitSeconds(0.2f)
                );

                Explosion.PlayEffect(Effect.PlayEffectGroup, RuneSelect(473263, 475194, 473264, 475171, 473361, 473432));

                foreach (var enemy in Explosion.GetMonstersInRange(20f))
                {
                    if (Rune_B > 0)
                        AddBuff(enemy, new Rune_B_Buff());
                    else if (Rune_A > 0)
                    {
                        AddBuff(enemy, new Base_Buff());
                        AddBuff(User, new Rune_A_Buff());
                    }
                    else if (Rune_E > 0)
                        AddBuff(enemy, new Rune_E_Buff());
                    else if (Rune_C > 0)
                        AddBuff(enemy, new Rune_C_Buff());
                    else
                        AddBuff(enemy, new Base_Buff());
                }

            }

            yield break;
        }

        [ImplementsPowerBuff(0)]
        public class Base_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            SecondsTickTimer Ticker;
            
            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    payload.Context.User.AddPercentHP(2);
                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(4)]
        public class Rune_B_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            SecondsTickTimer Ticker;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    payload.Context.User.AddPercentHP(2);
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 100)
                    {
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();

                        var newms = payload.Target.GetMonstersInRange(40f);

                        if (newms.Count > 0)
                            AddBuff(newms.OrderBy(x => Guid.NewGuid()).Take(1).Single(), new Rune_B_Buff());
                    }
                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(1, true)]
        public class Rune_A_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            SecondsTickTimer Ticker;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
                MaxStackCount = 20;
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                User.Attributes[GameAttributes.Hitpoints_Regen_Per_Second_Bonus] += 751f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Hitpoints_Regen_Per_Second_Bonus] += 751f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }

            public override void Remove()
            {
                User.Attributes[GameAttributes.Hitpoints_Regen_Per_Second_Bonus] -= StackCount * 751f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class Rune_E_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            SecondsTickTimer Ticker;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    payload.Context.User.AddPercentHP(2);
                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(2)]
        public class Rune_DBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            SecondsTickTimer Ticker;
            Actor Obj = null;

            public Rune_DBuff(Actor O)
            {
                Obj = O;
            }

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
                Ticker = new SecondsTickTimer(User.World.Game, 1.0f);
                
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                if (Ticker.TimedOut)
                {
                    ((Player) User).AddPercentageHP(Obj.GetMonstersInRange(20f).Count);
                    Ticker = new SecondsTickTimer(User.World.Game, 1.0f);
                }

                return false;
            }

            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class Rune_C_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;
            SecondsTickTimer Ticker;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    payload.Context.User.AddPercentHP(2);
                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();
            }
        }
    }
    #endregion
    //Done 
    #region Frailty
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.Frailty)]
    public class Fragile : Skill
    {
        #region Content
        /*
            [470810] [Actor] p6_necro_frailty_B_indi_runeLight_actor
            [470834] [Actor] p6_necro_frailty_C_indi_runeLight_actor
            [470845] [Actor] p6_necro_frailty_D_indi_runeLight_actor
            [470906] [Actor] p6_necro_frailty_E_indi_runeLight_actor
            [470909] [Actor] p6_necro_frailty_E_indi_energyDownward_actor
            [465439] [Actor] p6_necro_frailty_A_indi_energyDownward_actor
            [465761] [Actor] p6_necro_frailty_A_indi_runeLight_actor
            [471792] [AmbientSound] P6_Skill_Necro_Frailty_Rune_Aura
            [466333] [EffectGroup] p6_necro_frailty_A_cast
            [476465] [EffectGroup] p6_necro_frailty_B_death
            [476468] [EffectGroup] p6_necro_frailty_C_death
            [476471] [EffectGroup] p6_necro_frailty_E_death
            [461659] [EffectGroup] p6_necro_frailty_A_debuff
            [470715] [EffectGroup] p6_necro_frailty_cast_runeSwitch
            [470925] [EffectGroup] p6_necro_frailty_B_debuff
            [470928] [EffectGroup] p6_necro_frailty_C_debuff
            [470932] [EffectGroup] p6_necro_frailty_D_debuff
            [470934] [EffectGroup] p6_necro_frailty_E_debuff
            [471057] [EffectGroup] p6_necro_frailty_debuff_runeSwitch
            [471124] [EffectGroup] p6_necro_frailty_F_Aura
            [471144] [EffectGroup] p6_necro_frailty_C_AOE
            [471394] [EffectGroup] p6_necro_frailty_E_cast
            [471806] [EffectGroup] p6_necro_frailty_F_cast
            [472011] [EffectGroup] p6_necro_frailty_A_death
            [473628] [EffectGroup] p6_necro_frailty_C_cast
            [471845] [Power] P6_Necro_PassiveManager_Frailty
            [473992] [Power] P6_Necro_Frailty_Aura
            [460870] [Power] P6_Necro_Frailty

            [471244] [EffectGroup] p6_necro_frailty_indi_runeSwitch
            [461651] [EffectGroup] p6_necro_frailty_A_indi
            [470786] [EffectGroup] p6_necro_frailty_B_indi
            [470824] [EffectGroup] p6_necro_frailty_C_indi
            [470838] [EffectGroup] p6_necro_frailty_D_indi
            [470892] [EffectGroup] p6_necro_frailty_E_indi
            
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            if (Rune_A > 0)
                User.AddPercentHP(-10);
            else
                UsePrimaryResource(EvalTag(PowerKeys.ResourceCost));

            var proxy = SpawnProxy(TargetPosition, WaitSeconds(0.2f));
            proxy.PlayEffect(Effect.PlayEffectGroup, RuneSelect(471244, 461651, 470786, 470824, 470838, 470892));

            var Targets = GetEnemiesInRadius(TargetPosition, 20f);
            foreach (var Target in Targets.Actors)
            {
                if (Rune_B > 0) //Сбор эссенции //Бафф 4
                    AddBuff(Target, new Rune_B_Buff());
                else if (Rune_A > 0)
                    AddBuff(Target, new Rune_A_Buff());
                else if (Rune_C > 0)
                    AddBuff(Target, new Rune_C_Buff());
                else if (Rune_D > 0)
                    AddBuff(Target, new Rune_D_Buff());
                else
                    AddBuff(Target, new FrailtyBuff());
            }
            yield break;
        }
        [ImplementsPowerBuff(0)]
        public class FrailtyBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 100 * 15)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    } 
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(4)]
        public class Rune_B_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 100 * 15)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        Remove();
                        GeneratePrimaryResource(2);
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class Rune_A_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 100 * 18)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class Rune_C_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 100 * 15)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        Target.PlayEffectGroup(471144);
                        foreach (var monster in Target.GetMonstersInRange(10f))
                            WeaponDamage(monster, 1.0f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class Rune_D_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(30f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 100 * 15)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }


        [ImplementsPowerBuff(1)]
        public class AA_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 10)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(2)]
        public class AB_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 10)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(3)]
        public class AC_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 10)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(4)]
        public class AD_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 10)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(5)]
        public class AE_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 10)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
        [ImplementsPowerBuff(6)]
        public class AF_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                return true;
            }
            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)

                {
                    if (Target.Attributes[GameAttributes.Hitpoints_Cur] <= Target.Attributes[GameAttributes.Hitpoints_Max_Total] / 10)
                    {
                        Target.Attributes[GameAttributes.Hitpoints_Cur] = 0;
                        Target.Attributes.BroadcastChangedIfRevealed();
                        //WeaponDamage(Target, 0.50f, DamageType.Physical);
                        Remove();
                    }
                }
            }
            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }
            public override void Remove()
            {
                base.Remove();
            }
        }
    }
    #endregion
    // TODO: Effects, Func: Done
    #region BoneArmor
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.BoneArmor)]
    public class ArmorBone : Skill
    {
        #region Content
        /*
                [466857] [Power] P6_Necro_BoneArmor
                [135701] [Power] HoodedNightmare_BoneArmor
                
                [474132] [EffectGroup] P6_Necro_BoneArmor_A
                [474133] [EffectGroup] P6_Necro_BoneArmor_B
                [474134] [EffectGroup] P6_Necro_BoneArmor_C
                [474137] [EffectGroup] P6_Necro_BoneArmor_D
                [474138] [EffectGroup] P6_Necro_BoneArmor_E
                [474139] [EffectGroup] P6_Necro_BoneArmor_F
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            if (Rune_B > 0)
                StartCooldown(45f);
            else
                StartCooldown(EvalTag(PowerKeys.CooldownTime));
            if (Rune_D > 0)
                ((Player) User).AddPercentageHP(-20f);

            int Count = 0;
            
            foreach (var Target in User.GetMonstersInRange(20f))
            {
                if (Count >= 10) break;

                if (Rune_C > 0)
                    AddBuff(Target, new DebuffStunned(WaitSeconds(2f)));
                else if (Rune_E > 0)
                    WeaponDamage(Target, 1.25f, DamageType.Cold);
                else if (Rune_A > 0)
                    WeaponDamage(Target, 1.45f, DamageType.Physical);
                else
                    WeaponDamage(Target, 1.25f, DamageType.Physical);
                Count++;
            }
            if (Rune_B > 0)
                AddBuff(User, new Rune_B_Buff());
            else
                for (int i = 0; i < Count; i++)
                {
                    AddBuff(User, new BoneArmorNBuff());
                    if (Rune_D > 0)
                        AddBuff(User, new Rune_D_Buff());
                    else if (Rune_E > 0)
                        AddBuff(User, new Rune_E_Buff());
                }
            AddBuff(User, new Rune_E_Buff());
            yield break;
        }

        [ImplementsPowerBuff(0,true)]
        public class BoneArmorNBuff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(10f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Melee] += 0.03f;
                User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Ranged] += 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Melee] += 0.03f;
                    User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Ranged] += 0.03f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Melee] -= StackCount * 0.03f;
                User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Ranged] -= StackCount * 0.03f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0)]
        public class Rune_B_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(5f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                User.Attributes[GameAttributes.God] = true;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override void Remove()
            {
                User.Attributes[GameAttributes.God] = false;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(7, true)]
        public class Rune_D_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(10f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                User.Attributes[GameAttributes.Hitpoints_Regen_Per_Second_Bonus] += 0.1f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Hitpoints_Regen_Per_Second_Bonus] += 0.1f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Hitpoints_Regen_Per_Second_Bonus] -= StackCount * 0.1f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }
        [ImplementsPowerBuff(0, true)]
        public class Rune_E_Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(10f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                User.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= 0.01f;
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override bool Stack(Buff buff)
            {
                bool stacked = StackCount < MaxStackCount;

                if (stacked)
                {
                    base.Stack(buff);
                    User.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] -= 0.01f;
                    User.Attributes.BroadcastChangedIfRevealed();
                }
                return true;
            }
            public override void Remove()
            {
                User.Attributes[GameAttributes.Movement_Scalar_Reduction_Percent] += StackCount * 0.01f;
                User.Attributes.BroadcastChangedIfRevealed();
                base.Remove();
            }
        }

        [ImplementsPowerBuff(5)]
        public class BoneArmor5Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(10f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Melee] += ScriptFormula(10);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();

            }
        }
        [ImplementsPowerBuff(6)]
        public class BoneArmor6Buff : PowerBuff
        {
            const float _damageRate = 1f;
            TickTimer _damageTimer = null;

            public override void Init()
            {
                Timeout = WaitSeconds(10f);
            }

            public override bool Apply()
            {
                if (!base.Apply())
                    return false;

                User.Attributes[GameAttributes.Damage_Percent_Reduction_From_Melee] += ScriptFormula(10);
                User.Attributes.BroadcastChangedIfRevealed();

                return true;
            }

            public override void OnPayload(Payload payload)
            {
                if (payload.Target == Target && payload is HitPayload)
                {

                }
            }

            public override bool Update()
            {
                if (base.Update())
                    return true;

                return false;
            }

            public override void Remove()
            {
                base.Remove();

            }
        }

        
    }
    #endregion
    // 
    #region BoneSpirit
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.BoneSpirit)]
    public class BoneSpirit : Skill
    {
        #region Content
        /*
            [466407] [Actor] p6_necro_boneSpirit_none_actor
            [468486] [Actor] p6_necro_boneSpirit_none_impact_export
            [469058] [Actor] p6_necro_boneSpirit_none_impact_skull_actor
            [471952] [Actor] P6_Necro_BoneSpirit_E_Death
            [473014] [Actor] p6_necro_boneSpirit_A_impact_distortion

            [473056] [Actor] p6_necro_boneSpirit_C_actor
            [473177] [Actor] p6_necro_boneSpirit_C_impact_skull_actor
            [473641] [Actor] p6_necro_boneSpirit_B_actor
            [473693] [Actor] p6_necro_boneSpirit_B_impact_skull_actor
            [473707] [Actor] p6_necro_boneSpirit_E_actor
            [473945] [Actor] p6_necro_boneSpirit_E_impact_skull_actor
            [473947] [Actor] p6_necro_boneSpirit_E_impact_distortion
            [474064] [Actor] p6_necro_boneSpirit_F_actor
            [474187] [Actor] p6_necro_boneSpirit_D_actor
            [474212] [Actor] p6_necro_boneSpirit_D_impact_skull_actor
            [475943] [Actor] p6_necro_boneSpirit_F_impact_skull_actor
            [466410] [Anim] p6_necro_boneSpirit_none_projectile_export_idle_0
            [467302] [Anim] p6_Necro_Male_HTH_Cast_BoneSpirit
            [468745] [Anim] p6_necro_boneSpirit_none_projectile_export_idle_01_intro
            [469060] [Anim] p6_necro_boneSpirit_none_impact_skull_export_idle_01
            [471291] [Anim] p6_Necro_Female_HTH_Cast_BoneSpirit
            [466409] [AnimSet] p6_necro_boneSpirit_none_projectile
            [469059] [AnimSet] p6_necro_boneSpirit_none_impact_skull_actor
            [473648] [AnimSet] p6_necro_boneSpirit_B_projectile
            [467680] [EffectGroup] p6_necro_boneSpirit_A_impact
            [469601] [EffectGroup] p6_necro_boneSpirit_cast_runeSwitch
            [469604] [EffectGroup] p6_necro_boneSpirit_A_cast
            [470213] [EffectGroup] p6_necro_boneSpirit_A_focus
            [470214] [EffectGroup] p6_necro_boneSpirit_A_target
            [472574] [EffectGroup] p6_necro_boneSpirit_lockedOnSFX

            [473025] [EffectGroup] p6_necro_boneSpirit_B_impact
            [473026] [EffectGroup] p6_necro_boneSpirit_C_impact
            [473027] [EffectGroup] p6_necro_boneSpirit_D_impact
            [473028] [EffectGroup] p6_necro_boneSpirit_E_impact
            [473029] [EffectGroup] p6_necro_boneSpirit_F_impact

            [473031] [EffectGroup] p6_necro_boneSpirit_B_focus
            [473032] [EffectGroup] p6_necro_boneSpirit_C_focus
            [473033] [EffectGroup] p6_necro_boneSpirit_D_focus
            [473034] [EffectGroup] p6_necro_boneSpirit_E_focus
            [473035] [EffectGroup] p6_necro_boneSpirit_F_focus
            [473041] [EffectGroup] p6_necro_boneSpirit_B_cast
            [473042] [EffectGroup] p6_necro_boneSpirit_C_cast
            [473043] [EffectGroup] p6_necro_boneSpirit_D_cast
            [473044] [EffectGroup] p6_necro_boneSpirit_E_cast
            [473045] [EffectGroup] p6_necro_boneSpirit_F_cast
            [473047] [EffectGroup] p6_necro_boneSpirit_target_runeSwitch
            [473048] [EffectGroup] p6_necro_boneSpirit_B_target
            [473050] [EffectGroup] p6_necro_boneSpirit_C_target
            [473051] [EffectGroup] p6_necro_boneSpirit_D_target
            [473052] [EffectGroup] p6_necro_boneSpirit_E_target
            [473053] [EffectGroup] p6_necro_boneSpirit_F_target
            [473093] [EffectGroup] p6_necro_boneSpirit_C_aoe
            [473768] [EffectGroup] p6_necro_boneSpirit_E_aoe
            [464896] [Power] p6_Necro_BoneSpirit
            [464999] [Power] P6_Necro_BoneSpirit_Passive
            [473695] [Rope] p6_necro_boneSpirit_B_focus_ropeMain
            [473696] [Rope] p6_necro_boneSpirit_B_focus_ropeBrightEnd
            [475947] [Rope] p6_necro_boneSpirit_F_focus_ropeMain
            [475948] [Rope] p6_necro_boneSpirit_F_focus_ropeBrightEnd
            [470476] [Rope] p6_necro_boneSpirit_A_focus_ropeMain
            [473961] [Rope] p6_necro_boneSpirit_E_focus_ropeMain
            [473962] [Rope] p6_necro_boneSpirit_E_focus_ropeBrightEnd
            [472946] [Rope] p6_necro_boneSpirit_A_focus_ropeBrightEnd
            [474224] [Rope] p6_necro_boneSpirit_D_focus_ropeMain
            [474225] [Rope] p6_necro_boneSpirit_D_focus_ropeBrightEnd
            [473226] [Rope] p6_necro_boneSpirit_C_focus_ropeMain
            [473228] [Rope] p6_necro_boneSpirit_C_focus_ropeBrightEnd
        //*/
        #endregion
        public override IEnumerable<TickTimer> Main()
        {
            //[466994] [Actor] p6_necro_boneSpirit_A_projectile
            //[473020] [Actor] p6_necro_boneSpirit_B_projectile
            //[473021] [Actor] p6_necro_boneSpirit_C_projectile
            //[473022] [Actor] p6_necro_boneSpirit_D_projectile
            //[473023] [Actor] p6_necro_boneSpirit_E_projectile
            //[473024] [Actor] p6_necro_boneSpirit_F_projectile
            User.Attributes[GameAttributes.Skill_Charges, PowerSNO] -= 1;
            User.Attributes.BroadcastChangedIfRevealed();
            var projectile = new Projectile(
                this,
                RuneSelect(
                    ActorSno._p6_necro_bonespirit_a_projectile,
                    ActorSno._p6_necro_bonespirit_b_projectile,
                    ActorSno._p6_necro_bonespirit_c_projectile,
                    ActorSno._p6_necro_bonespirit_d_projectile,
                    ActorSno._p6_necro_bonespirit_e_projectile,
                    ActorSno._p6_necro_bonespirit_f_projectile
                ),
                User.Position
            );
            projectile.Position.Z += 5f;  // fix height
            DamageType NowDamage = DamageType.Physical;
            int countdamagebonus = 0;
            bool Founded = false;
            
            projectile.OnCollision = (hit) =>
            {

                countdamagebonus++;

                if (Rune_E > 0)
                    WeaponDamage(hit, 40f * (countdamagebonus * (1.0f + (0.15f * countdamagebonus))), DamageType.Cold);
                else if (Rune_B > 0)
                {
                    WeaponDamage(hit, 40f, DamageType.Poison);
                    projectile.PlayEffectGroup(473093);
                    foreach (var mnstr in projectile.GetMonstersInRange(12f))
                        AddBuff(mnstr, new DebuffFeared(WaitSeconds(2f)));
                    projectile.Destroy();
                }
                else if (Rune_D > 0)
                {
                    projectile.PlayEffectGroup(473768);
                    foreach (var mnstr in projectile.GetMonstersInRange(10f))
                        WeaponDamage(hit, 12.5f, DamageType.Cold);
                    projectile.Destroy();
                }
                else if (Rune_A > 0)
                {
                    //TODO:CHARM
                    //AddBuff(hit, );
                    projectile.Destroy();
                }
                else
                    WeaponDamage(hit, 40f, NowDamage);
                
            };

            
            projectile.Launch(TargetPosition, 1f);

            projectile.OnUpdate = () =>
            {
                if (!Founded)
                    if (projectile.GetMonstersInRange(15f).Count > 0)
                    {
                        Founded = true;
                        var Target = projectile.GetMonstersInRange(25f).OrderBy(x => Guid.NewGuid()).Take(1).Single();
                        projectile.Launch(Target.Position, 1f);
                    }
            };
            yield break;
        }

        [ImplementsPowerBuff(2)]
        public class SpiritCountBuff : PowerBuff
        {
            public bool CoolDownStarted = false;
            public uint Max = 3;

            public override bool Update()
            {
                if (base.Update())
                    return true;

                if (((Player) User).SkillSet.HasSkillWithRune(464896, 2))
                    Max = 4;
                else
                {
                    Max = 3;
                    if (User.Attributes[GameAttributes.Skill_Charges, PowerSNO] == 4)
                        User.Attributes[GameAttributes.Skill_Charges, PowerSNO] = 3;
                }

                if (User.Attributes[GameAttributes.Skill_Charges, PowerSNO] < Max)
                {
                    if (!CoolDownStarted)
                    {
                        StartCooldownCharges(15f); CoolDownStarted = true;

                        Task.Delay(15100).ContinueWith(delegate
                        {
                            CoolDownStarted = false;
                            User.Attributes[GameAttributes.Skill_Charges, PowerSNO] = (int)Math.Min(User.Attributes[GameAttributes.Skill_Charges, PowerSNO] + 1, Max);
                            User.Attributes.BroadcastChangedIfRevealed();
                        });
                    }
                }

                return false;
            }
        }
    }
    #endregion

    //
    #region BloodRush
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.BloodRush)]
    public class BloodRush : Skill
    {

        public override IEnumerable<TickTimer> Main()
        {
            ActorMover mover = new ActorMover(User);
            /*while (targetDistance > 15f)
           {
               var EquPos = TargetPosition - User.Position;
               FinalPosition.X = User.Position.X += EquPos.X / 10;
               FinalPosition.X = User.Position.Y += EquPos.Y / 10;
           }*/
            /*
            mover.MoveArc(TargetPosition, 3, -0.1f, new ACDTranslateArcMessage
            {
                //Field3 = 303110, // used for male barb leap, not needed?
                FlyingAnimationTagID = 474708,
                LandingAnimationTagID = -1,
                PowerSNO = PowerSNO
            });
           //*/

            if (!User.World.CheckLocationForFlag(TargetPosition, DiIiS_NA.Core.MPQ.FileFormats.Scene.NavCellFlags.AllowWalk))
            {
                //Logger.Info("Tried to Teleport to unwalkable location");
            }
            else
            {
                var StartTP = SpawnProxy(User.Position, new TickTimer(User.World.Game, 500));
                var PointTP = SpawnProxy(TargetPosition, new TickTimer(User.World.Game, 500));

                foreach (var plr in User.World.Players.Values)
                {
                    //473 637
                    plr.InGameClient.SendMessage(new ACDTranslateSyncMessage()
                    {
                        ActorId = User.DynamicID(plr),
                        Position = User.Position,
                        Snap = true,
                        Field3 = 0xE56D
                    });
                    plr.InGameClient.SendMessage(new MessageSystem.Message.Definitions.Actor.ActorLookOverrideChangedMessage()
                    {
                        Field0 = (int)User.DynamicID(plr),
                        Field1 = -1,
                        Field2 = unchecked((int)0xD8A4C675)
                    });

                    ((Player) User).InGameClient.SendMessage(new ACDTranslateSnappedMessage()
                    {
                        ActorId = (int)User.DynamicID((Player) User),
                        Position = PointTP.Position,
                        Angle = ActorSystem.Movement.MovementHelpers.GetFacingAngle(User, PointTP),
                        Field3 = true,
                        Field4 = 0x90,
                        CameraSmoothingTime = 0x30,
                        Field6 = 0x100
                    });
                    //*/
                    User.Position = PointTP.Position;

                    plr.InGameClient.SendMessage(new ACDTranslateSyncMessage()
                    {
                        ActorId = User.DynamicID(plr),
                        Position = User.Position,
                        Snap = true,
                        Field3 = 0xE56D
                    });

                    plr.InGameClient.SendMessage(new ComplexEffectAddMessage()
                    {
                        EffectId = 0x771B0000,
                        Type = 0,
                        EffectSNO = 0x00073E50,
                        SourceActorId = (int)PointTP.DynamicID(plr),
                        TargetActorId = (int)User.DynamicID(plr),
                        Param1 = 0,
                        Param2 = 0

                    });
                    plr.InGameClient.SendMessage(new PlayEffectMessage()
                    {
                        ActorId = PointTP.DynamicID(plr),
                        Effect = Effect.PlayEffectGroup,
                        OptionalParameter = 456109
                    });
                    plr.InGameClient.SendMessage(new ComplexEffectAddMessage()
                    {
                        EffectId = 0x771B0000,
                        Type = 0,
                        EffectSNO = 0x00073E50,
                        SourceActorId = (int)PointTP.DynamicID(plr),
                        TargetActorId = (int)User.DynamicID(plr),
                        Param1 = 0,
                        Param2 = 0

                    });
                    plr.InGameClient.SendMessage(new EffectGroupACDToACDMessage()
                    {
                        EffectSNOId = 0x00073E50,
                        ActorID = StartTP.DynamicID(plr),
                        TargetID = PointTP.DynamicID(plr)
                    });
                }

                //SpawnProxy(User.Position).PlayEffectGroup(RuneSelect(170231, 205685, 205684, 191913, 192074, 192151));  // alt cast efg: 170231
                /*
                User.PlayEffect(Effect.PlayEffectGroup, 456109);
                //var Explosion = SpawnEffect(473654, User.Position, 0, WaitSeconds(0.5f));

                var FinalPosition = TargetPosition;
                float targetDistance = PowerMath.Distance2D(FinalPosition, this.User.Position);
                User.Teleport(TargetPosition);


                var Explosion1 = SpawnEffect(473654, User.Position, 0, WaitSeconds(0.5f));
                //*/
                //MDZ says this might work just as 191849.
                //User.PlayEffectGroup(RuneSelect(170232, 170232, 170232, 192053, 192080, 192152));
            }

            // wait for landing
            //while (!mover.Update())
            //    yield return WaitTicks(1);
            User.PlayEffect(Effect.PlayEffectGroup, 472922);
            yield break;
        }
        [ImplementsPowerBuff(5)]
        class TeleRevertBuff : PowerBuff
        {
            public Vector3D OrigSpot;
            public Actor OrigTele;

            public TeleRevertBuff(Vector3D OrigSpot, Actor OrigTele)
            {
                this.OrigSpot = OrigSpot;
                this.OrigTele = OrigTele;
            }

            public override void Init()
            {
                Timeout = WaitSeconds(ScriptFormula(1));
            }
            public override bool Apply()
            {
                if (!base.Apply())
                    return false;
                return true;
            }
            public override void Remove()
            {
                Timeout.Stop();
                //OrigTele.Destroy();  --   Removes the voidzone effect as though, but also throws an exception. 
                //                          Perhaps one cannot Destroy() a proxy actor, 
                //                          but is there any other way to quit the effectgroup early?
                base.Remove();
                StartCooldown(WaitSeconds(ScriptFormula(20)));
            }
        }
    }
    #endregion

    // TODO: implementation of the power Simalacrum
    #region Simulacrum
    [ImplementsPowerSNO(SkillsSystem.Skills.Necromancer.ExtraSkills.Simulacrum)]
    public class Simulacrum : Skill
    {
        public override IEnumerable<TickTimer> Main()
        {

            yield break;
        }
    }
    #endregion
}
