using ApacheTech.VintageMods.FluentChatCommands;
using ApacheTech.VintageMods.FluentChatCommands.Extensions;
using Gantry.Services.FileSystem.DependencyInjection;
using Gantry.Services.HarmonyPatches.DependencyInjection;
using Gantry.Services.Network.DependencyInjection;

namespace ApacheTech.VintageMods.Schematica
{
    /// <summary>
    ///     Entry-point for the mod. This class will configure and build the IOC Container, and Service list for the rest of the mod.
    ///     
    ///     Registrations performed within this class should be global scope; by convention, features should aim to be as stand-alone as they can be.
    /// </summary>
    /// <remarks>
    ///     Only one derived instance of this class should be added to any single mod within
    ///     the VintageMods domain. This class will enable Dependency Injection, and add all
    ///     of the domain services. Derived instances should only have minimal functionality, 
    ///     instantiating, and adding Application specific services to the IOC Container.
    /// </remarks>
    /// <seealso cref="ModHost" />
    [UsedImplicitly]
    public sealed class Program : ModHost
    {
        /// <summary>
        ///     Configures any services that need to be added to the IO Container, on the client side.
        /// </summary>
        /// <param name="services">The as-of-yet un-built services container.</param>
        protected override void ConfigureUniversalModServices(IServiceCollection services)
        {
            services.AddFileSystemService(o => o.RegisterSettingsFiles = true);
            services.AddHarmonyPatchingService(o => o.AutoPatchModAssembly = true);
            services.AddNetworkService();
        }

        /// <summary>
        /// Full start to the mod on the client side.
        /// <br />Note, in multi-player games, the server assets (blocks, items, entities, recipes) have not yet been received and so no blocks etc. are yet registered
        /// <br />For code that must run only after we have blocks, items, entities and recipes all registered and loaded, add your method to event BlockTexturesLoaded
        /// </summary>
        /// <param name="capi"></param>
        public override void StartClientSide(ICoreClientAPI capi)
        {
            capi.RegisterFluentCommand("schematica")!
                .WithDescription(LangEx.FeatureString("Schematica", "CommandDescription"));
        }

        /// <summary>
        ///     If this mod allows runtime reloading, you must implement this method to unregister any listeners / handlers
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            FluentChat.ClearCommands(UApi);
        }
    }
}