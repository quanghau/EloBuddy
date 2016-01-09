using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype_Lulu
{
    static class SpellProtectDB
    {
        public static Dictionary<string, List<string>> AvoidSpells = new Dictionary<string, List<string>>()

        {
            {"Ashe", new List<string>(){ "Volley"} },
            {"Sivir", new List<string>(){ "SivirQ" } },
            {"Caitlyn", new List<string>() { "CaitlynPiltoverPeacemaker" , "CaitlynAceintheHole" } },
            {"Jinx", new List<string>() { "JinxW", "JinxRWrapper" } },
            {"KogMaw", new List<string>() { "KogMawLivingArtillery" } },
            {"Varus",new List<string>() { "VarusQ" } },
            {"Vi", new List<string>() { "ViQ" } },
            {"Zed", new List<string>() { "ZedShuriken" } },
            {"Jayce", new List<string>() { "jayceshockblast" } },
            {"Brand", new List<string>() { "BrandWildfire", "brandconflagrationmissile" } },
            {"TahmKench", new List<string>() { "TahmKenchQ" } },
            {"Braum", new List<string>() { "BraumQ" } },
            {"Bard", new List<string>() { "BardQ" } },
            {"Karma", new List<string>() { "KarmaQMissileMantra", "KarmaQ" } },
            {"Blitzcrank", new List<string>() { "RocketGrabMissile", "RocketGrab" } },
            {"Thresh", new List<string>() { "ThreshQ" } },
            {"Sona", new List<string>() { "SonaQ" } },
            {"Nami", new List<string>() { "NamiQ", "NamiR" } },
            {"Morgana", new List<string>() { "DarkBindingMissile" } },
            {"Ezreal", new List<string>() { "EzrealTrueshotBarrage", "EzrealMysticShot" } },
            {"Leona", new List<string>() { "LeonaSolarFlare" } },
            {"Lux", new List<string>() { "LuxMaliceCannon", "LuxLightStrikeKugel", "LuxLightBinding" } },
            {"Nidalee", new List<string>() { "JavelinToss" } },
            {"Ahri", new List<string>() { "AhriSeduce" } },
            {"Karthus", new List<string>() { "KarthusFallenOne", "KarthusLayWasteA1", } }
        };
    }
}
