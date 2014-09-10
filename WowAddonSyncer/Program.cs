using System;
using System.Reflection;

namespace WowAddonSyncer
{
    // Purpose of this file is to embed our Metro Dll library so we only end up with the single file executable at build, 
    // which has all required resources embedded.
    
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var resourceName = Assembly.GetExecutingAssembly().GetName().Name + ".DllsAsResource." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            };
            App.Main();
        }
    }
}