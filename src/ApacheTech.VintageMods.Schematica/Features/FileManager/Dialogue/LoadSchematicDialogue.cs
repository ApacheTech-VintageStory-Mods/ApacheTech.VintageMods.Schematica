using System.Text;
using ApacheTech.VintageMods.Schematica.Core.DataStructures;
using ApacheTech.VintageMods.Schematica.Core.Extensions;
using ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures;
using ApacheTech.VintageMods.Schematica.Features.Placements.Renderer;
using Gantry.Core.GameContent.GUI;
using Gantry.Services.FileSystem;
using Vintagestory.Client;

namespace ApacheTech.VintageMods.Schematica.Features.FileManager.Dialogue
{
    /// <summary>
    ///     Dialogue Window: Allows the user to import waypoints from JSON files.
    /// </summary>
    /// <seealso cref="GenericDialogue" />
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LoadSchematicDialogue : GenericDialogue
    {
        private readonly LoadedSchematic _schematic;
        private readonly FileSystemWatcher _watcher;
        private int _watcherLock;

        private ElementBounds _clippedBounds;
        private ElementBounds _cellListBounds;

        private List<SchematicInfoCellEntry> _files = new();

        private GuiElementCellList<SchematicInfoCellEntry> _filesList;
        private GuiElementDynamicText _lblSelectedCount;

        // TODO: Should be taken from domain.
        private readonly string _schematicsDirectory = ModPaths.CreateDirectory(Path.Combine(GamePaths.DataPath, "Schematics"));

        private SchematicPlacementRenderer _renderer; 
        private EnumRenderStage RenderStage => EnumRenderStage.Opaque;

        /// <summary>
        /// 	Initialises a new instance of the <see cref="LoadSchematicDialogue" /> class.
        /// </summary>
        /// <param name="capi">Client API pass-through</param>
        /// <param name="system">The placement to render.</param>
        public LoadSchematicDialogue(ICoreClientAPI capi, LoadedSchematic schematic) : base(capi)
        {
            _schematic = schematic;
            Title = Lang("Title");
            Alignment = EnumDialogArea.CenterMiddle;

            _watcher = new FileSystemWatcher(_schematicsDirectory, "*.schematic");
            _watcher.Changed += OnDirectoryChanged;
            _watcher.Created += OnDirectoryChanged;
            _watcher.Deleted += OnDirectoryChanged;
            _watcher.Renamed += OnDirectoryChanged;

            ClientSettings.Inst.AddWatcher<float>("guiScale", _ =>
            {
                Compose();
                RefreshValues();
            });
        }

        #region Form Composition

        /// <summary>
        ///     Composes the GUI components for this instance.
        /// </summary>
        protected override void Compose()
        {
            base.Compose();
            RefreshFiles();
        }

        /// <summary>
        ///     Refreshes the file list, displayed on the form, whenever changes are made.
        /// </summary>
        private void RefreshFiles()
        {
            _files = GetImportCellsFromDirectory(_schematicsDirectory);
            _filesList.ReloadCells(_files);
            _watcherLock = 0;
            _watcher.EnableRaisingEvents = true;
        }

        private static string Lang(string code, params object[] args)
        {
            return LangEx.FeatureString("FileManager.Dialogue.Load", code, args);
        }

        private static List<SchematicInfoCellEntry> GetImportCellsFromDirectory(string path)
        {
            var files = new DirectoryInfo(path).GetFiles("*.schematic").OrderBy(p => p.CreationTime).ToList();
            var list = new List<SchematicInfoCellEntry>();
            foreach (var file in files)
            {
                try
                {
                    var dto = JsonConvert.DeserializeObject<SchematicInfoDto>(File.ReadAllText(file.FullName));
                    var detailText = new StringBuilder();
                    detailText.AppendLine(Lang("AuthorText", dto.Metadata.Author));
                    detailText.AppendLine(Lang("DescriptionText", dto.Metadata.Description));
                    detailText.AppendLine(Lang("CreatedText", dto.Metadata.DateCreated.ToString("F")));

                    var size = dto.Metadata.Size;
                    var sizeText = $"({size.X} x {size.Y} x {size.Z})";

                    list.Add(new SchematicInfoCellEntry
                    {
                        Title = dto.Metadata.Title,
                        DetailText = detailText.ToString(),
                        Enabled = true,
                        RightTopText = sizeText,
                        RightTopOffY = 3f,
                        DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                        Model = dto.With(d => d.File = file)
                    });
                }
                catch (Exception exception)
                {
                    ApiEx.Client.Logger.Error("[Schematica] Error caught while loading schematic file.");
                    ApiEx.Client.Logger.Error(exception.Message);
                    ApiEx.Client.Logger.Error(exception.StackTrace);
                    return new List<SchematicInfoCellEntry>();
                }
            }
            return list;
        }

        /// <summary>
        ///     Refreshes the displayed values on the form.
        /// </summary>
        protected override void RefreshValues()
        {
            if (SingleComposer is null) return;

            capi.Event.EnqueueMainThreadTask(() =>
            {
                var listHeight = (float)_clippedBounds.fixedHeight;
                var cellHeight = (float)_cellListBounds.fixedHeight;

                _cellListBounds.CalcWorldBounds();
                _clippedBounds.CalcWorldBounds();
                SingleComposer
                    .GetScrollbar("scrollbar")
                    .SetHeights(listHeight, cellHeight);
            }, "");
        }

        /// <summary>
        ///     Composes the main body of the dialogue window.
        /// </summary>
        /// <param name="composer">The GUI composer.</param>
        protected override void ComposeBody(GuiComposer composer)
        {
            var platform = ScreenManager.Platform;
            var scaledWidth = Math.Max(600, platform.WindowSize.Width * 0.5) / ClientSettings.GUIScale;
            var scaledHeight = Math.Max(600, (platform.WindowSize.Height - 65) * 0.85) / ClientSettings.GUIScale;

            var buttonRowBoundsRightFixed = ElementBounds
                .FixedSize(60, 30)
                .WithFixedPadding(10, 2)
                .WithAlignment(EnumDialogArea.RightFixed);

            var buttonRowBounds = ElementBounds
                .FixedSize(60, 30)
                .WithFixedPadding(10, 2);

            var textBounds = ElementBounds
                .FixedSize(300, 30)
                .WithFixedPadding(10, 2)
                .WithAlignment(EnumDialogArea.CenterTop);

            var outerBounds = ElementBounds
                .Fixed(EnumDialogArea.LeftTop, 0, 0, scaledWidth, 35);

            var insetBounds = outerBounds
                .BelowCopy(0, 3)
                .WithFixedSize(scaledWidth, scaledHeight);

            _clippedBounds = insetBounds
                .ForkContainingChild(3, 3, 3, 3);

            _cellListBounds = _clippedBounds
                .ForkContainingChild(0.0, 0.0, 0.0, -3.0)
                .WithFixedPadding(10.0);

            _filesList = new GuiElementCellList<SchematicInfoCellEntry>(capi, _cellListBounds, OnRequireCell, _files);

            _lblSelectedCount =
                new GuiElementDynamicText(capi, string.Empty,
                    CairoFont.WhiteDetailText().WithOrientation(EnumTextOrientation.Center),
                    textBounds.FixedUnder(insetBounds, 10));

            composer
                .AddInset(insetBounds)
                .AddVerticalScrollbar(OnScroll, ElementStdBounds.VerticalScrollbar(insetBounds), "scrollbar")
                .BeginClip(_clippedBounds)
                .AddInteractiveElement(_filesList)
                .EndClip()

                .AddSmallButton(Lang("OpenSchematicsFolder"), btnOpenSchematicsFolder_Click,
                    buttonRowBounds.FlatCopy().FixedUnder(insetBounds, 10.0))

                .AddInteractiveElement(_lblSelectedCount)

                .AddSmallButton(Lang("LoadSelectedSchematic"), btnLoadSelectedSchematic_Click,
                    buttonRowBoundsRightFixed.FlatCopy().FixedUnder(insetBounds, 10.0));
        }

        #endregion

        #region Control Event Handlers

        /// <summary>
        ///     Called when the GUI needs to refresh or create a cell to display to the user. 
        /// </summary>
        private IGuiElementCell OnRequireCell(SchematicInfoCellEntry cell, ElementBounds bounds)
        {
            return new SchematicInfoGuiCell(ApiEx.Client, cell, bounds)
            {
                On = false,
                OnMouseDownOnCellLeft = OnCellClick,
                OnMouseDownOnCellRight = OnCellClick
            };
        }

        private void OnScroll(float dy)
        {
            var bounds = _filesList.Bounds;
            bounds.fixedY = 0f - dy;
            bounds.CalcWorldBounds();
        }

        /// <summary>
        ///     Called when the user clicks on one of the cells in the grid.
        /// </summary>
        private void OnCellClick(int val)
        {
            var list = _filesList.elementCells.Cast<SchematicInfoGuiCell>().ToList();
            var cell = list[val];
            list.ForEach(c =>
            {
                c.On = c.Equals(cell) && !cell.On;
                c.Enabled = c.Equals(cell) && !cell.On;
            });
            RefreshValues();
        }

        /// <summary>
        ///     Called when the user adds, removes, or renames files within the imports folder.
        /// </summary>
        private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType is WatcherChangeTypes.Changed) return;
            if (++_watcherLock > 1) return;
            _watcher.EnableRaisingEvents = false;
            RefreshFiles();
            RefreshValues();
        }

        /// <summary>
        ///     Called when the user presses the "Open Imports Folder" button.
        /// </summary>
        private bool btnOpenSchematicsFolder_Click()
        {
            //NetUtil.OpenUrlInBrowser(_schematicsDirectory);

            capi.Event.UnregisterRenderer(_renderer, RenderStage);
            _renderer.Dispose();
            return true;
        }

        /// <summary>
        ///     Called when the user presses the "Import" button.
        /// </summary>
        private bool btnLoadSelectedSchematic_Click()
        {
            // TODO: Deserialise schematic.
            var info = _filesList.elementCells.Cast<SchematicInfoGuiCell>().FirstOrDefault(p => p.On)?.Cell.Model;
            if (info is null) return false;

            var dto = JsonConvert.DeserializeObject<SchematicFile>(File.ReadAllText(info.File.FullName));

            _schematic.HighlightGhostBlocks = false;
            _schematic.ShowLiquids = true;
            _schematic.ShowSolidBlocks = true;
            _schematic.ShowDecor = false;
            _schematic.ShowEntities = false;
            _schematic.Layer = 0;
            _schematic.RenderLayers = SchematicLayerType.All;
            _schematic.Rotation = 0;
            _schematic.StartPos = ClampedBlockPos.FromPlayerPos();
            _schematic.Metadata = dto.Metadata;
            _schematic.Schematic = dto.Schematic;

            _renderer = IOC.Services.Resolve<SchematicPlacementRenderer>();
            capi.Event.RegisterRenderer(_renderer, RenderStage);
            capi.Shader.ReloadShaders();

            _schematic.Enabled = true;

            return TryClose();
        }

        #endregion

        /// <summary>
        ///     Disposes the dialogue window.
        /// </summary>
        public override void Dispose()
        {
            _watcher?.Dispose();
            _filesList?.Dispose();
            _lblSelectedCount?.Dispose();
            base.Dispose();
        }
    }
}