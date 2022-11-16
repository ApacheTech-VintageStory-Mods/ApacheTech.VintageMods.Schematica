using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.Placements.Renderer;
using Gantry.Core.Extensions.Helpers;
using Gantry.Core.GameContent.GUI.Helpers;

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Dialogue.Windows
{
    /// <summary>
    ///     A GUI window that allows the user to save schematics to file.
    /// </summary>
    /// <seealso cref="IGuiWindow" />
    [UsedImplicitly]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hungarian Naming Convention in APL")]
    public class MoveSchematicWindow : IGuiWindow
    {
        private readonly ICoreClientAPI _capi;
        private readonly LoadedSchematic _schematic;
        private GuiDialog _parent;
        private GuiComposer _composer;

        /// <summary>
        ///     Initialises a new instance of the <see cref="MoveSchematicWindow" /> class.
        /// </summary>
        /// <param name="capi">The client Api.</param>
        /// <param name="schematic"></param>
        public MoveSchematicWindow(ICoreClientAPI capi, LoadedSchematic schematic)
        {
            _capi = capi;
            _schematic = schematic;
        }

        /// <inheritdoc/>
        public string Key => "MoveSchematic";

        /// <inheritdoc/>
        public GuiComposer Compose(GuiDialog parent)
        {
            _parent = parent;
            var dialogueBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightBottom)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, -GuiStyle.DialogToScreenPadding);

            var innerBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            innerBounds.BothSizing = ElementSizing.FitToChildren;

            _composer = _capi.Gui
                .CreateCompo(parent.ToggleKeyCombinationCode, dialogueBounds)
                .AddShadedDialogBG(innerBounds)
                .AddTitleBarWithNoMenu("Schematic Placement", () => parent.TryClose())
                .BeginChildElements(innerBounds);

            ComposeBody(_composer);

            return _composer.EndChildElements().Compose();
        }

        private void ComposeBody(GuiComposer composer)
        {
            const int columnPadding = 10;
            const int rowPadding = 20;
            const int columnWidth = 300;

            const int fixedTextBoxWidth = 150;
            const int fixedLayerButtonWidth = 20;
            const int fixedHeight = 30;

            const int fixedDropDownWidth = 210;
            const int fixedElementWidth = 200;
            const int fixedPadding = 5;

            var font = CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Justify);

            var left = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 6, fixedDropDownWidth, fixedHeight);

            var memberValues = Enum.GetNames(typeof(SchematicLayerType)).ToArray();
            var displayNames = memberValues.Select(p => p.SplitPascalCase()).ToArray();

            composer.AddDropDown(memberValues, displayNames, 0, cbxRenderLayers_SelectedIndexChanged, left, font, "cbxRenderLayers");

            var layerTextBoxBounds = left.FlatCopy().FixedUnder(left, -rowPadding).WithFixedWidth(fixedTextBoxWidth);
            var layerMinusButtonBounds = layerTextBoxBounds.FlatCopy().FixedRightOf(layerTextBoxBounds, columnPadding)
                .WithFixedWidth(fixedLayerButtonWidth);
            var layerPlusButtonBounds = layerTextBoxBounds.FlatCopy().FixedRightOf(layerMinusButtonBounds, columnPadding)
                .WithFixedWidth(fixedLayerButtonWidth);

            var values = Enumerable.Range(0, _schematic.Schematic.SizeY).Select(p => p.ToString()).ToArray();

            composer.AddDropDown(values, values, 0, cbxLayer_SelectedIndexChanged, layerTextBoxBounds, font, "cbxLayer");
            composer.AddSmallButton("-", btnDecrementLayer_Click, layerMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementLayer");
            composer.AddSmallButton("+", btnIncrementLayer_Click, layerPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementLayer");

            var rotateButtonBounds = ElementBounds
                .Fixed(0, GuiStyle.TitleBarHeight + 6, fixedElementWidth, fixedHeight)
                .FixedUnder(layerTextBoxBounds, -rowPadding)
                .WithFixedPadding(fixedPadding);

            composer.AddSmallButton("Rotate Schematic", btnRotateSchematic_Click, rotateButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnRotateSchematic");

            var mirrorButtonBounds = ElementBounds
                .Fixed(0, GuiStyle.TitleBarHeight + 6, fixedElementWidth, fixedHeight)
                .FixedUnder(rotateButtonBounds, -rowPadding)
                .WithFixedPadding(fixedPadding);

            composer.AddSmallButton("Mirror Schematic", btnMirrorSchematic_Click, mirrorButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnMirrorSchematic");
        }

        private void cbxRenderLayers_SelectedIndexChanged(string value, bool selected)
        {
            _schematic.RenderLayers = EnumEx.Parse<SchematicLayerType>(value);
        }

        private void cbxLayer_SelectedIndexChanged(string value, bool selected)
        {
            _schematic.Layer = int.Parse(value);
        }

        private bool btnDecrementLayer_Click()
        {
            if (_schematic.Layer == 0) return false;
            _schematic.Layer -= 1;
            RefreshValues();
            return true;
        }

        private bool btnIncrementLayer_Click()
        {
            if (_schematic.Layer == _schematic.Metadata.Size.Y) return false;
            _schematic.Layer += 1;
            RefreshValues();
            return true;
        }

        /// <inheritdoc/>
        public void RefreshValues()
        {
            var layerType = _schematic.RenderLayers.ToString();
            var cbxRenderLayers = _composer.GetDropDown("cbxRenderLayers");
            cbxRenderLayers.listMenu.MaxHeight = 100;
            cbxRenderLayers.SetSelectedValue(layerType);

            var cbxLayer = _composer.GetDropDown("cbxLayer");
            cbxLayer.listMenu.MaxHeight = 100;
            cbxLayer.SetSelectedValue(_schematic.Layer.ToString());
        }

        private bool btnRotateSchematic_Click()
        {
            var rotation = _schematic.Rotation + 90;
            if (rotation >= 360) rotation = 0;
            _schematic.Rotation = rotation;
            _schematic.Schematic.TransformWhilePacked(_capi.World, EnumOrigin.StartPos, _schematic.Rotation);
            _schematic.RequiresRedraw = true;
            return true;
        }

        private bool btnMirrorSchematic_Click()
        {
            _schematic.Schematic.TransformWhilePacked(_capi.World, EnumOrigin.StartPos, 0, EnumAxis.Z);
            _schematic.RequiresRedraw = true;
            return true;
        }
    }
}