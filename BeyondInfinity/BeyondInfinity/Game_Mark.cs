using System;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class Mark
    {
        public uint ID;

        public string Name;
        public Texture Icon;

        public uint EffectID;
        public int Stack;

        public double FullDuration;
        public double Duration;

        public bool Supportive;

        public Mark(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[1]);
            EffectID = Convert.ToUInt32(Arguments[2]);

            Name = Names[EffectID];
            Icon = Icons[EffectID];
            Stack = Convert.ToInt32(Arguments[3]);
            Duration = Convert.ToDouble(Arguments[4]);
            FullDuration = Convert.ToDouble(Arguments[5]);

            Supportive = Support[EffectID];
        }

        public void Update(double ElapsedTime)
        {
            if (0 < Duration - ElapsedTime) Duration -= ElapsedTime;
            else Duration = 0;
        }

        public void Duration_Set(double Value)
        {
            FullDuration = Value;
            Duration = Value;
        }

        public static string[] Names = new string[6 * 8 + 1]
        {
            "Burning Wounds","Mark","Mark","Mark","Mark","Mark",
            "Flame Strike","Mark","Mark","Mark","Mark","Mark",
            "Mark","Mark","Mark","Mark","Mark","Mark",
            "Mark","Mark","Mark","Mark","Mark","Mark",
            "Mark","Mark","Mark","Mark","Mark","Mark",
            "Mark","Mark","Mark","Mark","Mark","Mark",
            "Mark","Arcane Healing","Mark","Mark","Mark","Mark",
            "Lightning Shield","Mark","Mark","Mark","Mark","Mark",
            "Teleportation"
        };

        public static Texture[] Icons = new Texture[8 * 6 + 1];
        public static string[] Icon_Names = new string[8 * 6 + 1]
        {
            "mark_direct_fire","mark_direct_arcane","mark_direct_frost","mark_direct_nature","mark_direct_shadow","mark_direct_holy",
            "mark_periodic_fire","mark_periodic_arcane","mark_periodic_frost","mark_periodic_nature","mark_periodic_shadow","mark_periodic_holy",
            "mark_control_fire","spell_control_arcane","spell_control_frost","spell_control_nature","spell_control_shadow","spell_control_holy",
            "spell_evasion_fire","spell_evasion_arcane","temp","temp","spell_evasion_shadow","spell_evasion_holy",
            "temp","temp","temp","temp","temp","temp",
            "temp","temp","temp","temp","temp","temp",
            "spell_heal_fire","mark_heal_arcane","mark_heal_frost","mark_heal_nature","mark_heal_shadow","mark_heal_holy",
            "spell_shield_fire","spell_shield_arcane","spell_shield_frost","spell_shield_nature","spell_shield_shadow","spell_shield_holy",
            "spell_teleport"
        };

        public static bool[] Support = new bool[8 * 6 + 1]
            {   false,false,false,false,false,false,
                false,true,false,true,false,true,
                false,false,false,false,false,false,
                true,false,false,true,true,true,
                true,true,true,true,true,true,
                true,true,true,true,true,true,
                true,true,true,true,true,true,
                true,true,true,true,true,true,
                true};

        public static string[] Tooltips = new string[8 * 6 + 1]
        {
            "Reduced healing effects.","Increased damage received.","Haste decreased.","Periodic damage.","Resistances reduced.","Accuracy decreased.",
            "Clearcast chance reduced.","Clearcast chance increased.","Movement slowed.","Resistances increased.","Periodic damage, which heals the caster.","Haste increased.",
            "Unable to move.","Unable to move, and cast spells.","Unable to move, and cast spells.","Unable to cast spells.","Unable to cast spells.","Unable to move.",
            "","","","","Others can't see you.","Immune to damage.",
            "","","","","","",
            "","","","","","",
            "Increased speed.","Increased total energy.","Increased accuracy.","Periodic healing.","Periodic healing.","Increased healing received.",
            "Unstoppable.","Can't be silenced.","Reflects harmful spells back.","Harmful spell's caster is damaged.","No unsupportive marks added.","Increased total energy.",
            "Teleporting to your keep.."
        };
    }
}
