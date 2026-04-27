using System.Reflection;
using System.Runtime.Loader;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection
{
    public class DllFileLoader
    {
        public Assembly Load(string path)
        {
            var assemblyLoadContext = new AssemblyLoadContext("TemporaryAssemblyLoadContext");
            var assemblyBytes = File.ReadAllBytes(path);
            return assemblyLoadContext.LoadFromStream(new MemoryStream(assemblyBytes));
        }

        public Assembly LoadFromStream(Stream stream)
        {

            var assemblyLoadContext = new AssemblyLoadContext("TemporaryAssemblyLoadContext");
            return assemblyLoadContext.LoadFromStream(stream);
        }
    }
}
