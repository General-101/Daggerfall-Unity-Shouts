using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Save;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace DragonBorn
{
    public class WordOfPower {
        public string name;
        public string word_A;
        public int cooldown_A;
        public string word_B;
        public int cooldown_B;
        public string word_C;
        public int cooldown_C;
        public WordOfPower(string name, string word_A, int cooldown_A, string word_B, int cooldown_B, string word_C, int cooldown_C)
        {
            this.name = name;
            this.word_A = word_A;
            this.cooldown_A = cooldown_A;
            this.word_B = word_B;
            this.cooldown_B = cooldown_B;
            this.word_C = word_C;
            this.cooldown_C = cooldown_C;
        }
    }

    public class DragonBorn : MonoBehaviour
    {
        static Mod mod;

        const float rayDistance = 10.0f;

        Camera playerCamera;
        int playerLayerMask = 0;
        GameObject player;
        public CharacterController controller;
        EnemyMotor enemyMotor;
        PlayerGroundMotor groundMotor;
        DaggerfallEntityBehaviour playerEntity;

        public List<WordOfPower> wordsOfPowerList = new List<WordOfPower>();
        WordOfPower selectedWord = null;
        int selectedIndex = 0;
        int prevSelectedIndex = 0;
        int knockback = 0;
        int speedMulti = 0;
        float chargeTimer;
        float cooldownTimer;
        float shoutTimer;
        float cooldown;
        bool chargingShout;
        RaycastHit hit;
        bool hitSomething = false;
        DaggerfallMissile ArrowMissilePrefab;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<DragonBorn>();
        }

        public void Start()
        {
            playerCamera = GameManager.Instance.MainCamera;
            playerLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
            player = GameObject.FindWithTag("Player");
            groundMotor = player.GetComponent<PlayerGroundMotor>();
            playerEntity = player.transform.GetComponent<DaggerfallEntityBehaviour>();

            wordsOfPowerList.Add(new WordOfPower("Animal Allegiance", "Raan", 50, "Mir", 60, "Tah", 70));
            wordsOfPowerList.Add(new WordOfPower("Aura Whisper", "Laas", 30, "Yah", 40, "Nir", 50));
            wordsOfPowerList.Add(new WordOfPower("Battle Fury", "Mid", 20, "Vur", 30, "Shaan", 40));
            wordsOfPowerList.Add(new WordOfPower("Become Ethereal", "Feim", 30, "Zii", 40, "Gron", 50));
            wordsOfPowerList.Add(new WordOfPower("Bend Will", "Gol", 10, "Hah", 90, "Dov", 120));
            wordsOfPowerList.Add(new WordOfPower("Call Dragon", "Od", 5, "Ah", 5, "Viing", 300));
            wordsOfPowerList.Add(new WordOfPower("Call of Valor", "Hun", 180, "Kaal", 180, "Zoor", 180));
            wordsOfPowerList.Add(new WordOfPower("Clear Skies", "Lok", 5, "Vah", 10, "Koor", 15));
            wordsOfPowerList.Add(new WordOfPower("Cyclone", "Ven", 30, "Gaar", 45, "Nos", 60));
            wordsOfPowerList.Add(new WordOfPower("Disarm", "Zun", 30, "Haal", 35, "Viik", 40));
            wordsOfPowerList.Add(new WordOfPower("Dismay", "Faas", 40, "Ru", 45, "Maar", 50));
            wordsOfPowerList.Add(new WordOfPower("Dragon Aspect", "Mul", 5, "Qah", 5, "Diiv", 5));
            wordsOfPowerList.Add(new WordOfPower("Dragonrend", "Joor", 10, "Zah", 12, "Frul", 15));
            wordsOfPowerList.Add(new WordOfPower("Drain Vitality", "Gaan", 30, "Lah", 60, "Haas", 90));
            wordsOfPowerList.Add(new WordOfPower("Elemental Fury", "Su", 30, "Grah", 40, "Dun", 50));
            wordsOfPowerList.Add(new WordOfPower("Fire Breath", "Yol", 30, "Toor", 50, "Shul", 100));
            wordsOfPowerList.Add(new WordOfPower("Frost Breath", "Fo", 30, "Krah", 50, "Diin", 100));
            wordsOfPowerList.Add(new WordOfPower("Ice Form", "Iiz", 60, "Slen", 90, "Nus", 120));
            wordsOfPowerList.Add(new WordOfPower("Kyne's Peace", "Kaan", 40, "Drem", 50, "Ov", 60));
            wordsOfPowerList.Add(new WordOfPower("Marked for Death", "Krii", 20, "Lun", 30, "Aus", 40));
            wordsOfPowerList.Add(new WordOfPower("Soul Tear", "Rii", 5, "Vaaz", 5, "Zol", 90));
            wordsOfPowerList.Add(new WordOfPower("Slow Time", "Tiid", 30, "Klo", 45, "Ul", 90));
            wordsOfPowerList.Add(new WordOfPower("Storm Call", "Strun", 300, "Bah", 480, "Qo", 600));
            wordsOfPowerList.Add(new WordOfPower("Summon Durnehviir", "Dur", 5, "Neh", 5, "Viir", 300));
            wordsOfPowerList.Add(new WordOfPower("Throw Voice", "Zul", 30, "Mey", 15, "Gut", 5));
            wordsOfPowerList.Add(new WordOfPower("Unrelenting Force", "Fus", 15, "Ro", 20, "Dah", 45));
            wordsOfPowerList.Add(new WordOfPower("Whirlwind Sprint", "Wuld", 20, "Nah", 25, "Kest", 35));
            selectedWord = wordsOfPowerList.ElementAt(0);

            mod.IsReady = true;
        }

        public void Update()
        {
            if (!chargingShout)
            {
                SetWordIndex();
                if (cooldown != 0.0f)
                {
                    cooldownTimer += Time.deltaTime;
                    if (cooldownTimer > cooldown)
                    {
                        cooldownTimer =  0.0f;
                        cooldown = 0.0f;
                        chargeTimer = 0.0f;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab) && cooldown == 0.0f)
            {
                printWordofPower(selectedWord.word_A);
                cooldown = selectedWord.cooldown_A;
                chargingShout = true;
            }
            if (chargingShout)
            {
                chargeTimer += Time.deltaTime;
                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    prevSelectedIndex = selectedIndex;
                    DoShout();
                    chargingShout = false;
                }
            }

            if (knockback != 0)
            {
                shoutTimer += Time.deltaTime;
                if (shoutTimer > 2.0f)
                {
                    knockback = 0;
                    shoutTimer = 0.0f;
                    hitSomething = false;
                }
                else
                {
                    EnemyMotor enemyMotor = hit.transform.GetComponent<EnemyMotor>();
                    DaggerfallEntityBehaviour entityBehaviour = hit.transform.GetComponent<DaggerfallEntityBehaviour>();
                    if (hitSomething && enemyMotor && entityBehaviour)
                    {
                        // Knock back enemy based on damage and enemy weight
                        EnemyEntity enemyEntity = entityBehaviour.Entity as EnemyEntity;
                        if (enemyMotor.KnockbackSpeed <= (5 / (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10)) &&
                            entityBehaviour.EntityType == EntityTypes.EnemyClass ||
                            enemyEntity.MobileEnemy.Weight > 0)
                        {
                            float enemyWeight = enemyEntity.GetWeightInClassicUnits();
                            float tenTimesDamage = knockback * 10;
                            float twoTimesDamage = knockback * 2;

                            float knockBackAmount = ((tenTimesDamage - enemyWeight) * 256) / (enemyWeight + tenTimesDamage) * twoTimesDamage;
                            float KnockbackSpeed = (tenTimesDamage / enemyWeight) * (twoTimesDamage - (knockBackAmount / 256));
                            KnockbackSpeed /= (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10);

                            if (KnockbackSpeed < (15 / (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10)))
                                KnockbackSpeed = (15 / (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10));
                            enemyMotor.KnockbackSpeed = KnockbackSpeed;
                            enemyMotor.KnockbackDirection =  playerCamera.transform.forward;
                        }
                    }
                }
            }
            else if (speedMulti != 0)
            {
                shoutTimer += Time.deltaTime;
                if (shoutTimer > 0.5f)
                {
                    speedMulti = 0;
                    shoutTimer = 0.0f;
                }
                else
                {
                    groundMotor.MoveWithMovingPlatform(playerCamera.transform.forward * speedMulti);
                }
            }
        }

        private void SetWordIndex()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if(selectedIndex + 1 <= wordsOfPowerList.Count()) 
                {
                    selectedIndex++;
                }
                else
                {
                    selectedIndex = 0;
                }
                selectedWord = wordsOfPowerList.ElementAt(selectedIndex);
                printWordofPower(selectedWord.name);

            }
            else if (Input.GetAxis("Mouse ScrollWheel") <  0)  
            {
                if(selectedIndex - 1 >= 0) 
                {
                    selectedIndex--; 
                }
                else
                {
                    selectedIndex = wordsOfPowerList.Count() - 1;
                }
                selectedWord = wordsOfPowerList.ElementAt(selectedIndex);
                printWordofPower(selectedWord.name);
            }
        }

        private void DoShout()
        {
            int level = 0;
            if (chargeTimer > 0.4f)
            {
                printWordofPower(selectedWord.word_B);
                cooldown = selectedWord.cooldown_B;
                level = 1;
            }
            if (chargeTimer > 0.8f)
            {
                printWordofPower(selectedWord.word_C);
                cooldown = selectedWord.cooldown_C;
                level = 2;
            }

            switch(selectedIndex) 
            {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
            case 15:
                FireBreath(level);
                break;
            case 16:
                FrostBreath(level);
                break;
            case 17:
                break;
            case 18:
                break;
            case 19:
                break;
            case 20:
                break;
            case 21:
                break;
            case 22:
                break;
            case 23:
                break;
            case 24:
                break;
            case 25:
                UnrelentingForce(level);
                break;
            case 26:
                WhirlwindSprint(level);
                break;
            }
        }

        private void FireBreath(int level)
        {
            EffectBundleSettings bundleSettings;
            SpellRecord.SpellRecordData spell;

            GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(14, out spell);
            GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spell, BundleTypes.Spell, out bundleSettings);

            for (int i = 0; i < bundleSettings.Effects.Length; i++)
            {
                bundleSettings.Effects[i].Settings.MagnitudeBaseMin = 99999999;
                bundleSettings.Effects[i].Settings.MagnitudeBaseMax = 99999999;
            }

            DaggerfallMissile missile = GameManager.Instance.PlayerEffectManager.InstantiateSpellMissile(bundleSettings.ElementType);
            missile.Payload = new EntityEffectBundle(bundleSettings);
            Vector3 customAimPosition = player.transform.position + playerCamera.transform.forward;
            customAimPosition.y += 40 * MeshReader.GlobalScale;
            missile.CustomAimPosition = customAimPosition;
            missile.CustomAimDirection = playerCamera.transform.forward;
        }

        private void FrostBreath(int level)
        {
            EffectBundleSettings bundleSettings;
            SpellRecord.SpellRecordData spell;

            GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(16, out spell);
            GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spell, BundleTypes.Spell, out bundleSettings);

            for (int i = 0; i < bundleSettings.Effects.Length; i++)
            {
                bundleSettings.Effects[i].Settings.MagnitudeBaseMin = 99999999;
                bundleSettings.Effects[i].Settings.MagnitudeBaseMax = 99999999;
            }

            DaggerfallMissile missile = GameManager.Instance.PlayerEffectManager.InstantiateSpellMissile(bundleSettings.ElementType);
            missile.Payload = new EntityEffectBundle(bundleSettings);
            Vector3 customAimPosition = player.transform.position + playerCamera.transform.forward;
            customAimPosition.y += 40 * MeshReader.GlobalScale;
            missile.CustomAimPosition = customAimPosition;
            missile.CustomAimDirection = playerCamera.transform.forward;
        }

        private void UnrelentingForce(int level)
        {
            switch(level) 
            {
            case 0:
                knockback = 40;
                break;
            case 1:
                knockback = 60;
                break;
            case 2:
                knockback = 80;
                break;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            hitSomething = Physics.Raycast(ray, out hit, rayDistance, playerLayerMask);
        }

        private void WhirlwindSprint(int level)
        {
            switch(level) 
            {
            case 0:
                speedMulti = 40;
                break;
            case 1:
                speedMulti = 60;
                break;
            case 2:
                speedMulti = 80;
                break;
            }
        }

        private void printWordofPower(string prompt)
        {
            DaggerfallUI.AddHUDText(prompt);
        }
    }
}
