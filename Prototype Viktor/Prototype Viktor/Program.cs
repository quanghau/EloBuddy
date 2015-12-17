using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpDX;
using System.Reflection;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace Protype_Viktor
{
    class Program
    {
        #region Variables
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }
        private static Spell.Targeted Q;
        private static Spell.Skillshot W, E, R;
        private static int ERange = 700;
        private static int EMaxRange = 1225;
        private static Vector3 startPos;
        private static Menu ViktorMenu;
        private static Menu ViktorComboMenu, ViktorLaneClearMenu, ViktorMiscMenu, ViktorDrawMenu;
        private static string version = "0.0.0.4";
        #endregion

        #region PropertyChecks
        private static bool _ViktorQ
        {
            get { return ViktorComboMenu["UseQ"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _ViktorW
        {
            get { return ViktorComboMenu["UseW"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _ViktorE
        {
            get { return ViktorComboMenu["UseE"].Cast<CheckBox>().CurrentValue; }

        }
        private static bool _ViktorR
        {
            get { return ViktorComboMenu["UseR"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _CheckR
        {
            get { return ViktorComboMenu["CheckR"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _AutoFollowR
        {
            get { return ViktorComboMenu["FollowR"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _KillSteal
        {
            get { return ViktorComboMenu["EnableKS"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _KsQ
        {
            get { return ViktorComboMenu["KsQ"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _KsE
        {
            get { return ViktorComboMenu["KsE"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _GapCloser
        {
            get { return ViktorMiscMenu["Gapclose"].Cast<CheckBox>().CurrentValue; }

        }
        private static bool _AntiRengar
        {
            get { return ViktorMiscMenu["AntiRango"].Cast<CheckBox>().CurrentValue; }

        }
        private static bool _LaneClearE
        {
            get { return ViktorLaneClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue; }

        }
        private static bool _LaneClearQ
        {
            get { return ViktorLaneClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue; }

        }
        private static int _LaneClearMana
        {
            get { return ViktorLaneClearMenu["LaneClearMana"].Cast<Slider>().CurrentValue; }
        }
        private static int _MinMinions
        {
            get { return ViktorLaneClearMenu["MinMinions"].Cast<Slider>().CurrentValue; }
        }
        private static bool _DrawQ
        {
            get { return ViktorDrawMenu["DrawQ"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _DrawW
        {
            get { return ViktorDrawMenu["DrawW"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _DrawE
        {
            get { return ViktorDrawMenu["DrawE"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _DrawR
        {
            get { return ViktorDrawMenu["DrawR"].Cast<CheckBox>().CurrentValue; }
        }

        private static int _MinW
        {
            get { return ViktorComboMenu["MinW"].Cast<Slider>().CurrentValue; }
        }
        private static int _RTicks
        {
            get { return ViktorComboMenu["RTicks"].Cast<Slider>().CurrentValue; }
        }

        private static HitChance PredictionRate
        {
            get
            {
                if (ViktorComboMenu["PredictionRate"].Cast<Slider>().CurrentValue <= 1)
                    return HitChance.Low;
                else if (ViktorComboMenu["PredictionRate"].Cast<Slider>().CurrentValue == 2)
                    return HitChance.Medium;
                else
                    return HitChance.High;
            }
        }
        #endregion

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        #region Events
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Viktor")
                return;

            LoadSkills();
            LoadMenu();

            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;

            Chat.Print("Prototype Viktor " + version + " Loaded!");
            Console.WriteLine("Prototype Viktor " + version + " Loaded!");

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || _Player.HasBuff("Recall")) return;

            if (_AutoFollowR) AutoFollowR();

            KillSecure();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                Combo();

            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                LaneClear();

        }
        #endregion

        #region SkillsInit
        private static void LoadSkills()
        {
            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular, 500, int.MaxValue, 300);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Skillshot(SpellSlot.E, 525, SkillShotType.Linear, 250, int.MaxValue, 100);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, 250, int.MaxValue, 450);
            R.AllowedCollisionCount = int.MaxValue;
        }
        #endregion

        #region Drawings
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_DrawQ && Q.IsReady())
                Circle.Draw(Color.Aqua, Q.Range, _Player.Position);
            if (_DrawW && W.IsReady())
                Circle.Draw(Color.Brown, W.Range, _Player.Position);
            if (_DrawE && E.IsReady())
                Circle.Draw(Color.HotPink, EMaxRange, _Player.Position);
            if (_DrawR && R.IsReady())
                Circle.Draw(Color.Gray, R.Range, _Player.Position);
            /*
            if (E.IsReady())
                Circle.Draw(Color.HotPink, 6, 40, startPos);
            */
        }
        #endregion

        #region Menu
        private static void LoadMenu()
        {
            ViktorMenu = MainMenu.AddMenu("Prototype Viktor", "Viktor");
            ViktorMenu.AddGroupLabel("[Prototype Viktor Info]");
            ViktorMenu.AddLabel("Beta Viktor Addon. Made by Tear for EloBuddy!");
            ViktorMenu.AddLabel("Before you play, please adjust your settings in Menu.");
            ViktorMenu.AddLabel("Please, report any bugs in forums.");
            ViktorMenu.AddLabel("Work In Progress:");
            ViktorMenu.AddLabel("-Auto Ignite in Combo");
            ViktorMenu.AddLabel("-Auto Cleanse");
            ViktorMenu.AddLabel("-Improve Damage Calculations");

            ViktorComboMenu = ViktorMenu.AddSubMenu("Combo", "Combo");
            ViktorComboMenu.AddLabel("[Combo Settings:]");
            ViktorComboMenu.Add("UseQ", new CheckBox("Use Q"));
            ViktorComboMenu.Add("UseW", new CheckBox("Use W",false));
            ViktorComboMenu.Add("UseE", new CheckBox("Use E"));
            ViktorComboMenu.Add("UseR", new CheckBox("Use R"));
            ViktorComboMenu.Add("FollowR", new CheckBox("Auto Follow R"));
            ViktorComboMenu.Add("CheckR", new CheckBox("Cast R only if enemy is killable with Combo"));
            ViktorComboMenu.AddSeparator(10);
            ViktorComboMenu.AddLabel("[KillSteal Options:]");
            ViktorComboMenu.Add("EnableKS", new CheckBox("Enable KillSteal"));
            ViktorComboMenu.Add("KsQ", new CheckBox("KillSteal with Q"));
            ViktorComboMenu.Add("KsE", new CheckBox("KillSteal with E"));
            ViktorComboMenu.AddSeparator(10);
            ViktorComboMenu.Add("PredictionRate", new Slider("Prediction Rate:", 3, 1, 3));
            ViktorComboMenu.Add("MinW", new Slider("Mininum Enemies for W:", 2, 1, 5));
            ViktorComboMenu.Add("RTicks", new Slider("R Ticks (per 0.5s) to calcuate in Damage Output:", 10, 1, 14));

            ViktorLaneClearMenu = ViktorMenu.AddSubMenu("Lane Clear", "LaneClear");
            ViktorLaneClearMenu.Add("LaneClearQ", new CheckBox("Use Q in Lane Clear "));
            ViktorLaneClearMenu.Add("LaneClearE", new CheckBox("Use E in Lane Clear"));
            ViktorLaneClearMenu.Add("LaneClearMana", new Slider("Mana (%):", 40, 0, 100));
            ViktorLaneClearMenu.Add("MinMinions", new Slider("Minimum Minions to use E", 3, 1, 10));

            ViktorDrawMenu = ViktorMenu.AddSubMenu("Drawings", "Drawings");
            ViktorDrawMenu.Add("DrawQ", new CheckBox("Draw Q"));
            ViktorDrawMenu.Add("DrawW", new CheckBox("Draw W"));
            ViktorDrawMenu.Add("DrawE", new CheckBox("Draw E"));
            ViktorDrawMenu.Add("DrawR", new CheckBox("Draw R"));

            ViktorMiscMenu = ViktorMenu.AddSubMenu("Misc", "Misc");
            ViktorMiscMenu.AddLabel("[Misc Settings]");
            ViktorMiscMenu.Add("Gapclose", new CheckBox("Anti GapCloser (W)"));
            ViktorMiscMenu.Add("AntiRango", new CheckBox("Anti Rengar (W)"));

            ViktorMiscMenu.AddSeparator(30);
            ViktorMiscMenu.AddLabel("* Anti Gapcloser/Anti Rengar will cast (W) on Viktor's position");
        }
        #endregion

        private static void Combo()
        {
            if (W.IsReady() && _ViktorW) CastW();
            if (Q.IsReady() && _ViktorQ) CastQ();
            if (E.IsReady() && _ViktorE) Core.DelayAction(CastE, 200);
            if (R.IsReady() && _ViktorR) CastR();
        }

        private static void LaneClear()
        {
            if (_LaneClearMana <= _Player.ManaPercent)
            {
                var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Enemy, _Player.Position, EMaxRange, false);

                foreach (var minion in minions)
                {
                    if (_LaneClearE && minions.Count() >= _MinMinions )
                    { 
                    var loc = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, E.Width, EMaxRange);
                    Player.CastSpell(SpellSlot.E, loc.CastPosition, minion.ServerPosition);
                       // Chat.Print("Minions in Lane: " + minions.Count() + "Mininum minions to cast E: " + _MinMinions);
                    }
                    if (_LaneClearQ && minion.Health < _Player.GetSpellDamage(minion,SpellSlot.Q) + CalculateAADmg())
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        // Credits to Fluxy.
        //Needs further testing.
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            AIHeroClient rengar = EntityManager.Heroes.Enemies.FirstOrDefault(x => x.Hero == Champion.Rengar);
            if (_AntiRengar && rengar != null && sender.Name == "Rengar_LeapSound.troy" && _Player.Distance(Player.Instance.Position) <= W.Range)
            {
                W.Cast(_Player);
            }
        }

        //Needs further testing
        //So far it tested with Xin Zhao (E), Alistar (W)
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (_GapCloser && (e.End.Distance(_Player) <= 170) && sender.IsValidTarget())
                W.Cast(_Player);
        }

        private static void KillSecure()
        {
            if (!_KillSteal) return;
            foreach (AIHeroClient target in EntityManager.Heroes.Enemies)
            {
                if (_KsE && target.IsValidTarget(EMaxRange) && target.Health < _Player.GetSpellDamage(target, SpellSlot.E))
                {
                    CastE();
                }
                else if (_KsQ && target.IsValidTarget(Q.Range) && target.Health < _Player.GetSpellDamage(target, SpellSlot.E) + _Player.GetSpellDamage(target, SpellSlot.Q) + CalculateAADmg())
                {
                    CastE();
                    Core.DelayAction(CastQ, 50);
                }
            }
        }

        private static void AutoFollowR()
        {
            if (_Player.HasBuff("Glory"))
            {
                var target = TargetSelector.GetTarget(1000, DamageType.Magical);
                if (target != null)
                    R.Cast(target);
            }
        }

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target != null && Q.IsInRange(target))
            {
                Q.Cast(target);
            }
        }

        private static void CastW()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target !=null )
            {
                if (target.CountEnemiesInRange(W.Range) >= _MinW)

                W.Cast(target);
            }
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(EMaxRange, DamageType.Magical);
            if (target != null)
            {
                if (_Player.ServerPosition.Distance(target.ServerPosition) < E.Range)
                {
                    E.SourcePosition = target.ServerPosition;
                    var prediction = E.GetPrediction(target);
                    if (prediction.HitChance >= PredictionRate)
                    {
                        Player.CastSpell(SpellSlot.E, prediction.UnitPosition, target.ServerPosition);
                    }
                }
                else if (_Player.ServerPosition.Distance(target.ServerPosition) < EMaxRange)
                {
                    startPos = _Player.ServerPosition.To2D().Extend(target.ServerPosition, E.Range).To3D();

                    var prediction = E.GetPrediction(target);
                    E.SourcePosition = startPos;
                    if (prediction.HitChance >= PredictionRate)
                    {
                        Player.CastSpell(SpellSlot.E, prediction.UnitPosition, startPos);
                    }
                }
            }
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (target != null)
            {
                var prediction = E.GetPrediction(target);
                var predictDmg = PredictDamage(target);
                //Chat.Print("Target Health: " + target.Health + "Predict Dmg: " + predictDmg);
                if (target.HealthPercent > 5 && _CheckR  && prediction.HitChance >= HitChance.High)
                {
                    if (target.Health <= predictDmg)
                    R.Cast(target);
                }
                else if (target.HealthPercent > 5 && !_CheckR && prediction.HitChance >= HitChance.High)
                {
                    R.Cast(target);
                }
            }
        }

        private static float PredictDamage(AIHeroClient t)
        {
            float dmg = 0f;
            if (_ViktorQ && Q.IsReady() && _Player.IsInAutoAttackRange(t) )
            {
                dmg += _Player.GetSpellDamage(t, SpellSlot.Q);
                dmg += (float) CalculateAADmg();
            }

            if (_ViktorE && E.IsReady() && _Player.ServerPosition.Distance(t.ServerPosition) < EMaxRange)
            {
                dmg += _Player.GetSpellDamage(t, SpellSlot.E);
            }

            if (_ViktorR && R.IsReady() && R.IsInRange(t))
            {
                dmg += _Player.GetSpellDamage(t, SpellSlot.R);
                dmg += (float) CalculateRTickDmg(t, _RTicks);
            }

            return dmg;    
        }


        private static double CalculateAADmg()
        {
            var AAdmg = new Double[] { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };

            return (double) AAdmg[_Player.Level >= 18 ? 18 - 1 : _Player.Level - 1] + _Player.TotalMagicalDamage * 0.5 + _Player.TotalAttackDamage;
        }


        private static double CalculateRTickDmg(AIHeroClient t, int ticks)
        {
            if (R.Level == 0) return 0;
            double dmg = 0f; ;
            if (R.Level == 1)
                dmg += 15 + _Player.TotalMagicalDamage * 0.10;
            else if (R.Level == 2)
                dmg += 30 + _Player.TotalMagicalDamage * 0.10;
            else
                dmg += 45 + _Player.TotalMagicalDamage * 0.10;

            return dmg;
        }


    }
}
