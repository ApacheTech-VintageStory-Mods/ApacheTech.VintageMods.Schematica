namespace ApacheTech.VintageMods.Schematica.Core.Extensions
{
    public static class BooleanExtensions
    {
        public static void If(this bool condition, Action trueAction)
        {
            if (condition) trueAction();
        }

        public static void IfNot(this bool condition, Action falseAction)
        {
            if (!condition) falseAction();
        }

        public static void IfElse(this bool condition, Action trueAction, Action falseAction)
        {
            if (condition) trueAction();
            else falseAction();
        }

        public static void If<T>(this bool condition, Action<T> trueAction, T args)
        {
            if (condition) trueAction(args);
        }

        public static void IfNot<T>(this bool condition, Action<T> falseAction, T args)
        {
            if (!condition) falseAction(args);
        }

        public static void IfElse<T>(this bool condition, Action<T> trueAction, Action<T> falseAction, T args)
        {
            if (condition) trueAction(args);
            else falseAction(args);
        }
        
        public static T IfElse<T>(this bool condition, System.Func<T> trueFunction, System.Func<T> falseFunction)
        {
            return condition ? trueFunction() : falseFunction();
        }

        public static TOut IfElse<TIn, TOut>(this bool condition, System.Func<TIn, TOut> trueFunction, System.Func<TIn, TOut> falseFunction, TIn args)
        {
            return condition ? trueFunction(args) : falseFunction(args);
        }
    }
}