using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModSettingsTab.Framework.Integration;
using StardewModdingAPI;
using StardewValley.Menus;
using OptionsElement = ModSettingsTab.Framework.Components.OptionsElement;

namespace ModSettingsTab
{
    public static class ModData
    {
        /// <summary>
        /// selected text field to reset and hold focus
        /// </summary>
        public static TextBox CurrentTextBox { get; set; }

        public const int Offset = 192;

        /// <summary>
        /// collection of loaded mods, only those that have settings
        /// </summary>
        public static ModList ModList { get; private set; }

        /// <summary>
        /// predefined collection of mod settings
        /// </summary>
        /// <remarks>
        /// Dictionary&lt;string:UniqueId, ModIntegrationSettings&gt;
        /// </remarks>
        public static Dictionary<string, ModIntegrationSettings> PredefinedIntegration;

        /// <summary>
        /// list of all modification settings
        /// </summary>
        public static List<OptionsElement> Options;

        public static SmapiIntegration SMAPI;

        /// <summary>
        /// list of favorite mod options
        /// </summary>
        public static readonly List<Mod> FavoriteMod;
        public static readonly Texture2D Tabs;
        public static Dictionary<string,Rectangle> FavoriteTabSource;
        public static readonly Queue<Rectangle> FreeFavoriteTabSource;

        public delegate void Update();

        public static Update UpdateFavoriteMod;

        static ModData()
        {
            FavoriteMod = new List<Mod>();
            Tabs = ModEntry.Helper.Content.Load<Texture2D>("assets/Tabs.png");
            FreeFavoriteTabSource = new Queue<Rectangle>
            (new[]{
                new Rectangle(0,128,32,24),
                new Rectangle(32,128,32,24),
                new Rectangle(0,152,32,24),
                new Rectangle(32,152,32,24),
                new Rectangle(0,176,32,24),
            });
            FavoriteTabSource = new Dictionary<string, Rectangle>();
        }

        /// <summary>
        /// initialization of master data
        /// </summary>
        public static async void Init()
        {
            await LoadIntegrations();
            ModEntry.Console.Log($"Load {PredefinedIntegration.Count} Integrations", LogLevel.Info);
            await LoadOptions();
            ModEntry.Console.Log($"Load {ModList.Count} mods and {Options.Count} Options | {FavoriteMod.Count}",
                LogLevel.Info);
        }

        /// <summary>
        /// asynchronously updates the list of settings of selected mods
        /// </summary>
        public static async void UpdateFavoriteOptionsAsync()
        {
            await Task.Run(LoadFavoriteOptions);
            UpdateFavoriteMod();
        }

        private static Task LoadIntegrations()
        {
            return Task.Run(() =>
            {
                PredefinedIntegration =
                    ModEntry.Helper.Data.ReadJsonFile<Dictionary<string, ModIntegrationSettings>>(
                        "data/integration.json") ?? new Dictionary<string, ModIntegrationSettings>();
            });
        }

        private static Task LoadOptions()
        {
            return Task.Run(() =>
            {
                ModList = new ModList();
                SMAPI = new SmapiIntegration();
                Options = ModList.SelectMany(mod => mod.Value.Options).ToList();
                LoadFavoriteOptions();
            });
        }

        private static void LoadFavoriteOptions()
        {
            FavoriteMod.Clear();
            foreach (var id in FavoriteData.Favorite)
            {
                FavoriteMod.Add(ModList[id]);
            }
            for (var i = FavoriteMod.Count; i > 0 ; i--)
            {
                var id = FavoriteMod[i-1].Manifest.UniqueID;
                if (!FavoriteTabSource.ContainsKey(id))
                    FavoriteTabSource.Add(id, FreeFavoriteTabSource.Dequeue());
            }
            
        }
    }
}