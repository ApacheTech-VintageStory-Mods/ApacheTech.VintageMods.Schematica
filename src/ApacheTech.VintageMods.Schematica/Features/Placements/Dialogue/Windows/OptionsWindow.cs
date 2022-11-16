using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.Placements.Renderer;
using Gantry.Core.GameContent.GUI.Helpers;

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Dialogue.Windows
{
    /// <summary>
    ///     A GUI window that allows the user to show, or hide, preview highlighting for schematic regions.
    /// </summary>
    /// <seealso cref="IGuiWindow" />
    [UsedImplicitly]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hungarian Naming Convention in APL")]
    public class OptionsWindow : IGuiWindow
    {
        private readonly ICoreClientAPI _capi;
        private readonly LoadedSchematic _schematic;
        private GuiComposer _composer;

        /// <summary>
        ///     Initialises a new instance of the <see cref="OptionsWindow" /> class.
        /// </summary>
        /// <param name="capi">The client Api.</param>
        /// <param name="schematic">The <see cref="LoadedSchematic"/> that hosts the schematic area.</param>
        public OptionsWindow(ICoreClientAPI capi, LoadedSchematic schematic)
        {
            _capi = capi;
            _schematic = schematic;
        }

        /// <inheritdoc/>
        public string Key => "Options";

        /// <inheritdoc />
        public GuiComposer Compose(GuiDialog parent)
        {
            var dialogueBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.LeftBottom)
                .WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, -GuiStyle.DialogToScreenPadding);

            var innerBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            innerBounds.BothSizing = ElementSizing.FitToChildren;

            _composer = _capi.Gui
                .CreateCompo(parent.ToggleKeyCombinationCode, dialogueBounds)
                .AddShadedDialogBG(innerBounds)
                .AddTitleBarWithNoMenu("Render Options", () => parent.TryClose())
                .BeginChildElements(innerBounds);

            ComposeBody(_composer);

            return _composer.EndChildElements().Compose();
        }

        private void ComposeBody(GuiComposer composer)
        {
            // TODO: Strings to lang file.
            const int rowPadding = 20;
            const int columnWidth = 500;

            const int fixedElementWidth = 50;
            const int fixedLabelWidth = 200;
            const int fixedHeight = 5;

            var font = CairoFont.WhiteSmallText();
            var row = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 6, columnWidth, fixedHeight);

            var labelBounds = row.FlatCopy().WithFixedWidth(fixedLabelWidth);
            var switchBounds = row.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Highlight Ghost Blocks:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblHighlightGhostBlocks");
            composer.AddSwitch(btnHighlightGhostBlocks_Toggle, switchBounds, "btnHighlightGhostBlocks");
            
            labelBounds = row.FlatCopy().FixedUnder(labelBounds).WithFixedWidth(fixedLabelWidth);
            switchBounds = labelBounds.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Show Ghost Blocks:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblShowGhostBlocks");
            composer.AddSwitch(btnShowGhostBlocks_Toggle, switchBounds, "btnShowGhostBlocks");

            labelBounds = row.FlatCopy().FixedUnder(labelBounds).WithFixedWidth(fixedLabelWidth);
            switchBounds = labelBounds.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Show Blocks:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblShowBlocks");
            composer.AddSwitch(btnShowBlocks_Toggle, switchBounds, "btnShowBlocks");

            labelBounds = row.FlatCopy().FixedUnder(labelBounds).WithFixedWidth(fixedLabelWidth);
            switchBounds = labelBounds.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Show Fluids:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblShowFluids");
            composer.AddSwitch(btnShowFluids_Toggle, switchBounds, "btnShowFluids");

            labelBounds = row.FlatCopy().FixedUnder(labelBounds).WithFixedWidth(fixedLabelWidth);
            switchBounds = labelBounds.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Show Decor:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblShowDecor");
            composer.AddSwitch(btnShowDecor_Toggle, switchBounds, "btnShowDecor");

            labelBounds = row.FlatCopy().FixedUnder(labelBounds).WithFixedWidth(fixedLabelWidth);
            switchBounds = labelBounds.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Show Entities:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblShowEntities");
            composer.AddSwitch(btnShowEntities_Toggle, switchBounds, "btnShowEntities");
        }

        /// <inheritdoc/>
        public void RefreshValues()
        {
            _composer.GetSwitch("btnHighlightGhostBlocks").On = _schematic.HighlightGhostBlocks;
            _composer.GetSwitch("btnShowGhostBlocks").On = _schematic.ShowGhostBlocks;
            _composer.GetSwitch("btnShowBlocks").On = _schematic.ShowSolidBlocks;
            _composer.GetSwitch("btnShowFluids").On = _schematic.ShowLiquids;
            _composer.GetSwitch("btnShowDecor").On = _schematic.ShowDecor;
            _composer.GetSwitch("btnShowEntities").On = _schematic.ShowEntities;
        }

        private void btnHighlightGhostBlocks_Toggle(bool state)
        {
            _schematic.HighlightGhostBlocks = state;
        }
        
        private void btnShowGhostBlocks_Toggle(bool state)
        {
            _schematic.ShowGhostBlocks = state;
        }

        private void btnShowBlocks_Toggle(bool state)
        {
            _schematic.ShowSolidBlocks = state;
        }

        private void btnShowFluids_Toggle(bool state)
        {
            _schematic.ShowLiquids = state;
        }

        private void btnShowDecor_Toggle(bool state)
        {
            _schematic.ShowDecor = state;
        }

        private void btnShowEntities_Toggle(bool state)
        {
            _schematic.ShowEntities = state;
        }
    }
}