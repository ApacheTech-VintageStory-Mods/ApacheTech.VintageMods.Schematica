using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.FileManager.Dialogue;
using Gantry.Core.Extensions.GameContent.Gui;
using Gantry.Core.GameContent.GUI.Helpers;

namespace ApacheTech.VintageMods.Schematica.Features.Regions.Dialogue.Windows
{
    /// <summary>
    ///     A GUI window that allows the user to save schematics to file.
    /// </summary>
    /// <seealso cref="IGuiWindow" />
    [UsedImplicitly]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hungarian Naming Convention in APL")]
    public class SaveSchematicWindow : IGuiWindow
    {
        private readonly ICoreClientAPI _capi;
        private GuiDialog _parent;

        /// <summary>
        ///     Initialises a new instance of the <see cref="SaveSchematicWindow" /> class.
        /// </summary>
        /// <param name="capi">The client Api.</param>
        public SaveSchematicWindow(ICoreClientAPI capi)
        {
            _capi = capi;
        }

        /// <inheritdoc/>
        public string Key => "SaveSchematic";

        /// <inheritdoc/>
        public GuiComposer Compose(GuiDialog parent)
        {
            _parent = parent;
            var dialogueBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightBottom)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, -GuiStyle.DialogToScreenPadding);

            var innerBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            innerBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = _capi.Gui
                .CreateCompo(parent.ToggleKeyCombinationCode, dialogueBounds)
                .BeginChildElements(innerBounds);
            
            const int fixedElementWidth = 200;
            const int fixedPadding = 5;
            const int fixedHeight = 30;
            
            var buttonBounds = ElementBounds
                .Fixed(0, GuiStyle.TitleBarHeight + 6, fixedElementWidth, fixedHeight)
                .WithFixedPadding(fixedPadding);
            
            composer.AddSmallButton("Save Schematic", btnSaveSchematic_Click, buttonBounds, EnumButtonStyle.Normal,
                EnumTextOrientation.Center, "btnSaveSchematic");

            return composer.EndChildElements().Compose();
        }

        /// <inheritdoc/>
        public void RefreshValues()
        {
        }

        private bool btnSaveSchematic_Click()
        {
            _parent.TryClose();
            var dialogue = IOC.Services.CreateInstance<SaveSchematicDialogue>();

            dialogue.OnClosed += () => _parent.TryOpen();

            dialogue.ToggleGui();
            return true;
        }
    }
}