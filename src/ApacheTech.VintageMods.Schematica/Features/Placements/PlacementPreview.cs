using ApacheTech.VintageMods.Schematica.Features.Highlighter;
using ApacheTech.VintageMods.Schematica.Features.Placements.Dialogue.Windows;
using ApacheTech.VintageMods.Schematica.Features.Placements.Dialogue;
using Gantry.Core.Extensions.GameContent.Gui;
using ApacheTech.VintageMods.Schematica.Features.Placements.Renderer;

namespace ApacheTech.VintageMods.Schematica.Features.Placements
{
    [UsedImplicitly]
    public class PlacementPreview : ClientModSystem, IClientServiceRegistrar
    {
        public void ConfigureClientModServices(IServiceCollection services)
        {
            services.AddSingleton<LoadedSchematic>();

            services.AddSingleton<PlacementPreviewHighlighter>();
            services.AddTransient<SchematicPlacementRenderer>();

            services.AddSingleton<MoveCoordinatesWindow>();
            services.AddSingleton<OptionsWindow>();
            services.AddSingleton<MoveSchematicWindow>();
            services.AddSingleton<MoveSchematicDialogue>();
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            capi.Input.RegisterGuiDialogueHotKey(IOC.Services.Resolve<MoveSchematicDialogue>(), "Move Schematic", GlKeys.F9, false, true);
        }
    }
}
