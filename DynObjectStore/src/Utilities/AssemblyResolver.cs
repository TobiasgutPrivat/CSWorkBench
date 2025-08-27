namespace DynObjectStore;

using System.Reflection;

public class AssemblyResolver
{

    public static void AddAssemblyResolvers()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var requestedName = new AssemblyName(args.Name);
            string assemblyFile = Path.Combine(AppContext.BaseDirectory, requestedName.Name + ".dll");

            if (File.Exists(assemblyFile))
            {
                return Assembly.LoadFrom(assemblyFile);
            }

            return null;
        };
        // 2nd: could try NuGet download here
    }
}