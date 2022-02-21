using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace InstanceDejavu
{
    public static class XrefUtils
    {
        public static bool CheckMethod(MethodInfo method, string match)
        {
            try
            {
                return XrefScanner.XrefScan(method).Any(instance => instance.Type == XrefType.Global && instance.ReadAsObject().ToString().Contains(match));
            }
            catch
            {
                // no catch
            }
            return false;
        }
    }
}