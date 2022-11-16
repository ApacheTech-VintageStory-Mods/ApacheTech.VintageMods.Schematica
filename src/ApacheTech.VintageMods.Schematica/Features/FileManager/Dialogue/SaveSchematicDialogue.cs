using ApacheTech.VintageMods.Schematica.Core.Extensions;
using ApacheTech.VintageMods.Schematica.Features.FileManager.DataStructures;
using ApacheTech.VintageMods.Schematica.Features.Regions;
using Gantry.Core.DependencyInjection.Annotation;
using Gantry.Core.GameContent.GUI;
using Gantry.Services.FileSystem.FileAdaptors;

namespace ApacheTech.VintageMods.Schematica.Features.FileManager.Dialogue
{
    [UsedImplicitly]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hungarian Naming Convention in APL")]
    public class SaveSchematicDialogue : GenericDialogue
    {
        private readonly RegionSelection _system;
        private readonly SchematicMetadata _model;

        private string _fileName;
        private readonly string _defaultFileName;

        private static string Lang(string code, params object[] args)
        {
            return LangEx.FeatureString("FileManager.Dialogue.Metadata", code, args);
        }

        [SidedConstructor(EnumAppSide.Client)]
        public SaveSchematicDialogue(ICoreClientAPI capi, RegionSelection system) : base(capi)
        {
            _system = system;
            Title = Lang("Title");
            Alignment = EnumDialogArea.CenterMiddle;
            Modal = true;
            _model = new SchematicMetadata
            {
                Title = Lang("UntitledSchematic"),
                Description = Lang("CreatedDate", DateTime.Now.ToString("G")),
                Author = capi.World.Player.PlayerName,
                Size = system.Region.Size(),
            };
            _defaultFileName = $"{capi.World.Player.PlayerName}-{DateTime.Now:yyyy-M-dd-HH-mm-ss}.schematic";
        }

        #region Form Composition

        protected override void RefreshValues()
        {
            SingleComposer.GetTextInput("txtTitle").SetPlaceHolderText(Lang("UntitledSchematic"));
            SingleComposer.GetTextInput("txtAuthor").SetPlaceHolderText(capi.World.Player.PlayerName);
            SingleComposer.GetTextInput("txtFileName").SetPlaceHolderText(_defaultFileName);
        }

        protected override void ComposeBody(GuiComposer composer)
        {
            var labelFont = CairoFont.WhiteSmallText();
            var textInputFont = CairoFont.WhiteDetailText().WithOrientation(EnumTextOrientation.Center);
            var confirmButtonText = Lang("btnSave.Text");
            var cancelButtonText = LangEx.ConfirmationString("cancel");
            var headerBounds = ElementBounds.FixedSize(400, 30);

            //
            // Title
            //

            var left = ElementBounds.FixedSize(100, 30).FixedUnder(headerBounds, 10); //.WithFixedOffset(0, 30);
            var right = ElementBounds.FixedSize(270, 30).FixedUnder(headerBounds, 10).FixedRightOf(left, 10);

            composer
                .AddStaticText(Lang("lblTitle.Text"),
                    labelFont, EnumTextOrientation.Right, left.WithFixedOffset(0, 5), "lblTitle")
                .AddHoverText(Lang("lblTitle.HoverText"), textInputFont, 260, left)
                .AddTextInput(right, txtTitle_TextChanged, textInputFont, "txtTitle");

            //
            // Author
            //

            left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
            right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

            composer
                .AddStaticText(Lang("lblAuthor.Text"),
                    labelFont, EnumTextOrientation.Right, left, "lblAuthor")
                .AddHoverText(Lang("lblAuthor.HoverText"), textInputFont, 260, left)
                .AddTextInput(right, txtAuthor_TextChanged, textInputFont, "txtAuthor");

            //
            // Description
            //

            left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
            right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

            composer
                .AddStaticText(Lang("lblDescription.Text"),
                    labelFont, EnumTextOrientation.Right, left, "lblDescription")
                .AddHoverText(Lang("lblDescription.HoverText"), textInputFont, 260, left)
                .AddTextInput(right, txtDescription_TextChanged, textInputFont, "txtDescription");

            //
            // File Name
            //

            left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
            right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

            composer
                .AddStaticText(Lang("lblFilename.Text"),
                    labelFont, EnumTextOrientation.Right, left, "lblFilename")
                .AddHoverText(Lang("lblFilename.HoverText"), textInputFont, 260, left)
                .AddTextInput(right, txtFileName_TextChanged, textInputFont, "txtFileName");

            //
            // Buttons
            //

            var controlRowBoundsLeftFixed = ElementBounds.FixedSize(150, 30).WithAlignment(EnumDialogArea.LeftFixed);
            var controlRowBoundsRightFixed = ElementBounds.FixedSize(150, 30).WithAlignment(EnumDialogArea.RightFixed);

            composer
                .AddSmallButton(cancelButtonText, btnCancel_Click, controlRowBoundsLeftFixed.FixedUnder(left, 10))
                .AddSmallButton(confirmButtonText, btnSave_Click, controlRowBoundsRightFixed.FixedUnder(right, 10).WithFixedOffset(0, 5));
        }

        #endregion

        #region Control Event Handlers

        private void txtTitle_TextChanged(string title) 
            => _model.Title = title;

        private void txtAuthor_TextChanged(string author) 
            => _model.Author = author;

        private void txtDescription_TextChanged(string description) 
            => _model.Description = description;

        private void txtFileName_TextChanged(string fileName) 
            => _fileName = fileName;

        private bool btnCancel_Click() 
            => TryClose();

        private bool btnSave_Click()
        {
            // TODO: Extract method to file manager.
            _fileName = _fileName
                .IfNullOrWhitespace(_defaultFileName)
                .EnsureEndsWith(".schematic");

            // TODO: Extract path to domain.
            var schematicsDirectory = Path.Combine(GamePaths.DataPath, "Schematics");
            var filePath = Path.Combine(schematicsDirectory, _fileName);

            var file = new JsonModFile(filePath);

            _model.DateCreated = DateTime.Now;

            var schematicBlock = new BlockSchematic();
            var region = _system.Region;

            schematicBlock.AddArea(capi.World, region.Cuboid.LowerBounds(), region.Cuboid.ExclusiveUpperBounds());
            schematicBlock.Pack(capi.World, region.Cuboid.LowerBounds());

            var materialsList = new MaterialsList
            {
                Blocks = schematicBlock.ExtractBlocksList(),
                Decor = schematicBlock.ExtractDecorList(),
                Items = schematicBlock.ExtractItemsList(),
                Entities = schematicBlock.ExtractEntitiesList()
            };

            var schematicFile = new SchematicFile
            {
                Metadata = _model,
                Materials = materialsList,
                Schematic = schematicBlock
            };

            var json = JsonConvert.SerializeObject(schematicFile, Formatting.Indented);

            file.SaveFrom(json);
            return TryClose();
        }

        #endregion
    }
}