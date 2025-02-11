﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rhisis.Core.Common;
using Rhisis.Core.DependencyInjection;
using Rhisis.Core.IO;
using Rhisis.Core.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rhisis.Core.Resources
{
    public class GameResources : Singleton<GameResources>
    {
        public static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        public static readonly string DialogsPath = Path.Combine(DataPath, "dialogs");
        public static readonly string ResourcePath = Path.Combine(DataPath, "res");
        public static readonly string MapsPath = Path.Combine(DataPath, "maps");
        public static readonly string ShopsPath = Path.Combine(DataPath, "shops");
        public static readonly string DataSub0Path = Path.Combine(ResourcePath, "data");
        public static readonly string DataSub1Path = Path.Combine(ResourcePath, "dataSub1");
        public static readonly string DataSub2Path = Path.Combine(ResourcePath, "dataSub2");
        public static readonly string MoversPropPath = Path.Combine(DataSub0Path, "propMover.txt");
        public static readonly string MoversPropExPath = Path.Combine(DataSub0Path, "propMoverEx.inc");
        public static readonly string ItemsPropPath = Path.Combine(DataSub2Path, "propItem.txt");
        public static readonly string WorldScriptPath = Path.Combine(DataSub0Path, "World.inc");
        public static readonly string JobPropPath = Path.Combine(DataSub1Path, "propJob.inc");
        public static readonly string TextClientPath = Path.Combine(DataSub1Path, "textClient.inc");
        public static readonly string ExpTablePath = Path.Combine(DataSub0Path, "expTable.inc");
        public static readonly string DeathPenalityPath = Path.Combine(ResourcePath, "deathPenality.json");

        // Logs messages
        public const string UnableLoadMapMessage = "Unable to load map '{0}'. Reason: {1}.";
        public const string UnableLoadMessage = "Unable to load {0}. Reason: {1}";
        public const string ObjectIgnoredMessage = "{0} id '{1}' was ignored. Reason: {2}.";
        public const string ObjectOverridedMessage = "{0} id '{1}' was overrided. Reason: {2}.";
        public const string ObjectErrorMessage = "{0} with id '{1}' has an error. Reason: {2}";

        private ILogger<GameResources> _logger;
        private IEnumerable<Type> _loaders;
        private MoverLoader _movers;
        private ItemLoader _items;
        private ExpTableLoader _expTables;
        private PenalityLoader _penalities;

        /// <summary>
        /// Gets the movers data.
        /// </summary>
        public MoverLoader Movers => this._movers ?? (this._movers = DependencyContainer.Instance.Resolve<MoverLoader>());

        /// <summary>
        /// Gets the items data.
        /// </summary>
        public ItemLoader Items => this._items ?? (this._items = DependencyContainer.Instance.Resolve<ItemLoader>());

        /// <summary>
        /// Gets the exp table data.
        /// </summary>
        public ExpTableLoader ExpTables => this._expTables ?? (this._expTables = DependencyContainer.Instance.Resolve<ExpTableLoader>());

        /// <summary>
        /// Gets the penality resources.
        /// </summary>
        public PenalityLoader Penalities => this._penalities ?? (this._penalities = DependencyContainer.Instance.Resolve<PenalityLoader>());

        /// <summary>
        /// Initialize the <see cref="GameResources"/> with loaders.
        /// </summary>
        /// <param name="loaderTypes"></param>
        public void Initialize(IEnumerable<Type> loaderTypes)
        {
            this._loaders = loaderTypes;

            foreach (var loaderType in this._loaders)
                DependencyContainer.Instance.Register(loaderType, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Load resources.
        /// </summary>
        public void Load()
        {
            this._logger = this._logger ?? DependencyContainer.Instance.Resolve<ILogger<GameResources>>();
            this._logger.LogInformation("Loading resources...");

            Profiler.Start("LoadResources");

            foreach (var loaderType in this._loaders)
            {
                var loader = DependencyContainer.Instance.Resolve(loaderType) as IGameResourceLoader;

                try
                {
                    loader.Load();
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, $"An error occured with loader {loader.GetType().Name}.");
                }
            }

            this._logger.LogInformation("Resources loaded in {0}ms.", Profiler.Stop("LoadResources").ElapsedMilliseconds);
        }
    }
}
