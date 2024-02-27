using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Unity.UI
{
    public class DoomHUD : MonoBehaviour
    {
        public Sprite[] LeftSprites;
        public Sprite[] StraightSprites;
        public Sprite[] RightSprites;
        public Sprite[] ManiacSprites;
        public Sprite[] DamageSprites;
        public Sprite SecretSprite;
        public Sprite DeadSprite;

        public enum SpritePhase
        {
            LEFT,
            STRAIGHT,
            RIGHT,
            MANIAC,
            DAMAGE,
            SECRET,
            DEAD
        }

        private SpritePhase _HeadPhase;
        public SpritePhase HeadPhase
        {
            get
            {
                return _HeadPhase;
            }
            set
            {
                _HeadPhase = value;
                UpdateSprite();
            }
        }

        public void UpdateSprite()
        {
            var dmg = (int) (4 - Mathf.RoundToInt(((float)HP / 100f) * 4));
            if (_HeadPhase == SpritePhase.LEFT)
                PlayerHead.sprite = LeftSprites[dmg];
            else if (_HeadPhase == SpritePhase.STRAIGHT)
                PlayerHead.sprite = StraightSprites[dmg];
            else if (_HeadPhase == SpritePhase.RIGHT)
                PlayerHead.sprite = RightSprites[dmg];
            else if (_HeadPhase == SpritePhase.DAMAGE)
                PlayerHead.sprite = DamageSprites[dmg];
            else if (_HeadPhase == SpritePhase.MANIAC)
                PlayerHead.sprite = ManiacSprites[dmg];
            else if (_HeadPhase == SpritePhase.DEAD)
                PlayerHead.sprite = DeadSprite;
            else if (_HeadPhase == SpritePhase.SECRET)
                PlayerHead.sprite = SecretSprite;
        }

        public Image PlayerHead;
        private int _HP;
        public int HP
        {
            get { return _HP; }
            set { _HP = value; HealthText.Text = value.ToString() + "%"; UpdateSprite(); }
        }
        private int _Armor;
        public int Armor
        {
            get { return _Armor; }
            set { _Armor = value; ArmorText.Text = value.ToString() + "%"; }
        }
        private int _BulletsAmmo = 0;
        public int BulletsAmmo
        {
            get { return _BulletsAmmo; }
            set { _BulletsAmmo = value; BulletsText.Text = value.ToString(); }
        }
        private int _ShellsAmmo = 100;
        public int ShellsAmmo
        {
            get { return _ShellsAmmo; }
            set { _ShellsAmmo = value; ShellsText.Text = value.ToString(); if (CurrentWeapon == 4) AmmoText.Text = value.ToString(); }
        }
        private int _RocketsAmmo = 0;
        public int RocketsAmmo
        {
            get { return _RocketsAmmo; }
            set { _RocketsAmmo = value; RocketsText.Text = value.ToString(); }
        }
        private int _CellsAmmo;
        public int CellsAmmo
        {
            get { return _CellsAmmo; }
            set { _CellsAmmo = value; CellsText.Text = value.ToString(); }
        }
        
        private bool _HasArms2;
        public bool HasArms2
        {
            get { return _HasArms2; }
            set { _HasArms2 = value; Arms2Text.Color = value ? ActiveColor : InactiveColor; }
        }
        private bool _HasArms3;
        public bool HasArms3
        {
            get { return _HasArms3; }
            set { _HasArms3 = value; Arms3Text.Color = value ? ActiveColor : InactiveColor; }
        }
        private bool _HasArms4;
        public bool HasArms4
        {
            get { return _HasArms4; }
            set { _HasArms4 = value; Arms4Text.Color = value ? ActiveColor : InactiveColor; }
        }
        private bool _HasArms5;
        public bool HasArms5
        {
            get { return _HasArms5; }
            set { _HasArms5 = value; Arms5Text.Color = value ? ActiveColor : InactiveColor; }
        }
        private bool _HasArms6;
        public bool HasArms6
        {
            get { return _HasArms6; }
            set { _HasArms6 = value; Arms6Text.Color = value ? ActiveColor : InactiveColor; }
        }
        private bool _HasArms7;
        public bool HasArms7
        {
            get { return _HasArms7; }
            set { _HasArms7 = value; Arms7Text.Color = value ? ActiveColor : InactiveColor; }
        }

        public Color ActiveColor;
        public Color InactiveColor;
        public DoomText Arms2Text;
        public DoomText Arms3Text;
        public DoomText Arms4Text;
        public DoomText Arms5Text;
        public DoomText Arms6Text;
        public DoomText Arms7Text;
        public DoomText HealthText;
        public DoomText AmmoText;
        public DoomText ArmorText;
        public DoomText BulletsText;
        public DoomText ShellsText;
        public DoomText RocketsText;
        public DoomText CellsText;
        public int CurrentWeapon = 4;

        public Animator ShotgunAnimator;

        void Start()
        {
            BulletsText.Text = BulletsAmmo.ToString();
            ShellsText.Text = ShellsAmmo.ToString();
            RocketsText.Text = RocketsAmmo.ToString();
            CellsText.Text = CellsAmmo.ToString();
            if (CurrentWeapon == 4)
                AmmoText.Text = ShellsAmmo.ToString();
            
            HealthText.Text = HP + "%";
            ArmorText.Text = Armor + "%";
        }

        private float ShootPhase = 0f;
        public bool Shoot()
        {
            if (ShellsAmmo > 1)
            {
                if (ShootPhase > 0f)
                    return false;
                ShotgunAnimator.SetTrigger("Shoot");
                ShellsAmmo -= 2;
                ShootPhase = 1.4f;
                return true;
            }
            return false;
        }

        void Update()
        {
            if (ShootPhase > 0f)
                ShootPhase -= Time.deltaTime;
        }
    }
}
