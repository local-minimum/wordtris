using Godot;
using System;


namespace GodotHelpers
{
    static class LoadTextResource
    {
        public static File Load(string resourcePath)
        {
            if (resourcePath == null) throw new ArgumentNullException("Resource path cannot be null");

            if (!resourcePath.StartsWith("res://"))
            {
                resourcePath = $"res://{resourcePath}";
            }

            if (OS.IsDebugBuild())
            {
                GD.Print($"Loading \"{resourcePath}\", remember to include this is Export > Resources > Export non-resource files");
            }

            var f = new File();
            f.Open(resourcePath, File.ModeFlags.Read);
            return f;
        }
    }
}
