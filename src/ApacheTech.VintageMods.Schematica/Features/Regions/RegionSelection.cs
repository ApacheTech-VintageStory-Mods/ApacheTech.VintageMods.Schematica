using ApacheTech.VintageMods.FluentChatCommands.Extensions;
using ApacheTech.VintageMods.Schematica.Features.FileManager.Dialogue;
using ApacheTech.VintageMods.Schematica.Features.Highlighter;
using ApacheTech.VintageMods.Schematica.Features.Regions.DataStructures;
using ApacheTech.VintageMods.Schematica.Features.Regions.Dialogue;
using ApacheTech.VintageMods.Schematica.Features.Regions.Dialogue.Windows;
using Gantry.Core.DependencyInjection.Registration;
using Gantry.Core.Extensions.GameContent.Gui;

namespace ApacheTech.VintageMods.Schematica.Features.Regions
{
    // .schematica region set -413 2 220 -421 8 212

    /// <summary>
    ///     Handles access to schematic regions.
    /// </summary>
    /// <seealso cref="ClientModSystem" />
    /// <seealso cref="IClientServiceRegistrar" />
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RegionSelection : ClientModSystem, IClientServiceRegistrar
    {
        /// <summary>
        ///     The area taken up by the schematic.
        /// </summary>
        public SchematicRegion Region { get; private set; }

        /// <summary>
        ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureClientModServices(IServiceCollection services)
        {
            services.AddSingleton<IBlockHighlighter, BlockHighlighter>();
            services.AddSingleton<CoordinatesWindow>();
            services.AddSingleton<SaveSchematicWindow>();
            services.AddSingleton<OptionsWindow>();
            services.AddSingleton<RegionSelectionDialogue>();
            services.AddSingleton<LoadSchematicDialogue>();
        }

        /// <summary>
        ///     Full start to the mod on the client side.
        ///     Note, in multi-player games, the server assets (blocks, items, entities, recipes) have not yet been received and so no blocks etc. are yet registered
        ///     For code that must run only after we have blocks, items, entities and recipes all registered and loaded, add your method to event BlockTexturesLoaded
        /// </summary>
        /// <param name="capi">The Client API.</param>
        public override void StartClientSide(ICoreClientAPI capi)
        {
            Region = IOC.Services.CreateInstance<SchematicRegion>();

            capi.FluentCommand("schematica")!
                .HasSubCommand("load", r => r.WithHandler(LoadSchematic).Build())
                .HasSubCommand("region", r => r
                    .HasSubCommand("copy", s => s.WithHandler(CopyRegion).Build())
                    .HasSubCommand("end", s => s.WithHandler(SetRegionEndPos).Build())
                    .HasSubCommand("export", s => s.WithHandler(ExportRegion).Build())
                    .HasSubCommand("hide", s => s.WithHandler(HideRegion).Build())
                    .HasSubCommand("new", s => s.WithHandler(NewRegion).Build())
                    .HasSubCommand("reset", s => s.WithHandler(ResetRegion).Build())
                    .HasSubCommand("save", s => s.WithHandler(SaveRegion).Build())
                    .HasSubCommand("set", s => s.WithHandler(SetRegion).Build())
                    .HasSubCommand("show", s => s.WithHandler(ShowRegion).Build())
                    .HasSubCommand("start", s => s.WithHandler(SetRegionStartPos).Build())
                    .Build());

            capi.Input.RegisterGuiDialogueHotKey(IOC.Services.Resolve<RegionSelectionDialogue>(), "Choose Schematic Region", GlKeys.F9);
            capi.Input.RegisterGuiDialogueHotKey(IOC.Services.Resolve<LoadSchematicDialogue>(), "Load Schematic", GlKeys.F9, false, false, true);
        }

        private void LoadSchematic(IPlayer player, int groupId, CmdArgs args)
        {
            throw new NotImplementedException();
        }

        private void ExportRegion(IPlayer player, int groupId, CmdArgs args)
        {
            // TODO: Export region in structure format.
            // var example = nameof(Sandbox.SaveStructureFile);
        }

        private void SetRegionStartPos(IPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length == 3)
            {
                var mapMiddle = Capi.World.DefaultSpawnPosition.XYZInt;

                Region.StartPos.SetX(args.PopInt().GetValueOrDefault() + mapMiddle.X);
                Region.StartPos.SetY(args.PopInt().GetValueOrDefault());
                Region.StartPos.SetZ(args.PopInt().GetValueOrDefault() + mapMiddle.Z);
                return;
            }

            if (player.CurrentBlockSelection is not null)
            {
                Region.StartPos.Set(player.CurrentBlockSelection.Position);
                return;
            }

            var playerPos = player.Entity.Pos.XYZInt.AsBlockPos;
            Region.StartPos.Set(playerPos);
        }

