using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;

namespace Prototype_Lulu
{
    static class Modes
    {
        public static void Initialize() { }

        static Modes()
        {
            Game.OnTick += Game_OnTick;

        }

        private static void Game_OnTick(EventArgs args)
        {
            KillSteal();
            Program.Pix();
            if (Config.ReturnBoolMenu("Protector", "ProtectorUseE") || Config.ReturnBoolMenu("Protector", "ProtectorUseR")) Protector();

            //Modes
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) LaneClear();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Flee();
        }

        private static void Combo()
        {
            if (SpellFactory.W.IsReady() && Config.ReturnBoolMenu("Combo", "UseW")) SpellFactory.CastW();
            if (SpellFactory.E.IsReady() && Config.ReturnBoolMenu("Combo", "UseE")) SpellFactory.CastE();
            if (SpellFactory.Q.IsReady() && Config.ReturnBoolMenu("Combo", "UseQ"))
            {
                var target = TargetSelector.GetTarget(1000, DamageType.Magical);
                if (target.IsValidTarget(SpellFactory.Q.Range) && !target.IsZombie && !target.IsInvulnerable)
                    SpellFactory.CastQ(target);
            }
            if (SpellFactory.R.IsReady()) SpellFactory.CastR();
            if (Program.bIgnite && SpellFactory.Ignite.IsReady() && Config.ReturnBoolMenu("Combo", "UseIgnite"))
                SpellFactory.UseIgnite();
        }

        private static void Harass()
        {
            if (Program._Player.ManaPercent <= Config.ReturnIntMenu("Harras", "HarrasManaSlider")) return;

            if (SpellFactory.E.IsReady() && Config.ReturnBoolMenu("Combo", "UseE")) SpellFactory.CastE();
            if (SpellFactory.Q.IsReady() && Config.ReturnBoolMenu("Combo", "UseQ"))
            {
                var target = TargetSelector.GetTarget(1600, DamageType.Magical);
                if (target.IsValidTarget(SpellFactory.Q.Range) && !target.IsZombie && !target.IsInvulnerable)
                    SpellFactory.CastQ(target);
                else if (target.IsValidTarget(1600) && !target.IsZombie && !target.IsInvulnerable)
                    SpellFactory.CastQ2(target);
            }

        }

        private static void LaneClear()
        {

            var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Enemy, Program._Player.Position, SpellFactory.Q.Range, false);

            foreach (var minion in minions)
            {
                if (minions.Count() >= 3)
                { 

                var loc = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, SpellFactory.Q.Width, (int)SpellFactory.Q.Range);
                SpellFactory.Q.Cast(loc.CastPosition);
                }

            }
        }

        private static void Flee()
        {
            if (SpellFactory.W.IsReady())
                SpellFactory.W.Cast(Program._Player);
        }

        private static void KillSteal()
        {
            if (!Config.ReturnBoolMenu("Combo", "KillSteal")) return;
            foreach (AIHeroClient target in EntityManager.Heroes.Enemies.Where(x => !x.IsZombie && !x.IsInvulnerable && x.IsValidTarget(1600)))
            {
                if (target.Health < Program._Player.GetSpellDamage(target, SpellSlot.Q))
                {
                    if (Config.ReturnBoolMenu("Combo", "KillStealE") && target.IsValidTarget(SpellFactory.E.Range))
                        SpellFactory.E.Cast(target);

                    if (Config.ReturnBoolMenu("Combo", "KillStealQ") && target.IsValidTarget(SpellFactory.Q.Range))
                    {
                        SpellFactory.CastQ(target);
                    }
                    else if (Config.ReturnBoolMenu("Combo", "KillStealQE") && target.IsValidTarget(SpellFactory.Q2.Range))
                    {
                        SpellFactory.CastQ2(target);
                    }
                }
            }
        }

        private static void Protector()
        {
            foreach (var ally in EntityManager.Heroes.Allies.Where(x => Program._Player.IsInRange(x, SpellFactory.E.Range) && x.HealthPercent < 95).OrderBy(x => x.HealthPercent))
            {
                if (ally.HasBuffOfType(BuffType.Poison) && Config.ReturnBoolMenu("Protector", "Poison"))
                    SpellFactory.E.Cast(Program._Player);
                if (ally.IsMe && Config.ReturnBoolMenu("Protector", ally.ChampionName + "CB"))
                {
                    if (ally.HealthPercent <= 40 && ally.CountEnemiesInRange(600) > 0)
                        SpellFactory.E.Cast(Program._Player);

                    if (!SpellFactory.E.IsReady() && ally.HealthPercent <= Config.ReturnIntMenu("Protector", ally.ChampionName + "SL"))
                        SpellFactory.R.Cast(ally);
                }
                else if (!ally.IsMe)
                {

                    if (SpellFactory.E.IsReady() && Config.ReturnBoolMenu("Protector", "ProtectorUseE") &&
                        ally.CountEnemiesInRange(600) > 0)
                    {
                        //Splitted (if) to nested to increase performance ?
                        if (Config.ReturnBoolMenu("Protector", ally.ChampionName + "CB"))
                        {
                            SpellFactory.E.Cast(ally);
                           // Console.WriteLine("Protected Ally: " + ally.ChampionName);
                        }
                    }
                    else if ((!SpellFactory.E.IsReady() || !Program._Player.IsInRange(ally,SpellFactory.E.Range)) && Config.ReturnBoolMenu("Protector", "ProtectorUseR") &&
                             Program._Player.CountEnemiesInRange(800) > 0)
                    {
                        if (Config.ReturnBoolMenu("Protector", ally.ChampionName + "CB") &&
                            ally.HealthPercent <= Config.ReturnIntMenu("Protector", ally.ChampionName + "SL"))
                        {
                            SpellFactory.R.Cast(ally);
                            //Console.WriteLine("Protected Ally: " + ally.ChampionName);
                        }
                    }
                }
            }
        }


    }
}
