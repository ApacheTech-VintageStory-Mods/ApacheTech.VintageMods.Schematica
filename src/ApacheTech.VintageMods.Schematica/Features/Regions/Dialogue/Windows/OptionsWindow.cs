using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.Regions.DataStructures;
using Gantry.Core.GameContent.GUI.Helpers;

namespace ApacheTech.VintageMods.Schematica.Features.Regions.Dialogue.Windows
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
        private GuiComposer _composer;
        private readonly SchematicRegion _area;

        /// <summary>
        ///     Initialises a new instance of the <see cref="OptionsWindow" /> class.
        /// </summary>
        /// <param name="capi">The client Api.</param>
        /// <param name="system">The <see cref="ModSystem"/> that hosts the schematic area.</param>
        public OptionsWindow(ICoreClientAPI capi, RegionSelection system)
        {
            _capi = capi; 
            _area = system.Region;
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
                .AddTitleBarWithNoMenu("Options", () => parent.TryClose())
                .BeginChildElements(innerBounds);

            ComposeBody(_composer);

            return _composer.EndChildElements().Compose();
        }

        private void ComposeBody(GuiComposer composer)
        {
            const int rowPadding = 20;
            const int columnWidth = 500;

            const int fixedElementWidth = 50;
            const int fixedLabelWidth = 200;
            const int fixedHeight = 30;

            var font = CairoFont.WhiteSmallText();
            var row = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 6, columnWidth, fixedHeight);

            var labelBounds = row.FlatCopy().WithFixedWidth(fixedLabelWidth);
            var switchBounds = row.FlatCopy().FixedRightOf(labelBounds, rowPadding).WithFixedWidth(fixedElementWidth);

            composer.AddStaticText("Display Preview Outline:", font, EnumTextOrientation.Right, labelBounds.WithFixedOffset(0, 5), "lblDisplayPreviewOutlines");
            composer.AddSwitch(btnPreviewOutlines_Toggle, switchBounds, "btnPreviewOutlines");
        }

        /// <inheritdoc/>
        public void RefreshValues()
        {
            _composer.GetSwitch("btnPreviewOutlines").On = _area.IsHighlighted;
            _area.RefreshHighlights();
        }

        private void btnPreviewOutlines_Toggle(bool state)
        {
            _area.IsHighlighted = state;
            RefreshValues();
        }
    }
}