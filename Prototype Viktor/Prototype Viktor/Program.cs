using System;
using System.Linq;
using System.Reflection;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Prototype_Viktor
{
    internal class Program
    {
        #region Variables

        public static AIHeroClient _Player { get { return ObjectManager.Player; } }


        private static Spell.Targeted Q, Ignite;
        private static SpellSlot IgniteSlot;
        private static bool bIgnite;
        private static Spell.Skillshot W, E, R;
        public static int EMaxRange = 1225;
        private static int _tick;
        private static Vector3 startPos;
        private static Menu ViktorMenu;

        private static Menu
            ViktorComboMenu,
            ViktorHarassMenu,
            ViktorLaneClearMenu,
            ViktorMiscMenu,
            ViktorDrawMenu,
            ViktorRMenu;

        private static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #endregion
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        #region SkillsInit

        private static void LoadSkills()
        {
            Q = new Spell.Targeted(SpellSlot.Q, 670);
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
            if (ViktorDrawMenu["DisableDraws"].Cast<CheckBox>().CurrentValue) return;

            if (_DrawQ && Q.IsReady())
                Circle.Draw(Color.Aqua, Q.Range, _Player.Position);
            if (_DrawW && W.IsReady())
                Circle.Draw(Color.Brown, W.Range, _Player.Position);
            if (_DrawE && E.IsReady())
                Circle.Draw(Color.HotPink, EMaxRange, _Player.Position);
            if (_DrawR && R.IsReady())
                Circle.Draw(Color.Gray, R.Range, _Player.Position);
        }

        #endregion

        #region Menu

        private static void LoadMenu()
        {
            ViktorMenu = MainMenu.AddMenu("Prototype Viktor", "Viktor");

            ViktorComboMenu = ViktorMenu.AddSubMenu("Combo", "Combo");
            ViktorComboMenu.AddLabel("[Combo Settings]");
            ViktorComboMenu.Add("UseQ", new CheckBox("Use Q"));
            ViktorComboMenu.Add("UseW", new CheckBox("Use W"));
            ViktorComboMenu.Add("UseE", new CheckBox("Use E"));
            ViktorComboMenu.Add("UseR", new CheckBox("Use R"));
            ViktorComboMenu.Add("UseIgnite", new CheckBox("Use Ignite"));
            ViktorComboMenu.Add("ComboMode", new ComboBox("Combo Mode", 1, "Safe :: (WQER)", "Burst :: (EQRW)"));
            ViktorComboMenu.AddSeparator(10);
            ViktorComboMenu.AddLabel("[Misc Combo Settings]");
            ViktorComboMenu.Add("MinW", new Slider("Mininum Enemies:{0} to cast W:", 2, 1, 5));

            ViktorComboMenu.AddSeparator(10);
            ViktorComboMenu.AddLabel("[KillSteal Options]");
            ViktorComboMenu.Add("EnableKS", new CheckBox("Enable KillSteal"));
            ViktorComboMenu.Add("KsQ", new CheckBox("KillSteal with Q"));
            ViktorComboMenu.Add("KsE", new CheckBox("KillSteal with E"));

            ViktorRMenu = ViktorMenu.AddSubMenu("Ulti(R) Options", "ROptions");
            ViktorRMenu.Add("CheckR", new CheckBox("Cast Ulti(R) only when Enemy is killable"));
            ViktorRMenu.AddSeparator(10);
            ViktorRMenu.AddLabel("[Ulti(R) Settings]");
            ViktorRMenu.Add("FollowOption", new ComboBox("Follow Options", 0, "Enemies & Viktor", "Only Enemies", "Disabled"));
            ViktorRMenu.Add("MinEnemiesR", new Slider("Minimum Enemies{0} to cast R:", 1, 1, 5));

            ViktorRMenu.AddSeparator(10);
            ViktorRMenu.AddLabel("[Advanced TeamFight Logic]");
            ViktorRMenu.Add("AdvancedTeamFight", new CheckBox("Enable TeamFight Ulti(R) cast", false));
            ViktorRMenu.Add("MinTeamFights", new Slider("Minimum Enemies(x) in Range to Cast (R):", 3, 2, 5));
            ViktorRMenu.AddLabel("This option will override Damage ouput calculations and Radius checks for Viktor's Ulti(R)");
            ViktorRMenu.AddLabel("It will cast ulti, if (x) number of enemies are within Viktor's range.");
            ViktorRMenu.AddSeparator(10);
            ViktorRMenu.Add("RTicks", new Slider("R Ticks (per 0.5s) to calculate in Damage Output:", 10, 1, 14));



            ViktorHarassMenu = ViktorMenu.AddSubMenu("Harass", "Harass");
            ViktorHarassMenu.AddLabel("[Harass Settings]");
            ViktorHarassMenu.Add("HarassQ", new CheckBox("Use Q"));
            ViktorHarassMenu.Add("HarassE", new CheckBox("Use E"));
            ViktorHarassMenu.AddSeparator(10);
            ViktorHarassMenu.AddLabel("[Harass Mana Settings]");
            ViktorHarassMenu.Add("HarassManaQ", new Slider("Minimum Mana to use Q in Harras Mode: {0}%", 40, 1, 100));
            ViktorHarassMenu.Add("HarassManaE", new Slider("Minimum Mana to use E in Harras Mode: {0}%", 40, 1, 100));

            ViktorLaneClearMenu = ViktorMenu.AddSubMenu("Lane Clear", "LaneClear");
            ViktorLaneClearMenu.AddLabel("[LaneClear Settings]");
            ViktorLaneClearMenu.Add("LaneClearQ", new CheckBox("Use Q"));
            ViktorLaneClearMenu.Add("LaneClearE", new CheckBox("Use E "));
            ViktorLaneClearMenu.AddSeparator(5);
            ViktorLaneClearMenu.Add("LaneClearManaQ", new Slider("Minimum mana for LaneClear Mode (%):", 40, 0, 100));
            ViktorLaneClearMenu.Add("LaneClearManaE", new Slider("Minimum mana for LaneClear Mode (%):", 40, 0, 100));
            ViktorLaneClearMenu.Add("MinMinions", new Slider("Minimum Minions(x) to use E in LaneClear Mode:", 3, 1, 10));

            ViktorDrawMenu = ViktorMenu.AddSubMenu("Drawings", "Drawings");
            ViktorDrawMenu.AddLabel("[Drawings Settings]");
            ViktorDrawMenu.Add("DisableDraws", new CheckBox("Disable All Drawings", false));
            ViktorDrawMenu.AddSeparator(10);
            ViktorDrawMenu.AddLabel("[Skill Settings]");
            ViktorDrawMenu.Add("DrawQ", new CheckBox("Draw Q"));
            ViktorDrawMenu.Add("DrawW", new CheckBox("Draw W"));
            ViktorDrawMenu.Add("DrawE", new CheckBox("Draw E"));
            ViktorDrawMenu.Add("DrawR", new CheckBox("Draw R"));

            ViktorMiscMenu = ViktorMenu.AddSubMenu("Misc", "Misc");
            ViktorMiscMenu.Add("RTickSlider", new Slider("R Follow Speed (ms):", 50, 10, 100));
            ViktorMiscMenu.AddLabel("*Lower is better, 50 is optimal.");
            ViktorMiscMenu.AddSeparator(10);
            ViktorMiscMenu.AddLabel("[Gapcloser Settings]");
            ViktorMiscMenu.Add("Gapclose", new CheckBox("Anti GapCloser (W)"));
            ViktorMiscMenu.AddLabel("Anti Gapcloser will cast (W) on Viktor's position");
            /*
            ViktorMiscMenu.AddLabel("[Skin Selector]");
            ViktorMiscMenu.Add("SkinChanger", new Slider("Skin ID:", 1, 1, 4));
            ViktorMiscMenu.AddSeparator(10);
           */

        }

        #endregion

        #region Events

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.ChampionName != "Viktor") return;

            IgniteSlot = _Player.GetSpellSlotFromName("summonerdot");
            if (IgniteSlot != SpellSlot.Unknown)
            {
                Console.WriteLine("Ignite Spell found on slot: " + IgniteSlot);
                bIgnite = true;
                Ignite = new Spell.Targeted(IgniteSlot, 600);
            }

            LoadSkills();
            LoadMenu();

            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
            Drawing.OnDraw += Drawing_OnDraw;


            Chat.Print("Prototype Viktor " + version + " Loaded!");
            Console.WriteLine("Prototype Viktor " + version + " Loaded! Last Patch Update: 6.20");
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (Q.IsReady() && _Player.GetSpellDamage(target,SpellSlot.Q) >= target.Health)
            {
                Q.Cast(target);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || _Player.HasBuff("Recall")) return;

            if (_AutoFollowR != 2)
            {
                if (R.Name != "ViktorChaosStorm" && Core.GameTickCount - _tick >= _RTickSlider)
                {
                   var stormT = TargetSelector.GetTarget(1750, DamageType.Magical);
                   if (stormT != null)
                    {
                      R.Cast(stormT);
                        _tick = Core.GameTickCount;
                    }
                  if (stormT == null && _AutoFollowR == 0)
                   {
                    R.Cast(_Player);
                        _tick = Core.GameTickCount;
                    }

                }
            }


                KillSecure();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (_ComboMode == 0) SafeCombo();
                else BurstCombo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) LaneClearBeta();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClearEBeta();
                JungleClearQBeta();
            }
           // if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) QLastHitBeta();
        }

  

        #endregion

        //(WQER)
        private static void SafeCombo()
        {
            if (W.IsReady() && _ViktorW) CastW();
            if (Q.IsReady() && _ViktorQ) CastQ();
            if (E.IsReady() && _ViktorE) CastE();
            if (R.IsReady() && _ViktorR) CastR();
            if (bIgnite && _UseIgnite) UseIgnite();
        }

        //(EQRW)
        private static void BurstCombo()
        {
            if (E.IsReady() && _ViktorE) CastE();
            if (Q.IsReady() && _ViktorQ) CastQ();
            if (R.IsReady() && _ViktorR) CastR();
            if (W.IsReady() && _ViktorW) CastW();
            if (bIgnite && _UseIgnite) UseIgnite();
        }


        private static void Harass()
        {

            if (E.IsReady() && _HarassE && _Player.ManaPercent >= _HarassManaE) CastE();
            if (Q.IsReady() && _HarassQ && _Player.ManaPercent >= _HarassManaQ) Core.DelayAction(CastQ, 40);

        }


        private static void LaneClearBeta()
        {

            var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Enemy, _Player.Position, EMaxRange, false);
            foreach (var minion in minions)
            {
                if (E.IsReady() && _LaneClearE && _Player.ManaPercent >= _LaneClearManaE)
                {
                    var farmLoc = Laser.GetBestLaserFarmLocation(false);
                    if (farmLoc.MinionsHit >= _MinMinions)
                    {
                        Player.CastSpell(SpellSlot.E, farmLoc.Position2.To3D(), farmLoc.Position1.To3D());
                    }
                }
                if (Q.IsReady() && _LaneClearQ && _Player.ManaPercent >= _LaneClearManaQ)
                {
                    if (minion.BaseSkinName.ToLower().Contains("siege") && Q.IsInRange(minion))
                    {
                        Q.Cast(minion);
                        Orbwalker.ForcedTarget = minion;
                    }
                    else
                    {
                        var mins = minions.OrderByDescending(x => x.HealthPercent);
                        Q.Cast(mins.FirstOrDefault());
                    }
                }
            }
        }


        private static void JungleClearEBeta()
        {
            if (!E.IsReady()) return;

            var startPos = new Vector2(0, 0);
            var endPos = new Vector2(0, 0);
            foreach (
                var minion in
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.Position, 525)
                        .Where(x => x.Distance(_Player) <= 1200))
            {
                var farmLoc = Laser.LaserLocation(minion.Position.To2D(),
                    (from mnion in
                        EntityManager.MinionsAndMonsters.GetJungleMonsters(minion.Position,
                            525)
                     select mnion.Position.To2D()).ToList(), E.Width, 525);
                startPos = minion.Position.To2D();
                endPos = farmLoc;
            }
            if (startPos.Distance(_Player.ServerPosition) <= 525)
            {
                Player.CastSpell(SpellSlot.E, endPos.To3D(), startPos.To3D());
            }
        }

        private static void JungleClearQBeta()
        {
            if (!Q.IsReady()) return;
            foreach (
                var minion in
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.Position, 525)
                        .Where(x => x.Distance(_Player) <= 670).OrderByDescending(x => x.HealthPercent))
            {
                Core.DelayAction(() => Q.Cast(minion), 35);
            }
        }

        public static void QLastHitBeta()
        {
            if (!Q.IsReady()) return;

            var min = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                _Player.Position, Q.Range)
                .Where(x => x.Health <= _Player.GetAutoAttackDamage(x)).ToList();
            if (min.Count() > 1)
            {
                var castedMinion = min.OrderBy(x => x.HealthPercent).FirstOrDefault();
                var secMinion = min.OrderBy(x => x.HealthPercent).FirstOrDefault();
                Q.Cast(castedMinion);
                Orbwalker.ForcedTarget = secMinion;
            }
        }


        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsAlly || !_GapCloser) return;
            if (e.End.Distance(_Player) <= 300)
                W.Cast(_Player);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            /*
            if (sender.IsMe && args.SData.Name.ToLower().Contains("viktorpowertransferreturn"))
                Core.DelayAction(Orbwalker.ResetAutoAttack, 100);
                */
            if (sender.IsMe && args.Slot == SpellSlot.Q)
            {
                Core.DelayAction(Orbwalker.ResetAutoAttack,60);
            }

        }

        private static void KillSecure()
        {
            if (!_KillSteal) return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(1250) && !x.IsZombie))
            {
                if (_KsE && target.HealthPercent <= 15)
                {
                    CastE();
                }

                if (_KsQ && target.IsValidTarget(Q.Range) && target.Health < _Player.GetSpellDamage(target, SpellSlot.Q) + CalculateAADmg())
                {     
                    CastQ();
                }
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
            if (target != null && target.CountEnemiesInRange(W.Width) >= _MinW)
            {
                W.Cast(target);
            }
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(EMaxRange, DamageType.Magical);
            if (target != null && target.IsEnemy)
            {
                if (_Player.ServerPosition.Distance(target.ServerPosition) < E.Range)
                {
                    E.SourcePosition = target.ServerPosition;
                    var prediction = E.GetPrediction(target);
                    E.CastStartToEnd(prediction.CastPosition, target.ServerPosition);
                    // E.CastStartToEnd(target.ServerPosition,_Player.ServerPosition);
                    // E.CastStartToEnd(target.Position,_Player.Position); //not working

                }
                else if (_Player.ServerPosition.Distance(target.ServerPosition) < EMaxRange)
                {
                    startPos = _Player.ServerPosition.To2D().Extend(target.ServerPosition, E.Range).To3D();

                    var prediction = E.GetPrediction(target);
                    E.SourcePosition = startPos;
                    if (prediction.HitChance >= HitChance.Medium)
                    {
                        Player.CastSpell(SpellSlot.E, prediction.UnitPosition, startPos);
                    }
                }
            }
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (target != null && target.IsEnemy && !target.IsZombie && target.CountEnemiesInRange(R.Width) >= _MinEnemiesR && R.Name == "ViktorChaosStorm")
            {
                var predictDmg = PredictDamage(target);

                if (target.HealthPercent > 5 && _CheckR)
                {
                    if (target.Health <= predictDmg * 1.15)
                        R.Cast(target);
                }
                else if (target.HealthPercent > 5 && !_CheckR)
                {
                    R.Cast(target);
                }
            }
            else if (ViktorRMenu["AdvancedTeamFight"].Cast<CheckBox>().CurrentValue)
            {
                if (_Player.CountEnemiesInRange(1200) >= ViktorRMenu["MinTeamFights"].Cast<Slider>().CurrentValue && R.Name == "ViktorChaosStorm")
                {
                    var targets = EntityManager.Heroes.Enemies.OrderBy(x => x.Health - PredictDamage(x)).Where(x => x.IsValidTarget(1200) && !x.IsZombie);

                    foreach (var ultiT in targets)
                    {
                        R.Cast(ultiT);
                    }
                }
            }
        }

        private static void UseIgnite()
        {
            if (!Ignite.IsReady()) return;
            var target = TargetSelector.GetTarget(Ignite.Range, DamageType.True);
            if (target != null && !target.IsZombie && !target.IsInvulnerable)
            {
                //Overkill Protection
                if (target.Health > PredictDamage(target) &&
                    target.Health <=
                    PredictDamage(target) + _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                {
                    Ignite.Cast(target);
                }
            }
        }

        private static float PredictDamage(AIHeroClient t)
        {
            var dmg = 0f;
            if (_ViktorQ && Q.IsReady() && _Player.IsInAutoAttackRange(t))
            {
                dmg += _Player.GetSpellDamage(t, SpellSlot.Q);
                dmg += (float)CalculateAADmg();
            }

            if (_ViktorE && E.IsReady() && _Player.ServerPosition.Distance(t.ServerPosition) <= EMaxRange)
            {
                dmg += _Player.GetSpellDamage(t, SpellSlot.E);
            }

            if (_ViktorR && R.IsReady() && R.IsInRange(t))
            {
                dmg += _Player.GetSpellDamage(t, SpellSlot.R);
                dmg += (float)CalculateRTickDmg(t, _RTicks);
            }
            return dmg;
        }

        private static double CalculateAADmg()
        {
            double[] AAdmg = { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };

            return AAdmg[_Player.Level - 1] + _Player.TotalMagicalDamage * 0.5 + _Player.TotalAttackDamage;
        }

        private static double CalculateRTickDmg(AIHeroClient t, int ticks)
        {
            double dmg = 0;
            switch (R.Level)
            {
                case 0:
                    return dmg = 0;
                case 1:
                    return dmg += (15 + _Player.TotalMagicalDamage * 0.10) * ticks;
                case 2:
                    return dmg += (30 + _Player.TotalMagicalDamage * 0.10) * ticks;
                case 3:
                    return dmg += (45 + _Player.TotalMagicalDamage * 0.10) * ticks;
            }
            return dmg;
        }


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

        private static bool _UseIgnite
        {
            get { return ViktorComboMenu["UseIgnite"].Cast<CheckBox>().CurrentValue; }
        }

        private static int _ComboMode
        {
            get { return ViktorComboMenu["ComboMode"].Cast<ComboBox>().CurrentValue; }
        }

        private static bool _CheckR
        {
            get { return ViktorRMenu["CheckR"].Cast<CheckBox>().CurrentValue; }
        }

        private static int _AutoFollowR
        {
            get { return ViktorRMenu["FollowOption"].Cast<ComboBox>().CurrentValue; }
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

        private static bool _HarassQ
        {
            get { return ViktorHarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _HarassE
        {
            get { return ViktorHarassMenu["HarassE"].Cast<CheckBox>().CurrentValue; }
        }

        private static int _HarassManaQ
        {
            get { return ViktorHarassMenu["HarassManaQ"].Cast<Slider>().CurrentValue; }
        }

        private static int _HarassManaE
        {
            get { return ViktorHarassMenu["HarassManaE"].Cast<Slider>().CurrentValue; }
        }

        private static bool _GapCloser
        {
            get { return ViktorMiscMenu["Gapclose"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _LaneClearE
        {
            get { return ViktorLaneClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool _LaneClearQ
        {
            get { return ViktorLaneClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue; }
        }

        private static int _LaneClearManaQ
        {
            get { return ViktorLaneClearMenu["LaneClearManaQ"].Cast<Slider>().CurrentValue; }
        }
        private static int _LaneClearManaE
        {
            get { return ViktorLaneClearMenu["LaneClearManaE"].Cast<Slider>().CurrentValue; }
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

        private static int _MinEnemiesR
        {
            get { return ViktorRMenu["MinEnemiesR"].Cast<Slider>().CurrentValue; }
        }

        private static int _RTicks
        {
            get { return ViktorRMenu["RTicks"].Cast<Slider>().CurrentValue; }
        }

        private static int _RTickSlider
        {
            get { return ViktorMiscMenu["RTickSlider"].Cast<Slider>().CurrentValue; }
        }


        private static HitChance PredictionRate
        {
            get
            {
                if (ViktorComboMenu["PredictionRate"].Cast<Slider>().CurrentValue <= 1)
                    return HitChance.Low;
                if (ViktorComboMenu["PredictionRate"].Cast<Slider>().CurrentValue == 2)
                    return HitChance.Medium;
                return HitChance.High;
            }
        }

        #endregion



    }
}