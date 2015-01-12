using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Spell : Equipment
    {
        public float[] Parameters = new float[3];

        public uint Effect;
        public float[] Effect_Parameters = new float[3];

        public float FullCooldown;
        public float Cooldown;
        public int RandomBonusRank;

        public int Key;
        public char KeyData;

        private static int[] Keys = new int[6] { 81, 87, 69, 82, 84, 90 };

        public Spell(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Parameters[0] = Convert.ToSingle(Arguments[2]);
            Parameters[1] = Convert.ToSingle(Arguments[3]);
            Parameters[2] = Convert.ToSingle(Arguments[4]);

            Effect = Convert.ToUInt32(Arguments[5]);
            Effect_Parameters[0] = Convert.ToSingle(Arguments[6]);
            Effect_Parameters[1] = Convert.ToSingle(Arguments[7]);
            Effect_Parameters[2] = Convert.ToSingle(Arguments[8]);

            Name = Names[Effect];
            Icon = Icons[Effect];

            Cooldown = Convert.ToSingle(Arguments[9]);
            RandomBonusRank = Convert.ToInt32(Arguments[10]);

            Key = Keys[35 < Effect ? Effect / 6 - 2 : Effect / 6];
            KeyData = (char)Key;
        }

        public void Update(double ElapsedTime)
        {
            ElapsedTime /= 1000;

            if (0 < Cooldown - ElapsedTime) Cooldown -= (float)ElapsedTime;
            else Cooldown = 0;
        }

        public void Cooldown_Set(float Value)
        {
            FullCooldown = Value;
            Cooldown = Value;
        }

        public static string[] Names = new string[8*6+1]
        {
            "Lavaburst","Arcane Barrage","Chilling Blast","Poison Spit","Shadow Bolt","Smite",
            "Flamestrike","Focused Mind","Spell","Hardened Skin","Spell","Inner Focus",
            "Bind","Morph","Freeze","Hex","Silence","Shackle",
            "Blink","Spellsteal","Purging Wind","Cleanse","Invisibility","Invulnerability",
            "Spell","Spell","Spell","Spell","Spell","Spell",
            "Spell","Spell","Spell","Spell","Spell","Spell",
            "Spell","Spell","Spell","Spell","Spell","Spell",
            "Freedom","Vocalize","Spell Reflection","Lightning Shield","Void Form","Shield",
            "Teleport"
        };

        public static Texture[] Icons = new Texture[8 * 6 + 1];
        public static string[] Icon_Names = new string[8*6+1]
        {
            "spell_direct_fire","spell_direct_arcane","spell_direct_frost","spell_direct_nature","spell_direct_shadow","spell_direct_holy",
            "spell_periodic_fire","spell_periodic_arcane","spell_periodic_frost","spell_periodic_nature","spell_periodic_shadow","spell_periodic_holy",
            "spell_control_fire","spell_control_arcane","spell_control_frost","spell_control_nature","spell_control_shadow","spell_control_holy",
            "spell_evasion_fire","spell_evasion_arcane","spell_evasion_frost","spell_evasion_nature","spell_evasion_shadow","spell_evasion_holy",
            "temp","temp","temp","temp","temp","temp",
            "temp","temp","temp","temp","temp","temp",
            "spell_heal_fire","spell_heal_arcane","spell_heal_frost","spell_heal_nature","spell_heal_shadow","spell_heal_holy",
            "spell_shield_fire","spell_shield_arcane","spell_shield_frost","spell_shield_nature","spell_shield_shadow","spell_shield_holy",
            "spell_teleport"
        };

        public static string[] Tooltips = new string[8*6+1]
        {
            "Reduces healing.","Increases damage dealt.","Decreases haste.","Periodic damage.","Decreases resistance.","Decrease accuracy.",
            "Decreases clearcast chance.","Increases clearcast chance.","Decreases speed.","Increases resistance.","Heals damage dealt by this spell.","Increases haste.",
            "Roots the target.","Stuns the target.","Stuns the target, breaks on damage.","Silences the target, breaks on damage.","Silences the target.","Roots the target, breaks on damage.",
            "Teleports target.","Steals supportive mark.","Removes supportive mark.","Removes unsupportive mark.","Target becomes invisible.","Target becomes invulnerable.",
            "","","","","","",
            "","","","","","",
            "Increases speed.","Increases total energy.","Increases accuracy.","Periodic heal.","Applies periodic heal.","Increases healing done.",
            "Target is free to move.","Target is free to cast spells.","Harmful spells are reflected back.","Damages harmful spell's caster.","No unsuppotive marks are added.","Removes unsupportive marks.",
            "Teleports to your keep."
        };
    }
}
