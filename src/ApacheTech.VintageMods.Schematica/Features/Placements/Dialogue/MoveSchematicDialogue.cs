using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using ApacheTech.VintageMods.Schematica.Features.Placements.Renderer;
using Gantry.Core.GameContent.GUI;

// TODO: Produce Materials Report.

namespace ApacheTech.VintageMods.Schematica.Features.Placements.Dialogue
{
    [UsedImplicitly]
    public class MoveSchematicDialogue : GenericDialogue
    {
        private readonly LoadedSchematic _schematic;

        /// <inheritdoc/>
        public override bool DisableMouseGrab => false;

        /// <summary>
        ///     Initialises a new instance of the <see cref="MoveSchematicDialogue"/> class.
        /// </summary>
        /// <param name="capi">The client API.</param>
        /// <param name="schematic"></param>
        public MoveSchematicDialogue(ICoreClientAPI capi, LoadedSchematic schematic) : base(capi)
        {
            _schematic = schematic;
        }

        /// <inheritdoc/>
        public override bool CaptureAllInputs() => false;

        protected override void Compose()
        {
            Composers.ClearComposers();
            if (_schematic.Schematic is null)
            {
                TryClose();
                return;
            }

            var windows = new List<IGuiWindow>()
            {
                IOC.Services.Resolve<Windows.MoveCoordinatesWindow>(),
                IOC.Services.Resolve<Windows.OptionsWindow>(),
                IOC.Services.Resolve<Windows.MoveSchematicWindow>()
            };

            foreach (var window in windows)
            {
                Composers[window.Key] = window.Compose(this);
                window.RefreshValues();
            }
            foreach (var composer in Composers.Values) composer.ReCompose();
        }

        protected override void ComposeBody(GuiComposer composer) { }
    }
}