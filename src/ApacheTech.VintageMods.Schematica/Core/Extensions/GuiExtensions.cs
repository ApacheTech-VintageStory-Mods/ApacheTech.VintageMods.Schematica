namespace ApacheTech.VintageMods.Schematica.Core.Extensions
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class GuiExtensions
    {
        public static T GetElement<T>(this GuiDialog dialogue, string name) where T : GuiElement
        {
            foreach (var composer in dialogue.Composers.Values)
            {
                try
                {
                    var element = composer.GetElement(name);
                    if (element is null) continue;
                    return (T)element;
                }
                catch
                {
                    // ignored
                }
            }
            return null;
        }
    }
}