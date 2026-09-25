using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace AdvancedForestBrush
{
    // The game's vegetation bar only keeps its current page's selection.
    // Keep a separate palette so changing pages does not discard earlier species.
    public static class ForestSpeciesPalette
    {
        public static bool Active;
        public static readonly List<Entity> Prefabs = new List<Entity>();
        public static readonly List<string> Names = new List<string>();

        public static string NamesJson
        {
            get
            {
                var json = new StringBuilder("[");
                for (int i = 0; i < Names.Count; i++)
                {
                    if (i > 0) json.Append(',');
                    json.Append('"');
                    foreach (char c in Names[i])
                    {
                        if (c == '"' || c == '\\') json.Append('\\');
                        if (c < 32) json.Append(' ');
                        else json.Append(c);
                    }
                    json.Append('"');
                }
                return json.Append(']').ToString();
            }
        }

        public static void Add(Entity prefab, string name)
        {
            if (prefab == Entity.Null || Prefabs.Contains(prefab)) return;
            Prefabs.Add(prefab);
            Names.Add(name);
        }

        public static void Remove(int index)
        {
            if (index < 0 || index >= Prefabs.Count) return;
            Prefabs.RemoveAt(index);
            Names.RemoveAt(index);
        }

        public static void Clear()
        {
            Prefabs.Clear();
            Names.Clear();
        }
    }
}
