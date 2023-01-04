using Godot;
using System.Collections.Generic;


namespace GodotHelpers
{
    static class SimpleTimer
    {
        private static Dictionary<string, ulong> starts = new Dictionary<string, ulong>();

        public static void Start(string key)
        {            
            starts[key] = Time.GetTicksMsec();
        }

        public static void Stop(string key)
        {
            var duration = Time.GetTicksMsec() - starts[key];
            starts.Remove(key);
            GD.Print($"Timing {key} @ {duration}ms");            
        }
    }
}
