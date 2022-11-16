using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.Placements.Renderer;

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Dialogue.Windows
{
    /// <summary>
    ///     A GUI window that allows the user to specify a schematic region.
    /// </summary>
    /// <seealso cref="IGuiWindow" />
    [UsedImplicitly]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hungarian Naming Convention in APL")]
    public class MoveCoordinatesWindow : IGuiWindow
    {
        private readonly ICoreClientAPI _capi;
        private readonly LoadedSchematic _schematic;
        private GuiComposer _composer;
        private bool _textChanging;

        /// <summary>
        ///     Initialises a new instance of the <see cref="MoveCoordinatesWindow"/> class.
        /// </summary>
        /// <param name="capi">The client Api.</param>
        /// <param name="schematic">The <see cref="LoadedSchematic"/> that hosts the schematic area.</param>
        public MoveCoordinatesWindow(ICoreClientAPI capi, LoadedSchematic schematic)
        {
            _capi = capi;
            _schematic = schematic;
        }

        /// <inheritdoc/>
        public string Key => "Coordinates";

        #region Form Composition

        /// <inheritdoc/>
        public GuiComposer Compose(GuiDialog parent)
        {
            var dialogueBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var innerBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            innerBounds.BothSizing = ElementSizing.FitToChildren;

            _composer = _capi.Gui
                .CreateCompo(parent.ToggleKeyCombinationCode, dialogueBounds)
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
            const int fixedValueLabelWidth = 100;
            const int fixedHeight = 30;

            var font = CairoFont.WhiteSmallText();

            var left = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 6, columnWidth, fixedHeight);

            composer.AddSmallButton("Move Here", btnStartPoint_Click, left, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnStartPoint");

            left = left.BelowCopy(fixedDeltaY: rowPadding);

            var leftDescriptionLabelBounds = left.FlatCopy().WithFixedWidth(0);
            var leftTextBoxBounds = left.FlatCopy().FixedRightOf(leftDescriptionLabelBounds, rowPadding)
                .WithFixedWidth(fixedTextBoxWidth);
            var leftMinusButtonBounds = left.FlatCopy().FixedRightOf(leftTextBoxBounds, columnPadding)
                .WithFixedWidth(rowPadding);
            var leftPlusButtonBounds = left.FlatCopy().FixedRightOf(leftMinusButtonBounds, columnPadding)
                .WithFixedWidth(rowPadding);
            var leftValueLabelBounds = left.FlatCopy().FixedRightOf(leftPlusButtonBounds, columnPadding)
                .WithFixedWidth(fixedValueLabelWidth);

            composer.AddStaticText("X:", font, EnumTextOrientation.Left,
                leftDescriptionLabelBounds.WithFixedOffset(0, 5), "lblStartPointX");
            composer.AddTextInput(leftTextBoxBounds, txtStartPointX_TextChanged, font, "txtStartPointX");
            composer.AddSmallButton("-", btnDecrementStartPointX_Click, leftMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementStartPointX");
            composer.AddSmallButton("+", btnIncrementStartPointX_Click, leftPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementStartPointX");
            composer.AddDynamicText("", font, leftValueLabelBounds.WithFixedOffset(0, 5), "lblStartPointXValue");

            leftDescriptionLabelBounds = leftDescriptionLabelBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftTextBoxBounds = leftTextBoxBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftMinusButtonBounds = leftMinusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftPlusButtonBounds = leftPlusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftValueLabelBounds = leftValueLabelBounds.BelowCopy(fixedDeltaY: rowPadding);

            composer.AddStaticText("Y:", font, EnumTextOrientation.Left, leftDescriptionLabelBounds, "lblStartPointY");
            composer.AddTextInput(leftTextBoxBounds, txtStartPointY_TextChanged, font, "txtStartPointY");
            composer.AddSmallButton("-", btnDecrementStartPointY_Click, leftMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementStartPointY");
            composer.AddSmallButton("+", btnIncrementStartPointY_Click, leftPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementStartPointY");
            composer.AddDynamicText("", font, leftValueLabelBounds, "lblStartPointYValue");

            leftDescriptionLabelBounds = leftDescriptionLabelBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftTextBoxBounds = leftTextBoxBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftMinusButtonBounds = leftMinusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftPlusButtonBounds = leftPlusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            leftValueLabelBounds = leftValueLabelBounds.BelowCopy(fixedDeltaY: rowPadding);

            composer.AddStaticText("Z:", font, EnumTextOrientation.Left, leftDescriptionLabelBounds, "lblStartPointZ");
            composer.AddTextInput(leftTextBoxBounds, txtStartPointZ_TextChanged, font, "txtStartPointZ");
            composer.AddSmallButton("-", btnDecrementStartPointZ_Click, leftMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementStartPointZ");
            composer.AddSmallButton("+", btnIncrementStartPointZ_Click, leftPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementStartPointZ");
            composer.AddDynamicText("", font, leftValueLabelBounds, "lblStartPointZValue");
        }

        #endregion

        #region Refresh Values

        /// <inheritdoc/>
        public void RefreshValues()
        {
            if (!_textChanging) RefreshTextBoxes();
            RefreshLabels();
            _textChanging = false;
        }

        private void RefreshTextBoxes()
        {
            _composer.GetTextInput("txtStartPointX").SetValue(_schematic.StartPos.RelativeToSpawn().X);
            _composer.GetTextInput("txtStartPointY").SetValue(_schematic.StartPos.RelativeToSpawn().Y);
            _composer.GetTextInput("txtStartPointZ").SetValue(_schematic.StartPos.RelativeToSpawn().Z);
        }

        private void RefreshLabels()
        {
            _composer.GetDynamicText("lblStartPointXValue").SetNewText(_schematic.StartPos.RelativeToSpawn().X.ToString("D"));
            _composer.GetDynamicText("lblStartPointYValue").SetNewText(_schematic.StartPos.RelativeToSpawn().Y.ToString("D"));
            _composer.GetDynamicText("lblStartPointZValue").SetNewText(_schematic.StartPos.RelativeToSpawn().Z.ToString("D"));
        }

        #endregion

        #region Button Event Handlers

        private bool btnStartPoint_Click()
        {
            var pos = _capi.World.Player.Entity.Pos.XYZInt.ToBlockPos();
            _schematic.StartPos.Set(pos);
            RefreshTextBoxes();
            RefreshValues();
            return true;
        }

        #endregion

        #region TextBox Event Handlers

        private void txtStartPointX_TextChanged(string value)
        {
            if (!int.TryParse(value, out var x)) return;
            _textChanging = true;
            x += _capi.World.DefaultSpawnPosition.XYZInt.X;
            _schematic.StartPos.SetX(x);
            RefreshValues();
        }

        private void txtStartPointY_TextChanged(string value)
        {
            if (!int.TryParse(value, out var y)) return;
            _textChanging = true;
            _schematic.StartPos.SetY(y);
            RefreshValues();
        }

        private void txtStartPointZ_TextChanged(string value)
        {
            if (!int.TryParse(value, out var z)) return;
            _textChanging = true;
            z += _capi.World.DefaultSpawnPosition.XYZInt.Z;
            _schematic.StartPos.SetZ(z);
            RefreshValues();
        }

        #endregion

        #region Increment/Decrement Event Handlers

        private bool ChangeValue(Action action)
        {
            action();
            RefreshTextBoxes();
            RefreshValues();
            return true;
        }

        private bool btnDecrementStartPointX_Click()
        {
            return ChangeValue(() => _schematic.StartPos.DecrementX());
        }

        private bool btnIncrementStartPointX_Click()
        {
            return ChangeValue(() => _schematic.StartPos.IncrementX());
        }

        private bool btnDecrementStartPointY_Click()
        {
            return ChangeValue(() => _schematic.StartPos.DecrementY());
        }

        private bool btnIncrementStartPointY_Click()
        {
            return ChangeValue(() => _schematic.StartPos.IncrementY());
        }

        private bool btnDecrementStartPointZ_Click()
        {
            return ChangeValue(() => _schematic.StartPos.DecrementZ());
        }

        private bool btnIncrementStartPointZ_Click()
        {
            return ChangeValue(() => _schematic.StartPos.IncrementZ());
        }

        #endregion
    }
}