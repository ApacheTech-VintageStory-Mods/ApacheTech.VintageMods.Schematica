using ApacheTech.VintageMods.Schematica.Core.Abstractions;
using Gantry.Core.GameContent.GUI;

// TODO: Produce Materials Report.

namespace ApacheTech.VintageMods.Schematica.Features.Regions.Dialogue
{
    [UsedImplicitly]
    public class RegionSelectionDialogue : GenericDialogue
    {
        private readonly List<IGuiWindow> _windows = new()
        {
            IOC.Services.Resolve<Windows.CoordinatesWindow>(),
            IOC.Services.Resolve<Windows.OptionsWindow>(),
            IOC.Services.Resolve<Windows.SaveSchematicWindow>()
        };

        /// <inheritdoc/>
        public override bool DisableMouseGrab => false;

        /// <summary>
        ///     Initialises a new instance of the <see cref="RegionSelectionDialogue"/> class.
        /// </summary>
        /// <param name="capi">The client API.</param>
        public RegionSelectionDialogue(ICoreClientAPI capi) : base(capi)
        {
        }

        /// <inheritdoc/>
        public override bool CaptureAllInputs() => false;

        protected override void Compose()
        {
            Composers.ClearComposers();
            foreach (var window in _windows)
            {
                Composers[window.Key] = window.Compose(this);
                window.RefreshValues();
            }
            foreach (var composer in Composers.Values) composer.ReCompose();
        }

        protected override void ComposeBody(GuiComposer composer) { }
    }
}