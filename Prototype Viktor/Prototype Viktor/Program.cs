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
        private static Spell.Targeted Q, Ignite;
        private static bool bIgnite;
        private static Spell.Skillshot W, E, R;
        private static int EMaxRange = 1225;
        private static int _tick = 0;
        private static Vector3 startPos;
        private static Menu ViktorMenu;
        private static Menu ViktorComboMenu, ViktorHarassMenu, ViktorLaneClearMenu, ViktorMiscMenu, ViktorDrawMenu;
        private static string version = "0.0.0.12";
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

        private static bool _UseIgnite
        {
            get { return ViktorComboMenu["UseIgnite"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _CheckR
        {
            get { return ViktorComboMenu["CheckR"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _AutoFollowR
        {
            get { return ViktorComboMenu["FollowR"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool _FollowRViktor
        {
            get { return ViktorComboMenu["FollowRViktor"].Cast<CheckBox>().CurrentValue; }
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

        private static int _HarassMana
        {
            get { return ViktorHarassMenu["HarassMana"].Cast<Slider>().CurrentValue; }
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
        private static int _MinEnemiesR
        {
            get { return ViktorComboMenu["MinEnemiesR"].Cast<Slider>().CurrentValue; }
        }
        private static int _RTicks
        {
            get { return ViktorComboMenu["RTicks"].Cast<Slider>().CurrentValue; }
        }
        private static int _RTickSlider
        {
            get { return ViktorMiscMenu["RTickSlider"].Cast<Slider>().CurrentValue; }
        }

        private static Slider _SkinChanger
        {
            get { return ViktorMiscMenu["SkinChanger"].Cast<Slider>(); }
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
            bIgnite = Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("summonerdot")) != null;
            if (bIgnite)
            {
                Ignite = new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);
                Console.WriteLine("Ignite Loaded!");
            }

            LoadSkills();
            LoadMenu();
            SelectSkin(_SkinChanger.CurrentValue);

            _SkinChanger.OnValueChange += delegate(ValueBase<int> s, ValueBase<int>.ValueChangeArgs aargs)
            {
                SelectSkin(aargs.NewValue);
            };
            


            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;

            Chat.Print("Prototype Viktor " + version + " Loaded!");
            Console.WriteLine("Prototype Viktor " + version + " Loaded!");

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || _Player.HasBuff("Recall")) return;

            if (_AutoFollowR)
            {
                if ((_Player.HasBuff("Glory") || R.Name == "GuideSingularity" || R.Name != "ViktorChaosStorm")) // && Environment.TickCount - _tick > 0
                {
                    var stormT = TargetSelector.GetTarget(2000, DamageType.Magical); //lower range.
                    if (stormT != null && stormT.IsValid && stormT.IsVisible)
                    {
                        R.Cast(stormT.ServerPosition);
                        //_tick = Environment.TickCount + _RTickSlider;
                    }
                    if (stormT == null && _FollowRViktor)
                    {
                        R.Cast(_Player.ServerPosition);
                        
                        //_tick = Environment.TickCount + _RTickSlider;
                    }
                }
            }

            KillSecure();
            /*
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    LaneClear();
                    break;
                case Orbwalker.ActiveModes.JungleClear:
                    JungleClear();
                    break;
            }
            */
            //Trying to locate the Auto E-R-bug

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) LaneClear();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) JungleClear();


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
            ViktorMenu.AddLabel("*Auto Ignite in Combo");
            ViktorMenu.AddLabel("*Killsteal Dragon/Baron");
            ViktorMenu.AddLabel("*Improve Damage Calculations");

            ViktorComboMenu = ViktorMenu.AddSubMenu("Combo", "Combo");
            ViktorComboMenu.AddLabel("[Combo Settings:]");
            ViktorComboMenu.Add("UseQ", new CheckBox("Use Q"));
            ViktorComboMenu.Add("UseW", new CheckBox("Use W", false));
            ViktorComboMenu.Add("UseE", new CheckBox("Use E"));
            ViktorComboMenu.Add("UseR", new CheckBox("Use R"));
            ViktorComboMenu.Add("UseIgnite", new CheckBox("Use Ignite", false));
            ViktorComboMenu.Add("FollowR", new CheckBox("Auto Follow R (Only Enemy)"));
            ViktorComboMenu.Add("FollowRViktor", new CheckBox("Auto Follow R (Enemies&Viktor)", false));
            ViktorComboMenu.Add("CheckR", new CheckBox("Cast R only if enemy is killable with Combo"));
            ViktorComboMenu.AddSeparator(10);
            ViktorComboMenu.AddLabel("[KillSteal Options:]");
            ViktorComboMenu.Add("EnableKS", new CheckBox("Enable KillSteal"));
            ViktorComboMenu.Add("KsQ", new CheckBox("KillSteal with Q"));
            ViktorComboMenu.Add("KsE", new CheckBox("KillSteal with E"));
            ViktorComboMenu.AddSeparator(10);
            ViktorComboMenu.Add("MinW", new Slider("Mininum Enemies to cast W:", 2, 1, 5));
            ViktorComboMenu.Add("MinEnemiesR", new Slider("Minimum Enemies to cast R", 1, 1, 5)); //
            ViktorComboMenu.Add("RTicks", new Slider("R Ticks (per 0.5s) to calculate in Damage Output:", 10, 1, 14));
            ViktorComboMenu.Add("PredictionRate", new Slider("Prediction Rate:", 3, 1, 3));

            ViktorHarassMenu = ViktorMenu.AddSubMenu("Harass", "Harass");
            ViktorHarassMenu.AddLabel("[Harass Settings:]");
            ViktorHarassMenu.Add("HarassQ", new CheckBox("Use Q"));
            ViktorHarassMenu.Add("HarassE", new CheckBox("Use E"));
            ViktorHarassMenu.AddSeparator(15);
            ViktorHarassMenu.Add("HarassMana", new Slider("Mana for Harassment (%):", 30, 1, 100));


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
            ViktorMiscMenu.AddLabel("* Anti Gapcloser will cast (W) on Viktor's position");
            ViktorMiscMenu.AddSeparator(10);
            ViktorMiscMenu.Add("RTickSlider", new Slider("How fast R will move to the next Target:", 250, 100, 500));
            ViktorMiscMenu.AddLabel("*Lower is better, but I think 250 is optimal.");
            ViktorMiscMenu.AddSeparator(10);
            ViktorMiscMenu.Add("SkinChanger",new Slider("Skin Select:",1,1,4));


        }
        #endregion

        private static void Combo()
        {
            if (W.IsReady() && _ViktorW) CastW();
            if (Q.IsReady() && _ViktorQ) CastQ();
            if (E.IsReady() && _ViktorE) Core.DelayAction(CastE, 120);
            if (R.IsReady() && _ViktorR) CastR();
            if (bIgnite && _UseIgnite) UseIgnite();


        }

        private static void Harass()
        {
            if (_HarassMana <= _Player.ManaPercent)
            {
                if (E.IsReady() && _HarassE) CastE();
                if (Q.IsReady() && _HarassQ) Core.DelayAction(CastQ, 50);
            }
        }

        private static void LaneClear()
        {
            if (_LaneClearMana <= _Player.ManaPercent)
            {
                var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Enemy, _Player.Position, EMaxRange, false);

                foreach (var minion in minions)
                {
                    if (_LaneClearE && minions.Count() >= _MinMinions)
                    {
                        var loc = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, E.Width, EMaxRange);
                        Player.CastSpell(SpellSlot.E, loc.CastPosition, minion.ServerPosition);
                        // Chat.Print("Minions in Lane: " + minions.Count() + "Mininum minions to cast E: " + _MinMinions);
                    }
                    if (_LaneClearQ && minion.Health < _Player.GetSpellDamage(minion, SpellSlot.Q) + CalculateAADmg())
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        private static void JungleClear()
        {
            var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Monster, EntityManager.UnitTeam.Both, _Player.Position, EMaxRange, false);

            foreach (var minion in minions)
            {
                if (_LaneClearE && minions.Count() >= _MinMinions)
                {
                    var loc = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, E.Width, EMaxRange);
                    Player.CastSpell(SpellSlot.E, loc.CastPosition, minion.ServerPosition);
                }
                if (_LaneClearQ && minion.Health < _Player.GetSpellDamage(minion, SpellSlot.Q) + CalculateAADmg())
                {
                    Q.Cast(minion);
                }
            }

        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (_GapCloser && (e.End.Distance(_Player) <= 170) && sender.IsEnemy) //fix?
                W.Cast(_Player);
        }

        private static void KillSecure()
        {
            if (!_KillSteal) return;
            foreach (AIHeroClient target in EntityManager.Heroes.Enemies)
            {
                if (_KsE && target.IsValidTarget(EMaxRange) &&
                    target.Health < _Player.GetSpellDamage(target, SpellSlot.E))
                {
                    CastE();
                }
                /*
                else if (_KsQ && target.IsValidTarget(Q.Range) && target.Health < (_Player.GetSpellDamage(target, SpellSlot.E) + _Player.GetSpellDamage(target, SpellSlot.Q) + CalculateAADmg()))
                {
                    CastE();
                    Core.DelayAction(CastQ, 50);
                }*/
                if (_KsQ && target.IsValidTarget(Q.Range) &&
                    target.Health < _Player.GetSpellDamage(target, SpellSlot.Q) + CalculateAADmg())
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
            if (target != null)
            {
                if (target.CountEnemiesInRange(W.Width) >= _MinW)

                    W.Cast(target);
            }
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(EMaxRange, DamageType.Magical);
            if (target != null && target.IsEnemy && target.IsVisible)
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
            if (target != null && target.IsEnemy && !target.IsZombie && target.IsVisible && target.CountEnemiesInRange(R.Width) >= _MinEnemiesR && R.Name == "ViktorChaosStorm")
            {
                var prediction = E.GetPrediction(target);
                var predictDmg = PredictDamage(target);
                //Chat.Print("Target Health: " + target.Health + "Predict Dmg: " + predictDmg);
                if (target.HealthPercent > 5 && _CheckR && prediction.HitChance >= HitChance.High)
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

        private static void UseIgnite()
        {
            if (!Ignite.IsReady()) return;
            var target = TargetSelector.GetTarget(Ignite.Range, DamageType.True);
            if (target != null && (!target.IsZombie || !target.IsInvulnerable))
            {   //Overkill Protection
                if (target.Health > PredictDamage(target) && target.Health <= PredictDamage(target) + _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                {
                    Ignite.Cast(target);
                }
            }
        }

        private static float PredictDamage(AIHeroClient t)
        {
            float dmg = 0f;
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
            double[] AAdmg = new double[] { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };

            return (double)AAdmg[_Player.Level - 1] + _Player.TotalMagicalDamage * 0.5 + _Player.TotalAttackDamage;
        }


        private static double CalculateRTickDmg(AIHeroClient t, int ticks)
        {
            if (R.Level == 0) return 0;
            double dmg = 0;
            if (R.Level == 1)
                dmg += (15 + _Player.TotalMagicalDamage * 0.10) * ticks;
            else if (R.Level == 2)
                dmg += (30 + _Player.TotalMagicalDamage * 0.10) * ticks;
            else if (R.Level == 3) //No point for that,  just testing if that was the error..
                dmg += (45 + _Player.TotalMagicalDamage * 0.10) * ticks;

            return dmg;
        }

        private static void SelectSkin(int skn )
        {
            switch (skn)
            {
                case 1:
                    _Player.SetSkinId(0);
                    break;
                case 2:
                    _Player.SetSkinId(1);
                    break;
                case 3:
                    _Player.SetSkinId(2);
                    break;
                case 4:
                    _Player.SetSkinId(3);
                    break;
            }
        }


    }
}
