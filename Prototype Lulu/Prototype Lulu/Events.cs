using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace Prototype_Lulu
{
    static class Events
    {
        public static void Initialize() { }


        static Events()
        {
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (!SpellFactory.W.IsReady() || sender.IsAlly) return;
            if (Config.ReturnBoolMenu("Protector", "Interrupt") && sender.IsValidTarget(SpellFactory.W.Range) && !sender.IsZombie)
                SpellFactory.W.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsAlly || !SpellFactory.W.IsReady()) return;
            if (e.End.Distance(Program._Player) <= 170 && Config.ReturnBoolMenu("Protector", "GapClose"))
            {
                SpellFactory.W.Cast(sender);
                //Console.WriteLine("(Me) Gapclose Prevented on Target: " + sender.ChampionName);
            }
            else if (Config.ReturnBoolMenu("Protector", "GapCloseAllies"))
            {
                foreach (var ally in EntityManager.Heroes.Allies.Where(x => x.IsAlly && Program._Player.IsInRange(x, SpellFactory.W.Range)))
                {
                    if (e.End.Distance(ally) <= 170)
                    {
                        SpellFactory.W.Cast(sender);
                        // Console.WriteLine("(Ally) Gapclose Prevented on Target: " + sender.ChampionName);
                    }
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || Program._Player.ManaPercent < Config.LuluAutoShieldMenu["AShieldMana"].Cast<Slider>().CurrentValue || !Config.LuluAutoShieldMenu["AShield"].Cast<CheckBox>().CurrentValue)
                return;

            foreach (var ally in EntityManager.Heroes.Allies.Where(x => Program._Player.IsInRange(x, SpellFactory.E.Range)))
            {
                if (sender is AIHeroClient && args.End.Distance(ally) <= 200)
                {
                    if (SpellProtectDB.AvoidSpells.ContainsKey(sender.BaseSkinName))
                    {
                        if (SpellProtectDB.AvoidSpells[sender.BaseSkinName].Contains(args.SData.Name))
                            if (Config.LuluAutoShieldMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                            SpellFactory.E.Cast(ally);
                    }
                }
            }
        }
    }
}
