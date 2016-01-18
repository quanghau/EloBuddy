using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

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
            if (SpellFactory.E.IsReady()) ProtectorE();
            if (SpellFactory.R.IsReady()) ProtectorR();

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

            if (SpellFactory.E.IsReady() && Config.ReturnBoolMenu("Harras", "HarrasE")) SpellFactory.CastE();
            if (SpellFactory.Q.IsReady() && Config.ReturnBoolMenu("Harras", "HarrasQ"))
            {
                var target = TargetSelector.GetTarget(1600, DamageType.Magical);
                if (target.IsValidTarget(SpellFactory.Q.Range) && !target.IsZombie && !target.IsInvulnerable)
                    SpellFactory.CastQ(target);
                else if (target.IsValidTarget(1600) && Config.ReturnBoolMenu("Harras", "HarrasQE") && !target.IsZombie && !target.IsInvulnerable)
                    SpellFactory.CastQ2(target);
            }

        }

        private static void LaneClear()
        {
            //if (!Config.LuluLaneclearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue || Config.LuluLaneclearMenu["LaneClearMana"].Cast<Slider>().CurrentValue <= Program._Player.ManaPercent)
             //   return;

            var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Enemy, Program._Player.Position, SpellFactory.Q.Range, false);
            foreach (var minion in minions)
            {
                if (minions.Count() >= Config.LuluLaneclearMenu["LaneclearMinions"].Cast<Slider>().CurrentValue)
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

       
        private static void ProtectorE()
        {
            foreach (var ally in EntityManager.Heroes.Allies.Where(x => Program._Player.IsInRange(x, SpellFactory.E.Range)).OrderBy(x => x.HealthPercent))
            {
                if (ally.HasBuffOfType(BuffType.Poison) && Config.ReturnBoolMenu("Protector", "Poison"))
                    SpellFactory.E.Cast(ally);
                if (ally.IsMe)
                {
                    if (ally.HealthPercent <= 50 && ally.CountEnemiesInRange(600) > 0)
                        SpellFactory.E.Cast(Program._Player);
                }
                else if (!ally.IsMe)
                {

                    if (Config._AShield && Config._AShieldMana <= Program._Player.ManaPercent &&  ally.CountEnemiesInRange(600) > 0)
                    {
                        if (ally.HealthPercent <= 85)
                            SpellFactory.E.Cast(ally);
                    }
                }
            }
        }

        private static void ProtectorR()
        {
            foreach (var ally in EntityManager.Heroes.Allies.Where(x => Program._Player.IsInRange(x, SpellFactory.R.Range)))
            {
                if (ally.IsMe)
                {
                    if (Config._AutoRLulu && Program._Player.HealthPercent <= Config._AutoRLuluHp)
                        SpellFactory.R.Cast(Program._Player);
                }
                else if (!ally.IsMe)
                {
                    if (Config._AutoR(ally.ChampionName) && ally.CountEnemiesInRange(800) > 0)
                    {
                        if (ally.HealthPercent <= Config._AutoRHp(ally.ChampionName))
                            SpellFactory.R.Cast(ally);
                    }
                }
            }
        }


    }
}
