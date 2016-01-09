using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using SharpDX;
using EloBuddy.SDK.Menu.Values;

namespace Prototype_Lulu
{
    class Program
    {
        private static readonly string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static bool bIgnite;
        public static GameObject pixie;
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.ChampionName != "Lulu") return;
            bIgnite = Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("summonerdot")) != null;

            SpellFactory.Initialize();
            Config.Initialize();
            Events.Initialize();
            Modes.Initialize();

            Drawing.OnDraw += Drawing_OnDraw;

            Utils.SelectSkin(Config._SkinSelector.CurrentValue);

            Config._SkinSelector.OnValueChange += delegate (ValueBase<int> s, ValueBase<int>.ValueChangeArgs aargs)
            {
                Utils.SelectSkin(aargs.NewValue);
            };




            Chat.Print("Prototype Lulu " + version + " Loaded");

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.ReturnBoolMenu("Drawings", "DisableAll")) return;

            if (Config.ReturnBoolMenu("Drawings", "DrawQ") && SpellFactory.Q.IsReady())
                Circle.Draw(Color.HotPink, SpellFactory.Q.Range, _Player.Position);
            if (Config.ReturnBoolMenu("Drawings", "DrawW") && SpellFactory.W.IsReady())
                Circle.Draw(Color.Brown, SpellFactory.W.Range, _Player.Position);
            if (Config.ReturnBoolMenu("Drawings", "DrawE") && SpellFactory.E.IsReady())
                Circle.Draw(Color.Aqua, SpellFactory.E.Range, _Player.Position);
            if (Config.ReturnBoolMenu("Drawings", "DrawR") && SpellFactory.R.IsReady())
                Circle.Draw(Color.DarkOrange, SpellFactory.R.Range, _Player.Position);
        }


        public static void Pix()
        {
            if (_Player.IsDead)
                pixie = _Player;
            else
            {
                pixie = ObjectManager.Get<GameObject>().FirstOrDefault(c => c.IsAlly && c.Name == "RobotBuddy");
            }
        }


    }
}
