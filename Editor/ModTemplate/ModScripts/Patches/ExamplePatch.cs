namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string ExamplePatch = @"namespace [[ModName]].Patches
{
    public static class ExamplePatch
    {
        // Below is an example of how to patch a method.

        // Signaling:
        // You have 2 options here

        // Option 1:
        //[HarmonyPatch]
        //public static class PlayPatch
        //{
        //    internal static MethodBase TargetMethod()
        //    {
        //        return AccessTools.Method(typeof(scnEditor), nameof(scnEditor.Play) or ""Play"" if method is private);
        //    }
        //}

        // Use this for precise method targetting

        // Option 2:
        //[HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Play) or ""Play"" if method is private)]
        //public static class PlayPatch
        //{
        //}

        // This option allows you to ignore ""TargetMethod"" though isn't very flexible when it comes to accurate targetting.

        // Now, within the same class, you have a few options to patch the method that I know:

        // Prefix: If you define a Prefix method within the patch, then it will execute the code in the method BEFORE the targetted method runs. Prefix can return void or bool. Return bool is useful since if you return false, it will skip the targetted method from running completely, and if you return true, it let the method run.

        // Postfix: Opposite of Prefex, a void method that runs after the targetted method finish running.

        // Transpiler: Allows you to completely rewrite the IL instructions of the targetted method completely. Really hard to pull off since you'd need to learn IL (Intermediate Language).

        // Example:
        //[HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Play))]
        //public static class PlayPatch
        //{
        //    static void Prefix()
        //    {
        //        Main.Logger.Log(""Hello! Running before Play is called"");
        //    }

        //    static void Postfix()
        //    {
        //        Main.Logger.Log(""Hello! Running after Play is called"");
        //    }
        //}

        // Parameters:
        // In the Prefix and Postfix method, you have a few options to put in as parameter. If the original method has some parameters itself, and you need to be able to read them. Then just write a parameter to Prefix/Postfix with the same type and name. Once you've done this you can read the passed parameter values once the method is called. Additionally, there are some other useful parameter:
        // {Class type of method} __instance -> Add this parameter to get the instance that the method is being called in, useful for non static classes;
        // {Property Type} value -> Useful for patching Setters, gives you the passed value;
        // ref {Property Type} __result -> Useful for patching Getters, allowing you to manipulate the output of a getter
    }
}";
	}
}
