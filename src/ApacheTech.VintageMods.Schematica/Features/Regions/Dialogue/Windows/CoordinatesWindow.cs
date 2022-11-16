using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.Regions.DataStructures;

namespace ApacheTech.VintageMods.Schematica.Features.Regions.Dialogue.Windows
{
    /// <summary>
    ///     A GUI window that allows the user to specify a schematic region.
    /// </summary>
    /// <seealso cref="IGuiWindow" />
    [UsedImplicitly]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hungarian Naming Convention in APL")]
    public class CoordinatesWindow : IGuiWindow
    {
        private readonly ICoreClientAPI _capi;
        private readonly SchematicRegion _area;
        private GuiComposer _composer;
        private bool _textChanging;

        /// <summary>
        ///     Initialises a new instance of the <see cref="CoordinatesWindow"/> class.
        /// </summary>
        /// <param name="capi">The client Api.</param>
        /// <param name="system">The <see cref="ModSystem"/> that hosts the schematic area.</param>
        public CoordinatesWindow(ICoreClientAPI capi, RegionSelection system)
        {
            _capi = capi; 
            _area = system.Region;
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

            composer.AddSmallButton("Start Point", btnStartPoint_Click, left, EnumButtonStyle.Normal,
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

            var right = ElementBounds.Fixed(columnWidth + rowPadding, GuiStyle.TitleBarHeight + 6, columnWidth,
                fixedHeight);

            composer.AddSmallButton("End Point", btnEndPoint_Click, right, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnEndPoint");

            right = right.BelowCopy(fixedDeltaY: rowPadding);

            var rightDescriptionLabelBounds = right.FlatCopy().WithFixedWidth(0);
            var rightTextBoxBounds = right.FlatCopy().FixedRightOf(rightDescriptionLabelBounds, rowPadding)
                .WithFixedWidth(fixedTextBoxWidth);
            var rightMinusButtonBounds = right.FlatCopy().FixedRightOf(rightTextBoxBounds, columnPadding)
                .WithFixedWidth(rowPadding);
            var rightPlusButtonBounds = right.FlatCopy().FixedRightOf(rightMinusButtonBounds, columnPadding)
                .WithFixedWidth(rowPadding);
            var rightValueLabelBounds = right.FlatCopy().FixedRightOf(rightPlusButtonBounds, columnPadding)
                .WithFixedWidth(fixedValueLabelWidth);

            composer.AddStaticText("X:", font, EnumTextOrientation.Left,
                rightDescriptionLabelBounds.WithFixedOffset(0, 5), "lblEndPointX");
            composer.AddTextInput(rightTextBoxBounds, txtEndPointX_TextChanged, font, "txtEndPointX");
            composer.AddSmallButton("-", btnDecrementEndPointX_Click, rightMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementEndPointX");
            composer.AddSmallButton("+", btnIncrementEndPointX_Click, rightPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementEndPointX");
            composer.AddDynamicText("", font, rightValueLabelBounds.WithFixedOffset(0, 5), "lblEndPointXValue");

            rightDescriptionLabelBounds = rightDescriptionLabelBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightTextBoxBounds = rightTextBoxBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightMinusButtonBounds = rightMinusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightPlusButtonBounds = rightPlusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightValueLabelBounds = rightValueLabelBounds.BelowCopy(fixedDeltaY: rowPadding);

            composer.AddStaticText("Y:", font, EnumTextOrientation.Left, rightDescriptionLabelBounds, "lblEndPointY");
            composer.AddTextInput(rightTextBoxBounds, txtEndPointY_TextChanged, font, "txtEndPointY");
            composer.AddSmallButton("-", btnDecrementEndPointY_Click, rightMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementEndPointY");
            composer.AddSmallButton("+", btnIncrementEndPointY_Click, rightPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementEndPointY");
            composer.AddDynamicText("", font, rightValueLabelBounds, "lblEndPointYValue");

            rightDescriptionLabelBounds = rightDescriptionLabelBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightTextBoxBounds = rightTextBoxBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightMinusButtonBounds = rightMinusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightPlusButtonBounds = rightPlusButtonBounds.BelowCopy(fixedDeltaY: rowPadding);
            rightValueLabelBounds = rightValueLabelBounds.BelowCopy(fixedDeltaY: rowPadding);

            composer.AddStaticText("Z:", font, EnumTextOrientation.Left, rightDescriptionLabelBounds, "lblEndPointZ");
            composer.AddTextInput(rightTextBoxBounds, txtEndPointZ_TextChanged, font, "txtEndPointZ");
            composer.AddSmallButton("-", btnDecrementEndPointZ_Click, rightMinusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnDecrementEndPointZ");
            composer.AddSmallButton("+", btnIncrementEndPointZ_Click, rightPlusButtonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnIncrementEndPointZ");
            composer.AddDynamicText("", font, rightValueLabelBounds, "lblEndPointZValue");
        }

        #endregion

        #region Refresh Values

        /// <inheritdoc/>
        public void RefreshValues()
        {
            if (!_textChanging) RefreshTextBoxes();
            RefreshLabels();
            _textChanging = false;

            _area.RefreshHighlights();
        }

        private void RefreshTextBoxes()
        {
            _composer.GetTextInput("txtStartPointX").SetValue(_area.RelativeStartPos.X);
            _composer.GetTextInput("txtStartPointY").SetValue(_area.RelativeStartPos.Y);
            _composer.GetTextInput("txtStartPointZ").SetValue(_area.RelativeStartPos.Z);

            _composer.GetTextInput("txtEndPointX").SetValue(_area.RelativeEndPos.X);
            _composer.GetTextInput("txtEndPointY").SetValue(_area.RelativeEndPos.Y);
            _composer.GetTextInput("txtEndPointZ").SetValue(_area.RelativeEndPos.Z);
        }

        private void RefreshLabels()
        {
            _composer.GetDynamicText("lblStartPointXValue").SetNewText(_area.RelativeStartPos.X.ToString("D"));
            _composer.GetDynamicText("lblStartPointYValue").SetNewText(_area.RelativeStartPos.Y.ToString("D"));
            _composer.GetDynamicText("lblStartPointZValue").SetNewText(_area.RelativeStartPos.Z.ToString("D"));

            _composer.GetDynamicText("lblEndPointXValue").SetNewText(_area.RelativeEndPos.X.ToString("D"));
            _composer.GetDynamicText("lblEndPointYValue").SetNewText(_area.RelativeEndPos.Y.ToString("D"));
            _composer.GetDynamicText("lblEndPointZValue").SetNewText(_area.RelativeEndPos.Z.ToString("D"));
        }

        #endregion

        #region Button Event Handlers

        private bool btnStartPoint_Click()
        {
            var pos = _capi.World.Player.Entity.Pos.XYZInt.ToBlockPos();
            _area.StartPos.Set(pos);
            RefreshTextBoxes();
            RefreshValues();
            return true;
        }

        private bool btnEndPoint_Click()
        {
            var pos = _capi.World.Player.Entity.Pos.XYZInt.ToBlockPos();
            _area.EndPos.Set(pos);
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
            _area.StartPos.SetX(x);
            RefreshValues();
        }

        private void txtStartPointY_TextChanged(string value)
        {
            if (!int.TryParse(value, out var y)) return;
            _textChanging = true;
            _area.StartPos.SetY(y);
            RefreshValues();
        }

        private void txtStartPointZ_TextChanged(string value)
        {
            if (!int.TryParse(value, out var z)) return;
            _textChanging = true;
            z += _capi.World.DefaultSpawnPosition.XYZInt.Z;
            _area.StartPos.SetZ(z);
            RefreshValues();
        }

        private void txtEndPointX_TextChanged(string value)
        {
            if (!int.TryParse(value, out var x)) return;
            _textChanging = true;
            x += _capi.World.DefaultSpawnPosition.XYZInt.X;
            _area.EndPos.SetX(x);
            RefreshValues();
        }

        private void txtEndPointY_TextChanged(string value)
        {
            if (!int.TryParse(value, out var y)) return;
            _textChanging = true;
            _area.EndPos.SetY(y);
            RefreshValues();
        }

        private void txtEndPointZ_TextChanged(string value)
        {
            if (!int.TryParse(value, out var z)) return;
            _textChanging = true;
            z += _capi.World.DefaultSpawnPosition.XYZInt.Z;
            _area.EndPos.SetZ(z);
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
            return ChangeValue(() => _area.StartPos.DecrementX());
        }

        private bool btnIncrementStartPointX_Click()
        {
            return ChangeValue(() => _area.StartPos.IncrementX());
        }

        private bool btnDecrementStartPointY_Click()
        {
            return ChangeValue(() => _area.StartPos.DecrementY());
        }

        private bool btnIncrementStartPointY_Click()
        {
            return ChangeValue(() => _area.StartPos.IncrementY());
        }

        private bool btnDecrementStartPointZ_Click()
        {
            return ChangeValue(() => _area.StartPos.DecrementZ());
        }

        private bool btnIncrementStartPointZ_Click()
        {
            return ChangeValue(() => _area.StartPos.IncrementZ());
        }

        private bool btnDecrementEndPointX_Click()
        {
            return ChangeValue(() => _area.EndPos.DecrementX());
        }

        private bool btnIncrementEndPointX_Click()
        {
            return ChangeValue(() => _area.EndPos.IncrementX());
        }

        private bool btnDecrementEndPointY_Click()
        {
            return ChangeValue(() => _area.EndPos.DecrementY());
        }

        private bool btnIncrementEndPointY_Click()
        {
            return ChangeValue(() => _area.EndPos.IncrementY());
        }

        private bool btnDecrementEndPointZ_Click()
        {
            return ChangeValue(() => _area.EndPos.DecrementZ());
        }

        private bool btnIncrementEndPointZ_Click()
        {
            return ChangeValue(() => _area.EndPos.IncrementZ());
        }

        #endregion
    }
}