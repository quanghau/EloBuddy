using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;

namespace Prototype_Lulu
{
    static class Utils
    {
        // public static void Initialize() { }
        public static float TravelTimeCalculate(float distance, float delay, float Speed = 0)
        {
            /*
            if (Speed != 0)
                return distance / Speed + delay;
            return delay;
            */

            return (Speed != 0) ? distance/Speed + delay : delay;

            
        }

        public static float CalculateMaxDamage(AIHeroClient t)
        {
            float dmg = 0;
            if (SpellFactory.Q.IsReady() && t.IsValidTarget(SpellFactory.Q.Range))
                dmg += Program._Player.GetSpellDamage(t, SpellSlot.Q);
            if (SpellFactory.E.IsReady() && t.IsValidTarget(SpellFactory.E.Range))
                dmg += Program._Player.GetSpellDamage(t, SpellSlot.E);

            return dmg;
        }


        public static float CalculateDamage(List<SpellSlot> spells, AIHeroClient target)
        {
            float dmg = 0;
            foreach (var spell in spells)
            {
                dmg += Program._Player.GetSpellDamage(target, spell);
            }
            return dmg;
        }
        /*
        public static void SelectSkin(int skn)
        {
            if (Program._Player.SkinId == skn) return;
            switch (skn)
            {
                case 1:
                    Program._Player.SetSkinId(0);
                    break;
                case 2:
                    Program._Player.SetSkinId(1);
                    break;
                case 3:
                    Program._Player.SetSkinId(2);
                    break;
                case 4:
                    Program._Player.SetSkinId(3);
                    break;
                case 5:
                    Program._Player.SetSkinId(4);
                    break;
            }
        }
        */



    }
}
