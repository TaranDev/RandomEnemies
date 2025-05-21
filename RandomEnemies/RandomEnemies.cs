using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EmotesAPI;
using System;
using RoR2.Audio;
using EntityStates;
using System.Collections.Generic;
using RoR2.Projectile;
using System.Collections;
using System.ComponentModel;
using R2API;
using System.Net;
using UnityEngine.UI;
using RoR2.DirectionalSearch;
using R2API.Utils;
using System.Linq;
using static RoR2.DirectorPlacementRule;
using EntityStates.Croco;
using UnityEngine.Networking;
using Newtonsoft.Json.Utilities;
using static RoR2.SpawnCard;
using Rewired;
using UnityEngine.UIElements;
using RoR2.CharacterAI;
using IL.RoR2.UI.LogBook;
using static Newtonsoft.Json.Converters.DiscriminatedUnionConverter;
using static RandomEnemies.RandomEnemies;
using MonoMod.Cil;
using UnityEngine.ResourceManagement.AsyncOperations;
using static RoR2.BlurOptimized;
using EntityStates.NewtMonster;
using UnityEngine.Events;
using System.Threading.Tasks;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace RandomEnemies
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class RandomEnemies : BaseUnityPlugin
    {

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "TaranDev";
        public const string PluginName = "RandomEnemies";
        public const string PluginVersion = "1.0.0";

        System.Random rnd;

        public static ConfigFile config;

        public static List<MasterPrefabConfig> masterPrefabEnemyConfigEntries;
        public static List<CharacterMaster> enabledEnemyMasters;

        public static List<MasterPrefabConfig> masterPrefabBossConfigEntries;
        public static List<CharacterMaster> enabledBossMasters;

        //public static List<MasterPrefabConfig> masterPrefabFinalBossConfigEntries;
        public static List<CharacterMaster> finalBossMasters;

        public static List<EquipmentDefConfig> equipmentDefConfigEntries;
        public static List<EquipmentDef> enabledEquipmentDefs;

        public static List<string> bossNames = new List<string>() { "titan", "titangold", "superroboballboss", "roboballboss", "minivoidraidcrab", "megaconstruct", "majorconstruct", "magmaworm", "impboss", "gravekeeper", "grandparent", "electricworm", "brother", "beetlequeen", "vagrant", "scav", "voidraidcrab", "falsesonboss", "clayboss", "voidmegacrab"};

        public static List<string> finalBossNames = new List<string>() { "minivoidraidcrab", "brother", "voidraidcrab", "falsesonboss"};

        public static List<string> prefabBlacklist = new List<string>() { "brotherhaunt", "beetleguardmastercrystal", "minorconstructattachable", "ancientwisp", "beetlecrystal", "clayman", "corruptionspike", "devotedlemurian", "lemurianbruisermasterhaunted", "lemurianbruisermasterpoison", "player", "railgunner", "voidbarnaclenocast", "minivoidraidcrabbase", "voidraidcrabjoint", "artifactshell" };

        public static List<string> equipBlacklist = new List<string>() { "elitegoldequipment", "eliteyellowequipment", "enigma", "irradiatinglaser", "ghostgun", "lunarpotion", "orbonuse", "orbitallaser", "soulcorruptor", "souljar" };

        public static List<string> playerEnemies = new List<string>() { "bandit2monster", "captainmonster", "chefmonster", "commandomonster", "crocomonster", "engimonster", "falsesonmonster", "huntressmonster", "loadermonster", "magemonster", "mercmonster", "drone1", "dronebackup", "dronecommander", "dronemissile", "engibeamturret", "engiwalkerturret", "engiturret", "megadrone", "railgunnermonster", "seekermonster", "toolbotmonster", "treebotmonster", "turret1", "voidsurvivormonster", "drone2", "emergencydrone", "equipmentdrone", "flamedrone", "hereticmonster" };

        public static List<string> allyPrefabs = new List<string>() { "titangoldally", "beetleguardally", "minorconstructonkill", "nullifierally", "voidbarnacleally", "voidjailerally", "voidmegacrabally", "squidturret", "roboballredbuddy", "roboballgreenbuddy", "dronecommander" };

        public static List<string> dronePrefabs = new List<string>() { "drone1", "dronebackup", "dronemissile", "engibeamturret", "engiwalkerturret", "engiturret", "megadrone", "turret1", "drone2", "emergencydrone", "equipmentdrone", "flamedrone" };


        public static Dictionary<string, string> nameReplacements = new Dictionary<string, string>
        {
            { "brotherhurt", "Phase 4 Mithrix" },
            { "key2", "value2" }
        };


        public struct MasterPrefabConfig
        {
            public readonly ConfigEntry<bool> configEntry;
            public readonly CharacterMaster masterPrefab;
            public MasterPrefabConfig(ConfigEntry<bool> configEntry, CharacterMaster masterPrefab)
            {
                this.configEntry = configEntry;
                this.masterPrefab = masterPrefab;
            }
        }

        public struct EquipmentDefConfig
        {
            public readonly ConfigEntry<bool> configEntry;
            public readonly EquipmentDef equipDef;
            public EquipmentDefConfig(ConfigEntry<bool> configEntry, EquipmentDef equipDef)
            {
                this.configEntry = configEntry;
                this.equipDef = equipDef;
            }
        }

        public static List<int> masterPrefabIndexes = new List<int>();

        private static readonly List<DirectorCard> characterSpawnCard = new List<DirectorCard>();

        public void Awake()
        {
            Log.Init(Logger);
            config = Config;
            masterPrefabEnemyConfigEntries = new List<MasterPrefabConfig>();
            masterPrefabBossConfigEntries = new List<MasterPrefabConfig>();

            finalBossMasters = new List<CharacterMaster>();
            enabledBossMasters = new List<CharacterMaster>();
            enabledEnemyMasters = new List<CharacterMaster>();

            equipmentDefConfigEntries = new List<EquipmentDefConfig>();
            enabledEquipmentDefs = new List<EquipmentDef>();

            On.RoR2.CombatDirector.Spawn += EnemySpawn;

            On.RoR2.CharacterSpawnCard.Spawn += CharacterSpawnCall;
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster += BuyDrone;
            On.RoR2.EquipmentSlot.FireDroneBackup += FireDroneBackup;

            On.RoR2.CharacterBody.SendConstructTurret += ConstructTurret;

            On.RoR2.BazaarController.Awake += BazaarControllerAwake;

            On.EntityStates.NewtMonster.KickFromShop.OnEnter += NewtKickFromShop;

            On.RoR2.SceneDirector.Start += SceneStart;

            On.EntityStates.Assassin2.Hide.OnEnter += AssassinOnEnter;
            On.EntityStates.Assassin2.Hide.Reveal += AssassinReveal;

            rnd = new System.Random();

            Configs();

            GetCharacterSpawnCards();
        }

        private void AssassinReveal(On.EntityStates.Assassin2.Hide.orig_Reveal orig, EntityStates.Assassin2.Hide self)
        {
            Util.PlaySound(EntityStates.Assassin2.Hide.endSoundString, self.gameObject);
            self.CreateHiddenEffect(Util.GetCorePosition(self.gameObject));
            if ((bool)self.modelTransform && (bool)EntityStates.Assassin2.Hide.destealthMaterial)
            {
                TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(self.animator.gameObject);
                temporaryOverlayInstance.duration = 1f;
                temporaryOverlayInstance.destroyComponentOnEnd = true;
                temporaryOverlayInstance.originalMaterial = EntityStates.Assassin2.Hide.destealthMaterial;
                temporaryOverlayInstance.inspectorCharacterModel = self.animator.gameObject.GetComponent<CharacterModel>();
                temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlayInstance.animateShaderAlpha = true;
            }
            if ((bool)self.characterMotor)
            {
                self.characterMotor.enabled = true;
            }
            self.PlayAnimation("Gesture", EntityStates.Assassin2.Hide.AppearStateHash);
            if ((bool)self.characterBody && NetworkServer.active)
            {
                self.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
            }
            if ((bool)self.healthComponent)
            {
                self.healthComponent.dontShowHealthbar = false;
            }
            if ((bool)self.smokeEffectInstance)
            {
                EntityState.Destroy(self.smokeEffectInstance);
            }
            self.hidden = false;
        }

        private void AssassinOnEnter(On.EntityStates.Assassin2.Hide.orig_OnEnter orig, EntityStates.Assassin2.Hide self)
        {
            if ((bool)self.characterBody)
            {
                self.attackSpeedStat = self.characterBody.attackSpeed;
                self.damageStat = self.characterBody.damage;
                self.critStat = self.characterBody.crit;
                self.moveSpeedStat = self.characterBody.moveSpeed;
            }
            Util.PlaySound(EntityStates.Assassin2.Hide.beginSoundString, self.gameObject);
            self.modelTransform = self.GetModelTransform();
            if ((bool)self.modelTransform)
            {
                self.animator = self.modelTransform.GetComponent<Animator>();
                self.characterModel = self.modelTransform.GetComponent<CharacterModel>();
                if ((bool)EntityStates.Assassin2.Hide.smokeEffectPrefab)
                {
                    Transform transform = self.modelTransform;
                    if ((bool)transform)
                    {
                        self.smokeEffectInstance = UnityEngine.Object.Instantiate(EntityStates.Assassin2.Hide.smokeEffectPrefab, transform);
                        ScaleParticleSystemDuration component = self.smokeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                        if ((bool)component)
                        {
                            component.newDuration = component.initialDuration;
                        }
                    }
                }
            }
            self.PlayAnimation("Gesture", EntityStates.Assassin2.Hide.DisappearStateHash);
            if ((bool)self.characterBody && NetworkServer.active)
            {
                self.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
            }
            self.CreateHiddenEffect(Util.GetCorePosition(self.gameObject));
/*            if ((bool)self.healthComponent)
            {
                self.healthComponent.dontShowHealthbar = true;
            }*/
            self.hidden = true;
        }

        private void SceneStart(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            //Log.Info("SCENENAME: " + SceneInfo.instance.sceneDef.cachedName);
            if (SceneInfo.instance.sceneDef.cachedName.ToLower().Contains("goolake"))
            {
                ReplaceRingLemurians();
                //Log.Info("REPLACING LEMURIAN BODIES");
            }
        }

        private async void ReplaceRingLemurians()
        {

            await Task.Delay((int)(3000)).ContinueWith(t => {
                GameObject secretRingArea = GameObject.Find("HOLDER: Secret Ring Area Content");
                if (secretRingArea)
                {
                    Transform eventController = secretRingArea.transform.GetChild(1).transform.GetChild(0);
                    if (eventController)
                    {
                        foreach (CharacterMaster origMaster in eventController.GetComponentsInChildren<CharacterMaster>())
                        {
                            CharacterMaster newMaster = GetReplacementPrefab(origMaster, false);
                            if (newMaster)
                            {
                                //Log.Info("REPLACING WITH: " + newMaster.name);
                                origMaster.bodyPrefab = newMaster.bodyPrefab;
                            }
                        }
                    }
                }
            });
        }

        private void NewtKickFromShop(On.EntityStates.NewtMonster.KickFromShop.orig_OnEnter orig, EntityStates.NewtMonster.KickFromShop self)
        {
            if(!SceneInfo.instance.sceneDef.cachedName.ToLower().Contains("bazaar"))
            {
                self.OnExit();
            } else
            {
                orig(self);
            }
        }

        // Credit to IHarbHD for how to do this
        private static void GetCharacterSpawnCards()
        {
            RoR2Application.onLoad += () =>
            {
                foreach (var resourceLocator in Addressables.ResourceLocators)
                {
                    foreach (var key in resourceLocator.Keys)
                    {
                        var keyString = key.ToString();
                        if (keyString.Contains("csc"))
                        {
                            var iscLoadRequest = Addressables.LoadAssetAsync<CharacterSpawnCard>(keyString);

                            iscLoadRequest.Completed += (completedAsyncOperation) =>
                            {
                                if (completedAsyncOperation.Status == AsyncOperationStatus.Succeeded)
                                {
                                    characterSpawnCard.Add(new DirectorCard
                                    {
                                        spawnCard = completedAsyncOperation.Result,
                                        forbiddenUnlockableDef = null,
                                        minimumStageCompletions = 0,
                                        preventOverhead = true,
                                        spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    });
                                }
                            };
                        }
                    }
                }
                
            };
            
        }

        private bool FireDroneBackup(On.RoR2.EquipmentSlot.orig_FireDroneBackup orig, EquipmentSlot self)
        {
            if(dronesRandomised.Value)
            {
                int sliceCount = 4;
                float num = 25f;
                if (NetworkServer.active)
                {
                    float y = Quaternion.LookRotation(self.GetAimRay().direction).eulerAngles.y;
                    float num2 = 3f;
                    foreach (float item in new DegreeSlices(sliceCount, 0.5f))
                    {
                        Quaternion quaternion = Quaternion.Euler(-30f, y + item, 0f);
                        Quaternion rotation = Quaternion.Euler(0f, y + item + 180f, 0f);
                        Vector3 position = self.transform.position + quaternion * (Vector3.forward * num2);

                        GameObject origPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/DroneBackupMaster");

                        CharacterMaster origMaster = origPrefab.GetComponent<CharacterMaster>();

                        GameObject newPrefab = GetReplacementPrefab(origMaster, true).gameObject;

                        CharacterMaster characterMaster = self.SummonMaster(newPrefab, position, rotation);
                        if ((bool)characterMaster)
                        {
                            characterMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = num + UnityEngine.Random.Range(0f, 3f);
                        }
                    }
                }
                self.subcooldownTimer = 0.5f;
                return true;
            } else
            {
                return orig(self);
            }
            
        }
        public DirectorCard GetSpawnCard(MasterCatalog.MasterIndex masterIndex)
        {
            foreach (var card in characterSpawnCard)
            {
                if (card.spawnCard && card.spawnCard.prefab && card.spawnCard.prefab.GetComponent<CharacterMaster>() != null && card.spawnCard.prefab.GetComponent<CharacterMaster>().masterIndex == masterIndex)
                {
                    return card;
                }
            }
            return null;
        }


        private bool EnemySpawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            GameObject origPrefab = spawnCard.prefab;
            if (origPrefab)
            {
                CharacterMaster origMaster = origPrefab.GetComponent<CharacterMaster>();
                if (origMaster)
                {
                    if (!IsBoss(origMaster))
                    {
                        int spawnChanceRoll = rnd.Next(0, 100);
                        //Log.Info("CHANCE ROLL: " + spawnChanceRoll);
                        if (spawnChanceRoll >= spawnChance.Value)
                        {
                            if(!spawnCard.name.ToLower().Contains("halcyonite") && spawnCard.prefab.GetComponent<CharacterMaster>() != null && !spawnCard.prefab.GetComponent<CharacterMaster>().isBoss)
                            {
                                //Log.Info("SKIPPING");
                                return true;
                            }
                        }
                        //Log.Info("ALLOWING");
                    }
                }
            }
            //return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);

            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
            {
                placementMode = placementMode,
                spawnOnTarget = spawnTarget,
                preventOverhead = preventOverhead
            };
            DirectorCore.GetMonsterSpawnDistance(spawnDistance, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
            directorPlacementRule.maxDistance = Mathf.Min(self.maxSpawnDistance, directorPlacementRule.maxDistance * self.spawnDistanceMultiplier);
            directorPlacementRule.minDistance = Mathf.Max(0f, Mathf.Min(directorPlacementRule.maxDistance - self.minSpawnRange, directorPlacementRule.minDistance * self.spawnDistanceMultiplier));
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, self.rng);
            directorSpawnRequest.ignoreTeamMemberLimit = self.ignoreTeamSizeLimit;
            directorSpawnRequest.teamIndexOverride = self.teamIndex;
            directorSpawnRequest.onSpawnedServer = OnCardSpawned;
            if (!DirectorCore.instance.TrySpawnObject(directorSpawnRequest))
            {
                Debug.LogFormat("Spawn card {0} failed to spawn. Aborting cost procedures.", spawnCard);
                return false;
            }
            return true;
            void OnCardSpawned(SpawnCard.SpawnResult result)
            {
                if (result.success)
                {
                    float num = 1f;
                    float num2 = 1f;
                    CharacterMaster component = result.spawnedInstance.GetComponent<CharacterMaster>();
                    GameObject bodyObject = component.GetBodyObject();
                    CharacterBody component2 = bodyObject.GetComponent<CharacterBody>();
                    if ((bool)component2)
                    {
                        component2.cost = valueMultiplier * (float)spawnCard.directorCreditCost;
                    }
                    if ((bool)self.combatSquad)
                    {
                        self.combatSquad.AddMember(component);
                    }
                    num = eliteDef?.healthBoostCoefficient ?? 1f;
                    num2 = eliteDef?.damageBoostCoefficient ?? 1f;
                    if (self.isHalcyonShrineSpawn)
                    {
                        if (self.shrineHalcyoniteDifficultyLevel > 20)
                        {
                            self.shrineHalcyoniteDifficultyLevel = 20 + self.shrineHalcyoniteDifficultyLevel / 100;
                        }
                        num += (float)self.shrineHalcyoniteDifficultyLevel * 0.6f;
                        num2 += (float)self.shrineHalcyoniteDifficultyLevel * 0.25f;
                        eliteDef = DLC2Content.Elites.Aurelionite;
                    }
                    EquipmentIndex equipmentIndex = eliteDef?.eliteEquipmentDef?.equipmentIndex ?? EquipmentIndex.None;
                    if (equipmentIndex != EquipmentIndex.None)
                    {
                        component.inventory.SetEquipmentIndex(equipmentIndex);
                    }
                    if ((bool)self.combatSquad && self.combatSquad.grantBonusHealthInMultiplayer)
                    {
                        int livingPlayerCount = Run.instance.livingPlayerCount;
                        num *= Mathf.Pow(livingPlayerCount, 1f);
                    }
                    component.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((num - 1f) * 10f));
                    component.inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((num2 - 1f) * 10f));
                    DeathRewards component3 = bodyObject.GetComponent<DeathRewards>();
                    if ((bool)component3)
                    {
                        /////

                        var newSpawnCard = GetSpawnCard(component.masterIndex);

                        int cost = spawnCard.directorCreditCost;

                        if (newSpawnCard != null)
                        {
                            cost = newSpawnCard.cost;
                        }

                        /////

                        float num3 = (float)cost * valueMultiplier * self.expRewardCoefficient;
                        component3.spawnValue = (int)Mathf.Max(1f, num3);
                        if (num3 > Mathf.Epsilon)
                        {
                            component3.expReward = (uint)Mathf.Max(1f, num3 * Run.instance.compensatedDifficultyCoefficient);
                            component3.goldReward = (uint)Mathf.Max(1f, num3 * self.goldRewardCoefficient * 2f * Run.instance.compensatedDifficultyCoefficient);
                        }
                        else
                        {
                            component3.expReward = 0u;
                            component3.goldReward = 0u;
                        }
                    }
                    if ((bool)self.spawnEffectPrefab && NetworkServer.active)
                    {
                        Vector3 origin = result.position;
                        CharacterBody characterBody = component2;
                        if ((bool)characterBody)
                        {
                            origin = characterBody.corePosition;
                        }
                        EffectManager.SpawnEffect(self.spawnEffectPrefab, new EffectData
                        {
                            origin = origin
                        }, transmit: true);
                    }
                    self.onSpawnedServer?.Invoke(result.spawnedInstance);
                }
            }
        }

        private void BazaarControllerAwake(On.RoR2.BazaarController.orig_Awake orig, BazaarController self)
        {
            orig(self);

            GameObject shopkeeper = GameObject.Find("HOLDER: Store Platforms").transform.GetChild(0).transform.GetChild(1).gameObject;
            CharacterMaster shopkeeperMaster = shopkeeper.GetComponent<CharacterMaster>();

            if (enabledBossMasters.Count + enabledEnemyMasters.Count > 0)
            {
                int replacementIndex = rnd.Next(0, enabledBossMasters.Count + enabledEnemyMasters.Count);

                if (replacementIndex >= enabledBossMasters.Count)
                {
                    shopkeeperMaster.bodyPrefab = enabledEnemyMasters[replacementIndex - enabledBossMasters.Count].bodyPrefab;
                }
                else
                {
                    shopkeeperMaster.bodyPrefab = enabledBossMasters[replacementIndex].bodyPrefab;
                }
            }
        }

        private void ConstructTurret(On.RoR2.CharacterBody.orig_SendConstructTurret orig, CharacterBody self, CharacterBody builder, Vector3 position, Quaternion rotation, MasterCatalog.MasterIndex masterIndex)
        {

            if (!NetworkClient.active)
            {
                Debug.LogWarning("[Client] function 'System.Void RoR2.CharacterBody::SendConstructTurret(RoR2.CharacterBody,UnityEngine.Vector3,UnityEngine.Quaternion,RoR2.MasterCatalog/MasterIndex)' called on server");
                return;
            }

            GameObject origPrefab = MasterCatalog.GetMasterPrefab(masterIndex);
            string origPrefabName = origPrefab.name;

            GameObject newPrefab = origPrefab;
            if (origPrefab != null)
            {
                CharacterMaster origMaster = origPrefab.GetComponent<CharacterMaster>();
                if (origMaster != null)
                {

                    if (turretsRandomised.Value)
                    {
                        newPrefab = GetReplacementPrefab(origMaster, true).gameObject;
                        newPrefab.GetComponent<CharacterMaster>().teamIndex = origMaster.teamIndex;
                        newPrefab.GetComponent<CharacterMaster>()._teamIndex = origMaster._teamIndex;
                    }
                }
                else
                {
                    Log.Info("Could not find orig character master");
                }
            }
            else
            {
                Log.Info("No orig prefab for enemy spawn");
            }


            CharacterBody.ConstructTurretMessage msg = new CharacterBody.ConstructTurretMessage
            {
                builder = builder.gameObject,
                position = position,
                rotation = rotation,
                turretMasterIndex = MasterCatalog.FindMasterIndex(newPrefab)
            };
            ClientScene.readyConnection.Send(62, msg);
        }

        private CharacterMaster BuyDrone(On.RoR2.SummonMasterBehavior.orig_OpenSummonReturnMaster orig, SummonMasterBehavior self, Interactor activator)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'RoR2.CharacterMaster RoR2.SummonMasterBehavior::OpenSummonReturnMaster(RoR2.Interactor)' called on client");
                return null;
            }


            string origPrefabName = self.masterPrefab.name;

            GameObject origPrefab = self.masterPrefab;
            GameObject newPrefab = origPrefab;
            if (origPrefab != null)
            {
                CharacterMaster origMaster = origPrefab.GetComponent<CharacterMaster>();
                if (origMaster != null)
                {

                    if (IsDrone(origMaster) && dronesRandomised.Value)
                    {
                        newPrefab = GetReplacementPrefab(origMaster, true).gameObject;
                        newPrefab.GetComponent<CharacterMaster>().teamIndex = origMaster.teamIndex;
                        newPrefab.GetComponent<CharacterMaster>()._teamIndex = origMaster._teamIndex;
                    }
                }
                else
                {
                    Log.Info("Could not find orig character master");
                }
            }
            else
            {
                Log.Info("No orig prefab for enemy spawn");
            }

            if(!newPrefab.GetComponent<AIOwnership>())
            {
                AIOwnership aiOwnership = newPrefab.AddComponent<AIOwnership>();
                aiOwnership.ownerMaster = newPrefab.GetComponent<CharacterMaster>();
                //aiOwnership.baseAI = newPrefab.GetComponent<AIOwnership>().baseAI;
            }

            

            float num = 0f;
            CharacterMaster characterMaster = new MasterSummon
            {
                masterPrefab = newPrefab,
                position = self.transform.position + Vector3.up * num,
                rotation = self.transform.rotation,
                summonerBodyObject = activator?.gameObject,
                ignoreTeamMemberLimit = true,
                useAmbientLevel = true
            }.Perform();
            if ((bool)characterMaster)
            {
                DontDestroyOnLoad(characterMaster);
                GameObject bodyObject = characterMaster.GetBodyObject();
                if ((bool)bodyObject)
                {
                    ModelLocator component = bodyObject.GetComponent<ModelLocator>();
                    if ((bool)component && (bool)component.modelTransform)
                    {
                        TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(component.modelTransform.gameObject);
                        temporaryOverlayInstance.duration = 0.5f;
                        temporaryOverlayInstance.animateShaderAlpha = true;
                        temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlayInstance.destroyComponentOnEnd = true;
                        temporaryOverlayInstance.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matSummonDrone");
                        temporaryOverlayInstance.AddToCharacterModel(component.modelTransform.GetComponent<CharacterModel>());
                    }
                }
            }
            if (self.destroyAfterSummoning)
            {
                Destroy(self.gameObject);
            }
            return characterMaster;
        }

        private void CharacterSpawnCall(On.RoR2.CharacterSpawnCard.orig_Spawn orig, CharacterSpawnCard self, Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnCard.SpawnResult result)
        {
            string origPrefabName = self.prefab.name;
            
            if (!enemyMinionsRandomised.Value)
            {
                if (origPrefabName.ToLower().Contains("minorconstructattachable") || origPrefabName.ToLower().Contains("roboballmini"))
                {
                    orig(self, position, rotation, directorSpawnRequest, ref result);
                    return;
                }
                if (directorSpawnRequest.summonerBodyObject)
                {
                    if(origPrefabName.ToLower().Contains("beetleguard") && directorSpawnRequest.summonerBodyObject.name.ToLower().Contains("beetlequeen"))
                    {
                        orig(self, position, rotation, directorSpawnRequest, ref result);
                        return;
                    }
                }
            }

            if(!voidInfestorsRandomised.Value)
            {
                if (origPrefabName.ToLower().Contains("voidinfestor"))
                {
                    orig(self, position, rotation, directorSpawnRequest, ref result);
                    return;
                }
            }

            GameObject origPrefab = directorSpawnRequest.spawnCard.prefab;
            GameObject newPrefab = origPrefab;
            if (origPrefab != null)
            {
                CharacterMaster origMaster = origPrefab.GetComponent<CharacterMaster>();
                if (origMaster != null)
                {
                    //Log.Info("TRY SPAWN: " + origMaster.name);

                    if(self is MasterCopySpawnCard && !alliesRandomised.Value)
                    {
                        goto AfterReplacePrefab;
                    }

                    if (((origMaster._teamIndex == TeamIndex.Monster || origMaster._teamIndex == TeamIndex.Void || origMaster._teamIndex == TeamIndex.Lunar) && !IsAlly(origMaster)) || (IsAlly(origMaster) && alliesRandomised.Value))
                    {
                        CharacterMaster newMaster = GetReplacementPrefab(origMaster, IsAlly(origMaster) || self is MasterCopySpawnCard);

                        if (newMaster == null)
                        {

                            result.spawnedInstance = null;
                            result.success = false;
                            _ = result.success;
                            return;
                        }
                        
                        newPrefab = newMaster.gameObject;
                        newPrefab.GetComponent<CharacterMaster>().teamIndex = origMaster.teamIndex;
                        newPrefab.GetComponent<CharacterMaster>()._teamIndex = origMaster._teamIndex;
                    }
                }
                else
                {
                    Log.Info("Could not find orig character master");
                }
            }
            else
            {
                Log.Info("No orig prefab for enemy spawn");
            }

            AfterReplacePrefab:

            MasterSummon masterSummon = new MasterSummon
            {
                masterPrefab = newPrefab,
                position = position,
                rotation = rotation,
                summonerBodyObject = directorSpawnRequest.summonerBodyObject,
                teamIndexOverride = directorSpawnRequest.teamIndexOverride,
                ignoreTeamMemberLimit = directorSpawnRequest.ignoreTeamMemberLimit,
                loadout = self.GetRuntimeLoadout(),
                inventoryToCopy = self.inventoryToCopy,
                inventoryItemCopyFilter = self.inventoryItemCopyFilter,
                inventorySetupCallback = self,
                preSpawnSetupCallback = self.GetPreSpawnSetupCallback(),
                useAmbientLevel = true
            };
            result.spawnedInstance = masterSummon.Perform()?.gameObject;
            result.success = result.spawnedInstance;
            _ = result.success;


            //Log.Info(result.ToString());

            CharacterMaster master = result.spawnedInstance.GetComponent<CharacterMaster>();
            CharacterBody body = master.GetBody();
            Inventory inventory = master.inventory;

            if (origPrefab != null)
            {
                CharacterMaster origMaster = origPrefab.GetComponent<CharacterMaster>();

                if (origMaster && IsAlly(origMaster) && alliesRandomised.Value)
                {
                    master.gameObject.AddComponent<Deployable>();
                }
            }

            if (body && body.name.ToLower().Contains("equipmentdrone") && !origPrefabName.ToLower().Contains("equipmentdrone") && inventory)
            {
                EquipmentDef equip = GetEquip();
                if(equip != null)
                {

                    if(equip.name.ToLower().Contains("healandrevive") && !equip.name.ToLower().Contains("consumed") && master.teamIndex != TeamIndex.Player)
                    {
                        return;
                    }

                    inventory.SetEquipmentIndex(equip.equipmentIndex);
                }
                
            }
        }
        public EquipmentDef GetEquip()
        {

            if (enabledEquipmentDefs.Count > 0)
            {
                int equipIndex = rnd.Next(0, enabledEquipmentDefs.Count);
                //Log.Info("ROLLED EQUIP: " + equipIndex);
                //Log.Info("RETURNING: " + enabledEquipmentDefs[equipIndex].name);
                return enabledEquipmentDefs[equipIndex];
            }
            return null ;
        }

        public CharacterMaster GetReplacementPrefab(CharacterMaster origMaster, bool isAlly)
        {

            List<CharacterMaster> tempEnabledEnemyMasters = new List<CharacterMaster>(enabledEnemyMasters);

            if (origMaster.teamIndex == TeamIndex.Player || origMaster._teamIndex == TeamIndex.Player || isAlly || IsAlly(origMaster))
            {
                // Dont want void infestor allies
                tempEnabledEnemyMasters.RemoveAll(x => x.name.ToLower().Contains("voidinfestor"));
            }

            if (IsFinalBoss(origMaster))
            {
                if(finalBossMode.Value == FinalBossEnum.defaultBoss)
                {
                    return origMaster;
                } else if (finalBossMode.Value == FinalBossEnum.AnyFinalBoss)
                {
                    int replacementIndex = rnd.Next(0, finalBossMasters.Count);
                    return finalBossMasters[replacementIndex];
                }

                List<CharacterMaster> tempEnabledBossMasters = new List<CharacterMaster>(enabledBossMasters);

                if (origMaster.name.ToLower().Contains("brother"))
                {
                    // Dont want to randomise into self
                    tempEnabledBossMasters.RemoveAll(x => x.name.ToLower().Contains("brother"));
                }

                if (finalBossMode.Value == FinalBossEnum.AnyBoss)
                {
                    if (tempEnabledBossMasters.Count > 0)
                    {
                        int replacementIndex = rnd.Next(0, tempEnabledBossMasters.Count);
                        return tempEnabledBossMasters[replacementIndex];
                    }
                }
                
                if (finalBossMode.Value == FinalBossEnum.AnyEnemy)
                {
                    if (tempEnabledBossMasters.Count + tempEnabledEnemyMasters.Count > 0)
                    {
                        int replacementIndex = rnd.Next(0, tempEnabledBossMasters.Count + tempEnabledEnemyMasters.Count);

                        if (replacementIndex >= tempEnabledBossMasters.Count)
                        {
                            return tempEnabledEnemyMasters[replacementIndex - tempEnabledBossMasters.Count];
                        }
                        else
                        {
                            return tempEnabledBossMasters[replacementIndex];
                        }
                    }
                }

                return origMaster;
            }

            if (isAlly)
            {
                if(alliesCanBeBosses.Value)
                {
                    if (enabledBossMasters.Count + tempEnabledEnemyMasters.Count > 0)
                    {
                        int replacementIndex = rnd.Next(0, enabledBossMasters.Count + tempEnabledEnemyMasters.Count);

                        if (replacementIndex >= enabledBossMasters.Count)
                        {
                            return tempEnabledEnemyMasters[replacementIndex - enabledBossMasters.Count];
                        }
                        else
                        {
                            return enabledBossMasters[replacementIndex];
                        }
                    }
                }
            }

            if (IsBoss(origMaster))
            {
                if(enabledBossMasters.Count > 0)
                {
                    int replacementIndex = rnd.Next(0, enabledBossMasters.Count);
                    return enabledBossMasters[replacementIndex];
                }
            }
            if (tempEnabledEnemyMasters.Count > 0)
            {
                int replacementIndex = rnd.Next(0, tempEnabledEnemyMasters.Count);
                //Log.Info("ROLLED: " + replacementIndex);
                //Log.Info("SPAWNING: " + tempEnabledEnemyMasters[replacementIndex].name);
                return tempEnabledEnemyMasters[replacementIndex];
            }
            return origMaster;
        }


        public void OnDisable()
        {
            On.RoR2.CombatDirector.Spawn -= EnemySpawn;

            On.RoR2.CharacterSpawnCard.Spawn -= CharacterSpawnCall;
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster -= BuyDrone;
            On.RoR2.EquipmentSlot.FireDroneBackup -= FireDroneBackup;

            On.RoR2.CharacterBody.SendConstructTurret -= ConstructTurret;

            On.RoR2.BazaarController.Awake -= BazaarControllerAwake;

            On.EntityStates.NewtMonster.KickFromShop.OnEnter -= NewtKickFromShop;

            On.RoR2.SceneDirector.Start -= SceneStart;

            On.EntityStates.Assassin2.Hide.OnEnter -= AssassinOnEnter;
            On.EntityStates.Assassin2.Hide.Reveal -= AssassinReveal;
        }

        public static bool IsFinalBoss(CharacterMaster master)
        {
            String name = master.name;

            if (name != null)
            {
                foreach (string s in finalBossNames)
                {
                    if (name.ToLower().Equals(s) || name.ToLower().Contains(s))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsBoss(CharacterMaster master)
        {
            String name = master.name;

            if (name != null)
            {
                foreach (string s in bossNames)
                {
                    if (name.ToLower().Equals(s) || name.ToLower().Contains(s))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsPlayerEnemy(CharacterMaster master)
        {
            String name = master.name;

            if (name != null)
            {
                foreach (string s in playerEnemies)
                {
                    if (name.ToLower().Equals(s) || name.ToLower().Contains(s))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsAlly(CharacterMaster master)
        {
            String name = master.name;
            
            if (name != null)
            {
                foreach (string s in allyPrefabs)
                {
                    if (name.ToLower().Equals(s) || name.ToLower().Contains(s))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsDrone(CharacterMaster master)
        {
            String name = master.name;

            if (name != null)
            {
                foreach (string s in dronePrefabs)
                {
                    if (name.ToLower().Equals(s) || name.ToLower().Contains(s))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool PrefabBlacklisted(CharacterMaster master)
        {
            String name = master.name;

            if (name != null)
            {
                foreach (string s in prefabBlacklist)
                {
                    if (name.ToLower().Equals(s) || name.ToLower().Contains(s))
                    {
                        return true;
                    }
                }

                if (IsAlly(master))
                {
                    return true;
                }

                // Special cases
                if (name.ToLower().Contains("voidraidcrab") && !name.ToLower().Contains("mini") || name.ToLower().Contains("railgunner") && !name.ToLower().Contains("2"))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EquipBlacklisted(EquipmentDef def)
        {
            string defName = def.name;

            if (defName != null)
            {
                foreach (string s in equipBlacklist)
                {
                    if (defName.ToLower().Equals(s) || defName.ToLower().Contains(s))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static String AdjustFormattedEquipName(String formattedEquipName, String defName)
        {
            if (defName.ToLower().Contains("elitevoid"))
            {
                formattedEquipName = "Void Aspect";
            }

            return formattedEquipName;
        }

        public static String AdjustFormattedPrefabName(String formattedMasterName, String masterName)
        {
            if (masterName.ToLower().Contains("ally"))
            {
                formattedMasterName = formattedMasterName + " Ally";
            }
            if (masterName.ToLower().Contains("brotherglass"))
            {
                formattedMasterName = formattedMasterName + " Glass";
            }
            if (masterName.ToLower().Contains("brotherhurt"))
            {
                formattedMasterName = "Phase 4 Mithrix";
            }
            if (masterName.ToLower().Contains("minorconstructonkill"))
            {
                formattedMasterName = "Alpha Construct Ally";
            }
            if (masterName.ToLower().Contains("lunarexploder"))
            {
                formattedMasterName = "Lunar Exploder";
            }
            if (masterName.ToLower().Contains("lunargolem"))
            {
                formattedMasterName = "Lunar Golem";
            }
            if (masterName.ToLower().Contains("lunarwisp"))
            {
                formattedMasterName = "Lunar Wisp";
            }
            if (masterName.ToLower().Contains("wispsoul"))
            {
                formattedMasterName = "Soul Wisp";
            }
            if (masterName.ToLower().Contains("affixearthhealer"))
            {
                formattedMasterName = "Mending Healing Core";
            }
            if (masterName.ToLower().Contains("archwisp"))
            {
                formattedMasterName = "Arch Wisp";
            }
            if (masterName.ToLower().Contains("engibeamturret"))
            {
                formattedMasterName = "Engineer Beam Turret";
            } else if (masterName.ToLower().Contains("engiturret"))
            {
                formattedMasterName = "Engineer Turret";
            } else if (masterName.ToLower().Contains("engiwalkerturret"))
            {
                formattedMasterName = "Engineer Walking Turret";
            }
            if (masterName.ToLower().Contains("falsesonmonster"))
            {
                formattedMasterName = "False Son Survivor";
            }
            if (masterName.ToLower().Contains("lemurianbruiserfire"))
            {
                formattedMasterName = "Kjaro Elder Lemurian";
            }
            if (masterName.ToLower().Contains("lemurianbruiserice"))
            {
                formattedMasterName = "Runald Elder Lemurian";
            }
            if (masterName.ToLower().Contains("falsesonbosslunarshardbroken"))
            {
                formattedMasterName = "Phase 3 False Son";
            } else if (masterName.ToLower().Contains("falsesonbosslunarshard"))
            {
                formattedMasterName = "Phase 2 False Son";
            } else if (masterName.ToLower().Contains("falsesonboss"))
            {
                formattedMasterName = "Phase 1 False Son";
            }
            if (masterName.ToLower().Contains("majorconstruct"))
            {
                formattedMasterName = "Old Xi Construct";
            }
            if (masterName.ToLower().Contains("minivoidraidcrabphase1"))
            {
                formattedMasterName = "Phase 1 Voidling";
            }
            if (masterName.ToLower().Contains("minivoidraidcrabphase2"))
            {
                formattedMasterName = "Phase 2 Voidling";
            }
            if (masterName.ToLower().Contains("minivoidraidcrabphase3"))
            {
                formattedMasterName = "Phase 3 Voidling";
            }



            return formattedMasterName;
        }

        public static ConfigEntry<bool> enemyMinionsRandomised;

        public static ConfigEntry<bool> alliesRandomised;

        public static ConfigEntry<bool> dronesRandomised;

        public static ConfigEntry<bool> turretsRandomised;

        public static ConfigEntry<bool> alliesCanBeBosses;

        public static ConfigEntry<bool> voidInfestorsRandomised;

        public static ConfigEntry<float> spawnChance;

        public static ConfigEntry<FinalBossEnum> finalBossMode;

        public enum FinalBossEnum
        {
            defaultBoss,
            AnyFinalBoss,
            AnyBoss,
            AnyEnemy
        }

        private static void Configs()
        {

            finalBossMode = config.Bind("General", "Final Boss Randomiser", FinalBossEnum.AnyBoss, "What final bosses (Mithrix, False Son, and Voidling) can be randomised into. defaultBoss will not randomise the boss at all, so the Mithrix fight will be normal. AnyFinalBoss will randomise into either Mithrix, False Son, or Voidling, AnyBoss will randomise into any boss enabled in the 'Possible Bosses' config, and AnyEnemy will randomise into any enemy or boss enabled in the configs.");

            ModSettingsManager.AddOption(new ChoiceOption(finalBossMode));

            enemyMinionsRandomised = config.Bind("General", "Randomise Enemy Minions", false, "If enemy minions (for example solus probes or beetle queens beetle guards) should be randomised.\nDefault is false.");
            ModSettingsManager.AddOption(new CheckBoxOption(enemyMinionsRandomised));

            alliesRandomised = config.Bind("General", "Randomise Allies", false, "If spawned allies (Empathy Cores, BG, Goobo etc) should be randomised.\nDefault is false.");
            ModSettingsManager.AddOption(new CheckBoxOption(alliesRandomised));

            dronesRandomised = config.Bind("General", "Randomise Drones", false, "If spawned drones (gunner turrets, healing drones etc) should be randomised.\nDefault is false.");
            ModSettingsManager.AddOption(new CheckBoxOption(dronesRandomised));

            turretsRandomised = config.Bind("General", "Randomise Turrets", false, "If engineers turrets should be randomised.\nDefault is false.");
            ModSettingsManager.AddOption(new CheckBoxOption(turretsRandomised));

            alliesCanBeBosses = config.Bind("General", "Random Allies Can Be Bosses", false, "If randomised allies can spawn from the list of bosses as well as enemes.\nDefault is false.");
            ModSettingsManager.AddOption(new CheckBoxOption(alliesCanBeBosses));

            voidInfestorsRandomised = config.Bind("General", "Randomise Void Infestors", true, "If void infestors from void cradles should be randomised.\nDefault is true.");
            ModSettingsManager.AddOption(new CheckBoxOption(voidInfestorsRandomised));

            spawnChance = config.Bind("General", "Spawn Rate", 100f, "How often enemies should spawn.\nDefault is 100 (100% of the base game spawn rate).");
            ModSettingsManager.AddOption(new StepSliderOption(spawnChance,
                new StepSliderConfig
                {
                    min = 0f,
                    max = 100f,
                    increment = 1f
                }));
        }

        public static List<string> formattedStrings = new List<string>() {};

        // Initialising configs
        [SystemInitializer(typeof(EquipmentCatalog))]
        private static void EquipConfigs()
        {
            foreach (EquipmentDef def in EquipmentCatalog.equipmentDefs)
            {
                
                string defName = def.name;

                if (def && (defName != null))
                {
                    Log.Info("Initialising equip def config: " + defName);

                    if (EquipBlacklisted(def))
                    {
                        continue;
                    }

                    /*int index = master.name.IndexOf("Master");
                    string masterName = (index < 0)
                        ? master.name
                        : master.name.Remove(index, 6);*/

                    string formattedEquipName = defName;

                    if (def.nameToken != null)
                    {
                        formattedEquipName = def.nameToken;
                        formattedEquipName = Language.GetString(def.nameToken);
                    }

                    formattedEquipName = AdjustFormattedEquipName(formattedEquipName, defName);

                    if (!formattedStrings.Contains(formattedEquipName))
                    {
                        formattedStrings.Add(formattedEquipName);
                    }
                    else
                    {
                        formattedStrings.Add(formattedEquipName);
                        int count = formattedStrings.Where(s => s == formattedEquipName).Count();
                        formattedEquipName = formattedEquipName + " " + count;
                    }

                    formattedEquipName = formattedEquipName.Replace("'", "");

                    bool defaultVal = true;
                    if (defName.ToLower().Contains("bfg") || defName.ToLower().Contains("commandmissile") || defName.ToLower().Contains("lightning") && !defName.ToLower().Contains("elite") || defName.ToLower().Contains("saw") || defName.ToLower().Contains("fireballdash"))
                    {
                        defaultVal = false;
                    }

                    ConfigEntry<bool> equipmentDefEnabled = config.Bind("Possible Equipment Drones", formattedEquipName, defaultVal, "If it's possible for a randomised equipment drone to be holding a " + formattedEquipName + " (internal name: " + defName + ").\nDefault is " + defaultVal + ".");
                    equipmentDefEnabled.SettingChanged += (o, args) =>
                    {

                        if (equipmentDefEnabled.Value && !enabledEquipmentDefs.Contains(def))
                        {
                            enabledEquipmentDefs.Add(def);
                        }
                        else if (!equipmentDefEnabled.Value && enabledEquipmentDefs.Contains(def))
                        {
                            enabledEquipmentDefs.Remove(def);
                        }

                    };
                    ModSettingsManager.AddOption(new CheckBoxOption(equipmentDefEnabled));
                    equipmentDefConfigEntries.Add(new EquipmentDefConfig(equipmentDefEnabled, def));

                    if (equipmentDefEnabled.Value && !enabledEquipmentDefs.Contains(def))
                    {
                        enabledEquipmentDefs.Add(def);
                    }
                    else if (!equipmentDefEnabled.Value && enabledEquipmentDefs.Contains(def))
                    {
                        enabledEquipmentDefs.Remove(def);
                    }
                }

            }
        }

        // Initialising configs
        [SystemInitializer(typeof(MasterCatalog))]
        private static void MasterConfigs()
        {

            foreach (CharacterMaster master in MasterCatalog.masterPrefabMasterComponents)
            {
                if (master && (master.name != null))
                {
                    Log.Info("Initialising master prefab config: " + master.name);
                    
                    if(PrefabBlacklisted(master))
                    {
                        continue;
                    }

                    int index = master.name.IndexOf("Master");
                    string masterName = (index < 0)
                        ? master.name
                        : master.name.Remove(index, 6);

                    string formattedMasterName = masterName;

                    if (master.bodyPrefab != null)
                    {
                        if (master.bodyPrefab.GetComponent<CharacterBody>() != null)
                        {
                            formattedMasterName = master.bodyPrefab.GetComponent<CharacterBody>().name;
                            if (master.bodyPrefab.GetComponent<CharacterBody>().baseNameToken != null)
                            {
                                formattedMasterName = Language.GetString(master.bodyPrefab.GetComponent<CharacterBody>().baseNameToken);
                            }
                        }
                    } 

                    formattedMasterName = AdjustFormattedPrefabName(formattedMasterName, masterName);

                    if(!formattedStrings.Contains(formattedMasterName))
                    {
                        formattedStrings.Add(formattedMasterName);
                    } else
                    {
                        formattedStrings.Add(formattedMasterName);
                        int count = formattedStrings.Where(s => s == formattedMasterName).Count();
                        formattedMasterName = formattedMasterName + " " + count;
                    }

                    if (IsBoss(master))
                    {
                        bool defaultVal = true;
                        if(masterName.ToLower().Contains("falsesonbosslunarshardbroken") || masterName.ToLower().Contains("minivoidraidcrabphase3") || masterName.ToLower().Contains("brotherhurt"))
                        {
                            defaultVal = false;
                        }

                        ConfigEntry<bool> masterPrefabEnabled = config.Bind("Possible Bosses", formattedMasterName, defaultVal, "If it's possible for a " + formattedMasterName + " (internal name: " + masterName + ") to spawn instead of another boss.\nDefault is " + defaultVal + ".");
                        masterPrefabEnabled.SettingChanged += (o, args) => {

                            if (masterPrefabEnabled.Value && !enabledBossMasters.Contains(master))
                            {
                                enabledBossMasters.Add(master);
                            }
                            else if (!masterPrefabEnabled.Value && enabledBossMasters.Contains(master))
                            {
                                enabledBossMasters.Remove(master);
                            }

                        };
                        ModSettingsManager.AddOption(new CheckBoxOption(masterPrefabEnabled));
                        masterPrefabBossConfigEntries.Add(new MasterPrefabConfig(masterPrefabEnabled, master));

                        if(masterPrefabEnabled.Value && !enabledBossMasters.Contains(master))
                        {
                            enabledBossMasters.Add(master);
                        } else if(!masterPrefabEnabled.Value && enabledBossMasters.Contains(master))
                        {
                            enabledBossMasters.Remove(master);
                        }

                        if(IsFinalBoss(master) && !masterName.ToLower().Contains("falsesonbosslunarshardbroken") && !masterName.ToLower().Contains("brotherhurt"))
                        {
                            finalBossMasters.Add(master);
                        }
                    } else if(IsPlayerEnemy(master))
                    {

                        bool defaultVal = false;
                        if (masterName.ToLower().Contains("drone2") || masterName.ToLower().Contains("emergencydrone") || masterName.ToLower().Contains("equipmentdrone"))
                        {
                            defaultVal = true;
                        }

                        ConfigEntry<bool> masterPrefabEnabled = config.Bind("Possible Player Enemies", formattedMasterName, defaultVal, "If it's possible for a " + formattedMasterName + " (internal name: " + masterName + ") to spawn instead of another enemy.\nDefault is " + defaultVal + ".");
                        masterPrefabEnabled.SettingChanged += (o, args) => {

                            if (masterPrefabEnabled.Value && !enabledEnemyMasters.Contains(master))
                            {
                                enabledEnemyMasters.Add(master);
                            }
                            else if (!masterPrefabEnabled.Value && enabledEnemyMasters.Contains(master))
                            {
                                enabledEnemyMasters.Remove(master);
                            }

                        };
                        ModSettingsManager.AddOption(new CheckBoxOption(masterPrefabEnabled));
                        masterPrefabEnemyConfigEntries.Add(new MasterPrefabConfig(masterPrefabEnabled, master));

                        if (masterPrefabEnabled.Value && !enabledEnemyMasters.Contains(master))
                        {
                            enabledEnemyMasters.Add(master);
                        }
                        else if (!masterPrefabEnabled.Value && enabledEnemyMasters.Contains(master))
                        {
                            enabledEnemyMasters.Remove(master);
                        }
                    }
                    else
                    {
                        ConfigEntry<bool> masterPrefabEnabled = config.Bind("Possible Monster Enemies", formattedMasterName, true, "If it's possible for a " + formattedMasterName + " (internal name: " + masterName + ") to spawn instead of another enemy.\nDefault is true.");
                        masterPrefabEnabled.SettingChanged += (o, args) => {

                            if (masterPrefabEnabled.Value && !enabledEnemyMasters.Contains(master))
                            {
                                enabledEnemyMasters.Add(master);
                            }
                            else if (!masterPrefabEnabled.Value && enabledEnemyMasters.Contains(master))
                            {
                                enabledEnemyMasters.Remove(master);
                            }

                        };
                        ModSettingsManager.AddOption(new CheckBoxOption(masterPrefabEnabled));
                        masterPrefabEnemyConfigEntries.Add(new MasterPrefabConfig(masterPrefabEnabled, master));

                        if (masterPrefabEnabled.Value && !enabledEnemyMasters.Contains(master))
                        {
                            enabledEnemyMasters.Add(master);
                        }
                        else if (!masterPrefabEnabled.Value && enabledEnemyMasters.Contains(master))
                        {
                            enabledEnemyMasters.Remove(master);
                        }
                    }


                }

            }
        }

    }
}