        private void SetRegionEndPos(IPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length == 3)
            {
                var mapMiddle = Capi.World.DefaultSpawnPosition.XYZInt;

                Region.EndPos.SetX(args.PopInt().GetValueOrDefault() + mapMiddle.X);
                Region.EndPos.SetY(args.PopInt().GetValueOrDefault());
                Region.EndPos.SetZ(args.PopInt().GetValueOrDefault() + mapMiddle.Z);
                return;
            }

            if (player.CurrentBlockSelection is not null)
            {
                Region.EndPos.Set(player.CurrentBlockSelection.Position);
                return;
            }

            var playerPos = player.Entity.Pos.XYZInt.AsBlockPos;
            Region.EndPos.Set(playerPos);
        }

        private void ResetRegion(IPlayer player, int groupId, CmdArgs args)
        {
            Region = IOC.Services.CreateInstance<SchematicRegion>();
            Region.RefreshHighlights();
        }

        private void CopyRegion(IPlayer player, int groupId, CmdArgs args)
        {
            var s = Region.RelativeStartPos;
            var e = Region.RelativeEndPos;
            Capi.Forms.SetClipboardText($".schematica region set {s.X} {s.Y} {s.Z} {e.X} {e.Y} {e.Z}");
        }

        private void NewRegion(IPlayer player, int groupId, CmdArgs args)
        {
            var playerPos = Capi.World.Player.Entity.Pos.XYZInt.AsBlockPos;
            Region.StartPos.Set(playerPos);
            Region.EndPos.Set(playerPos);
            Region.RefreshHighlights();
        }

        private void SaveRegion(IPlayer player, int groupId, CmdArgs args)
        {
            var fileName = args.PopWord($"Unknown-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}");
            var path = Path.Combine(GamePaths.DataPath, "Schematics", fileName.EnsureEndsWith(".schematic"));



            // TODO: Save schematic.
        }

        private void HideRegion(IPlayer player, int groupId, CmdArgs args)
        {
            Region.IsHighlighted = false;
            Region.RefreshHighlights();
        }

        private void ShowRegion(IPlayer player, int groupId, CmdArgs args)
        {
            Region.IsHighlighted = true;
            Region.RefreshHighlights();
        }

        private void SetRegion(IPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length != 6)
            {
                Capi.ShowChatMessage("Please specify coordinates for start, and end positions. .schematica region set x1 y1 z1 x2 y2 z2");
            }

            var mapMiddle = Capi.World.DefaultSpawnPosition.XYZInt;

            Region.StartPos.SetX(args.PopInt().GetValueOrDefault() + mapMiddle.X);
            Region.StartPos.SetY(args.PopInt().GetValueOrDefault());
            Region.StartPos.SetZ(args.PopInt().GetValueOrDefault() + mapMiddle.Z);

            Region.EndPos.SetX(args.PopInt().GetValueOrDefault() + mapMiddle.X);
            Region.EndPos.SetY(args.PopInt().GetValueOrDefault());
            Region.EndPos.SetZ(args.PopInt().GetValueOrDefault() + mapMiddle.Z);

            Region.RefreshHighlights();
        }
    }
}
