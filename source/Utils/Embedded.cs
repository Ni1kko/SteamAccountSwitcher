using System;
using System.Reflection;

namespace SteamAccountSwitcher.Utils
{
    public sealed class Embedded
    {
        public Assembly assembly = typeof(Embedded).Assembly;

        public Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            var askedAssembly = new AssemblyName(args.Name);

            lock (this)
            {
                var stream = Misc.AssemblyFunctions.assembly.GetManifestResourceStream($"SteamAccountSwitcher.Embedded.{askedAssembly.Name}.dll");
                if (stream == null) return null;

                Assembly assembly = null;

                try
                {
                    var assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    assembly = Assembly.Load(assemblyData);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Loading embedded assembly: {1}{0}Has thrown a unhandled exception: {2}", Environment.NewLine, askedAssembly.Name, e);
                }
                finally
                {
                    if (assembly != null) Console.WriteLine("Loaded embedded assembly: {0}", askedAssembly.Name);
                }

                return assembly;
            }
        }
    }
}
