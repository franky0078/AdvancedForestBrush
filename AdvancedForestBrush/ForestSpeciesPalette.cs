using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace AdvancedForestBrush
{
    public static class ForestSpeciesPalette
    {
        public static bool Active;
        public static readonly List<Entity> Prefabs = new List<Entity>();
        public static readonly List<string> Names = new List<string>();
        private static readonly List<int> Weights = new List<int>();
        private static int TotalWeight;

        public static string WeightsJson => "[" + string.Join(",", Weights) + "]";

        public static void SetWeight(int index, int value)
        {
            if (index < 0 || index >= Weights.Count) return;
            value = Math.Max(0, Math.Min(100, value));
            TotalWeight += value - Weights[index];
            Weights[index] = value;
        }

        public static Entity Select(uint hash)
        {
            // Zero-weight entries remain in the list but are never placed.
            if (TotalWeight == 0) return Entity.Null;
            int choice = (int)(hash % (uint)TotalWeight);
            for (int i = 0; i < Weights.Count; i++)
            {
                if (choice < Weights[i]) return Prefabs[i];
                choice -= Weights[i];
            }
            return Entity.Null;
        }

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
            Weights.Add(100);
            TotalWeight += 100;
        }

        public static void Remove(int index)
        {
            if (index < 0 || index >= Prefabs.Count) return;
            TotalWeight -= Weights[index];
            Weights.RemoveAt(index);
            Prefabs.RemoveAt(index);
            Names.RemoveAt(index);
        }

        public static void Clear()
        {
            Prefabs.Clear();
            Names.Clear();
            Weights.Clear();
            TotalWeight = 0;
        }
    }
}
