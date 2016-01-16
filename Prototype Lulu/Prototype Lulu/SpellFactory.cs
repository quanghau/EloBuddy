using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace Prototype_Lulu
{
    public static class SpellFactory
    {
        public static void Initialize() { }

        public static Spell.Skillshot Q { get; private set; }
        public static Spell.Skillshot Q2 { get; private set; }
        public static Spell.Targeted W { get; private set; }
        public static Spell.Targeted E { get; private set; }
        public static Spell.Targeted R { get; private set; }
        public static Spell.Targeted Ignite { get; private set; }

        static SpellFactory()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 1450, 60);
            Q.AllowedCollisionCount = int.MaxValue;
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1600, SkillShotType.Linear, 250, 1450, 60);
            Q2.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Targeted(SpellSlot.W, 650);
            E = new Spell.Targeted(SpellSlot.E, 650);
            R = new Spell.Targeted(SpellSlot.R, 900);

            if (Program.bIgnite)
            {
                Ignite = new Spell.Targeted(Program._Player.GetSpellSlotFromName("summonerdot"), 600);
                Console.WriteLine("Ignite Found! " + Ignite.Slot);
            }
        }


        public static void CastQ(AIHeroClient _t)
        {
            if (!Q.IsReady()) return;

            if (_t != null)
            {

                var pred = Q.GetPrediction(_t);

                //bool bCast = Program._Player.Distance(pred.UnitPosition) <= W.Range + Utils.TravelTimeCalculate(Program._Player.Distance(pred.UnitPosition), W.CastDelay, Q.Speed) * _t.MoveSpeed;
                if (pred.HitChance >= HitChance.High)
                    Q.Cast(pred.CastPosition);

            }
        }

        public static void CastQ2(AIHeroClient _t)
        {
            if (!Q.IsReady() || !E.IsReady()) return;
            if (_t != null)
            {

                foreach (var min in
                    EntityManager.Heroes.AllHeroes
                        .Where(x => x.IsValidTarget(E.Range) && x.Distance(Program._Player.Position) <= Q.Range)
                        .OrderBy(y => y.Distance(_t.ServerPosition)))
                {
                    if (min.Distance(_t) <= (float)Q.Range - 100)
                    {
                        // Chat.Print("Distance: " + min.Distance(_t));
                        E.Cast(min);
                        var pred = Q.GetPrediction(_t);
                        Q2.SourcePosition = Program.pixie.Position;

                        Q2.Cast(pred.CastPosition);
                        Player.CastSpell(SpellSlot.Q, Program.pixie.Position, _t.ServerPosition);
                        
                    }
                }

                foreach (var min in
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion,
                        EntityManager.UnitTeam.Both)
                        .Where(x => x.IsValidTarget(E.Range) && x.Distance(Program._Player.Position) <= Q.Range)
                        .OrderBy(y => y.Distance(_t.ServerPosition)))
                {
                    if (min.Distance(_t) <= (float)Q.Range - 100 && (min.IsAlly || !min.IsAlly && Program._Player.GetSpellDamage(min, SpellSlot.E) < min.Health))
                    {
                        // Chat.Print("Distance: " + min.Distance(_t));
                        E.Cast(min);
                        var pred = Q.GetPrediction(_t);

                        Q2.SourcePosition = Program.pixie.Position;

                        Q2.Cast(pred.CastPosition);
                        Player.CastSpell(SpellSlot.Q, Program.pixie.Position, _t.ServerPosition);
                       
                    }
                }
            }
        }

        public static void CastW()
        {
            AIHeroClient target = TargetSelector.GetTarget(SpellFactory.W.Range, DamageType.Magical);
            if (target.IsValidTarget(W.Range) && !target.IsZombie)
                W.Cast(target);
        }

        public static void CastE()
        {
            //var allies = EntityManager.Heroes.Allies.Where(x => !x.IsMe && Program._Player.IsInRange(x, SpellFactory.E.Range));
            if (Config.ReturnBoolMenu("Protector", "SupportMode") && Program._Player.CountAlliesInRange(700) >= 2) return;
            AIHeroClient target = TargetSelector.GetTarget(SpellFactory.E.Range, DamageType.Magical);
            if (target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsInvulnerable)
            {

                E.Cast(target);
            }
        }

        public static void CastR()
        {
            if (Config.ReturnBoolMenu("Combo", "UseR") && Program._Player.HealthPercent <= Config.ReturnIntMenu("Combo", "HealthR"))
                R.Cast(Program._Player);
        }

        public static void UseIgnite()
        {
            if (!SpellFactory.Ignite.IsReady()) return;
            var target = TargetSelector.GetTarget(SpellFactory.Ignite.Range, DamageType.True);
            if (target != null && !target.IsZombie && !target.IsInvulnerable)
            {   //Overkill Protection
                if (target.Health <= Utils.CalculateMaxDamage(target) + Program._Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                {
                    Ignite.Cast(target);
                }
            }
        }


    }
}
