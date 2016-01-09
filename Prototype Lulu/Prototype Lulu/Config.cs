using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Prototype_Lulu
{
    static class Config
    {
        public static void Initialize() { }
        private static Menu LuluMainMenu, LuluComboMenu, LuluHarrasMenu, LuluProtectorMenu, LuluMiscMenu, LuluDrawingsMenu;

        static Config()
        {
            LuluMainMenu = MainMenu.AddMenu("Prototype Lulu", "Prototype Lulu");

            LuluComboMenu = LuluMainMenu.AddSubMenu("Combo", "Combo");
            LuluComboMenu.AddLabel("[Combo Settings]");
            LuluComboMenu.Add("UseQ", new CheckBox("Use Q"));
            LuluComboMenu.Add("UseW", new CheckBox("Use W"));
            LuluComboMenu.Add("UseE", new CheckBox("Use E"));
            LuluComboMenu.Add("UseR", new CheckBox("Use R"));
            LuluComboMenu.Add("UseIgnite", new CheckBox("Use Ignite"));
            LuluComboMenu.Add("HealthR", new Slider("Use Ulti(R) when Health is under (%):", 10, 1, 100));
            LuluComboMenu.AddSeparator(15);
            LuluComboMenu.Add("KillSteal", new CheckBox("Enable KillSteal"));
            LuluComboMenu.Add("KillStealQ", new CheckBox("KillSteal with Q"));
            LuluComboMenu.Add("KillStealE", new CheckBox("KillSteal with E"));
            LuluComboMenu.Add("KillStealQE", new CheckBox("KillSteal with E-Q Combo (Extented Q)"));

            LuluHarrasMenu = LuluMainMenu.AddSubMenu("Harras", "Harras");
            LuluHarrasMenu.AddLabel("[Harras Settings]");
            LuluHarrasMenu.Add("HarrasQ", new CheckBox("Use Q"));
            LuluHarrasMenu.Add("HarrasE", new CheckBox("Use E"));
            LuluHarrasMenu.Add("HarrasQE", new CheckBox("Use E-Q (Extented Q)"));
            LuluHarrasMenu.Add("HarrasManaSlider", new Slider("Minimum mana to Harras (%):", 40, 1, 100));

            LuluProtectorMenu = LuluMainMenu.AddSubMenu("Protector", "Protector");
            LuluProtectorMenu.AddLabel("[Protector Settings]");
            LuluProtectorMenu.AddLabel("(This option will prioritize to protect Allies.)");
            LuluProtectorMenu.AddLabel("(Also it wont cast (E) skill on Enemies in Combo Mode)");
            LuluProtectorMenu.Add("SupportMode", new CheckBox("Enable Lulu Support Mode"));
            LuluProtectorMenu.AddLabel("[Protector Spells]");
            LuluProtectorMenu.Add("ProtectorUseE", new CheckBox("Use E"));
            LuluProtectorMenu.Add("ProtectorUseR", new CheckBox("Use R"));
            LuluProtectorMenu.AddLabel("[Gapcloser Settings]");
            LuluProtectorMenu.Add("GapClose", new CheckBox("Prevent GapClosers (W)"));
            LuluProtectorMenu.Add("GapCloseAllies", new CheckBox("Prevent Gapclosers on Allies (W)"));
            LuluProtectorMenu.Add("Interrupt", new CheckBox("Auto Interrupt Spells (W)"));
            LuluProtectorMenu.Add("Poison", new CheckBox("Auto Protect from Poison Spells (E)"));
            LuluProtectorMenu.AddLabel("[Protect Allies]");
            foreach (var protectTarget in EntityManager.Heroes.Allies)
            {
                LuluProtectorMenu.Add(protectTarget.ChampionName + "CB", new CheckBox("Protect " + protectTarget.ChampionName, false));
                LuluProtectorMenu.Add(protectTarget.ChampionName + "SL", new Slider(protectTarget.ChampionName + " Minimum HP to cast Ulti(R) (%):", 20, 1, 100));
            }

            LuluMiscMenu = LuluMainMenu.AddSubMenu("Misc", "Misc");
            LuluMiscMenu.AddGroupLabel("[Misc Settings]");
            LuluMiscMenu.AddLabel("[Skin Selector]");
            LuluMiscMenu.Add("SkinSelector", new CheckBox("Enable Skin Selector"));
            LuluMiscMenu.Add("SkinID", new Slider("Skin ID:", 1, 1, 5));
            LuluMiscMenu.AddLabel("[Prediction]");
            LuluMiscMenu.Add("PredictionH", new Slider("Prediction Hitchance:", 3, 1, 3));

            LuluDrawingsMenu = LuluMainMenu.AddSubMenu("Drawings", "Drawings");
            LuluDrawingsMenu.AddGroupLabel("[Drawings Settings");
            LuluDrawingsMenu.AddLabel("[Draw Range Settings]");
            LuluDrawingsMenu.Add("DisableAll", new CheckBox("Disable All Drawings", false));
            LuluDrawingsMenu.Add("DrawQ", new CheckBox("Draw Q"));
            LuluDrawingsMenu.Add("DrawW", new CheckBox("Draw W", false));
            LuluDrawingsMenu.Add("DrawE", new CheckBox("Draw E", false));
            LuluDrawingsMenu.Add("DrawR", new CheckBox("Draw R", false));
        }

        //Skin Selector Config
        public static Slider _SkinSelector
        {
            get { return LuluMiscMenu["SkinID"].Cast<Slider>(); }
        }

        //--------------- Menu Checkboxes -----------------//
        public static bool ReturnBoolMenu(string category, string unqIdentifier)
        {
            //Console.WriteLine("Returned Menu Name: {0} with identifier: {1}",name.DisplayName,unqIdentifier);
            switch (category)
            {
                case "Combo":
                    return LuluComboMenu[unqIdentifier].Cast<CheckBox>().CurrentValue;

                case "Harras":
                    return LuluHarrasMenu[unqIdentifier].Cast<CheckBox>().CurrentValue;

                case "Protector":
                    return LuluProtectorMenu[unqIdentifier].Cast<CheckBox>().CurrentValue;

                case "Drawings":
                    return LuluDrawingsMenu[unqIdentifier].Cast<CheckBox>().CurrentValue;

                case "Misc":
                    return LuluMiscMenu[unqIdentifier].Cast<CheckBox>().CurrentValue;
            }
            return false;
        }

        //--------------- Menu Slider -----------------//
        public static int ReturnIntMenu(string category, string unqIdentifier)
        {
            switch (category)
            {
                case "Combo":
                    return LuluComboMenu[unqIdentifier].Cast<Slider>().CurrentValue;

                case "Harras":
                    return LuluHarrasMenu[unqIdentifier].Cast<Slider>().CurrentValue;

                case "Protector":
                    return LuluProtectorMenu[unqIdentifier].Cast<Slider>().CurrentValue;

                case "Drawings":
                    return LuluDrawingsMenu[unqIdentifier].Cast<Slider>().CurrentValue;

                case "Misc":
                    return LuluMiscMenu[unqIdentifier].Cast<Slider>().CurrentValue;
            }
            return 0;
        }


    }
}
