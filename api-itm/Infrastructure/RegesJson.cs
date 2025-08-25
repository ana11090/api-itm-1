using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace api_itm.Infrastructure
{
    /// <summary>
    /// Utilities for: deep string cleanup (Unicode/diacritics/mojibake),
    /// safe serialization, and light normalization for names expected by REGES.
    /// </summary>
    public static class RegesJson
    {
        private const int MaxDepth = 64; // safety for deep graphs

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) // keep real Unicode
        };

        /// <summary>
        /// Deep-fix string values in the object graph, then serialize.
        /// </summary>
        public static string SanitizeAndSerialize(object payload)
        {
            SanitizeObjectDeep(payload);
            return JsonSerializer.Serialize(payload, _jsonOpts);
        }

        /// <summary>
        /// Walks an object graph and applies FixText to every string value.
        /// Does NOT change your object model/structure.
        /// </summary>
        public static void SanitizeObjectDeep(object? root)
        {
            if (root is null) return;

            var stack = new Stack<(object node, int depth)>();
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

            void PushIfNeeded(object? candidate, int depth)
            {
                if (candidate is null) return;

                var ct = candidate.GetType();

                // Skip value types
                if (ct.IsValueType) return;

                // Skip problematic or irrelevant types
                if (ShouldSkipType(ct)) return;

                if (visited.Add(candidate))
                    stack.Push((candidate, depth));
            }

            PushIfNeeded(root, 0);

            while (stack.Count > 0)
            {
                var (node, depth) = stack.Pop();
                if (depth > MaxDepth) continue;

                var t = node.GetType();

                // strings handled by parent
                if (t == typeof(string)) continue;

                // IDictionary
                if (node is IDictionary dict)
                {
                    var keyUpdates = new List<(object oldKey, object newKey)>();
                    var valUpdates = new List<(object key, object newVal)>();

                    foreach (DictionaryEntry kv in dict)
                    {
                        var k = kv.Key;
                        var v = kv.Value;

                        if (k is string ks)
                        {
                            var nk = FixText(ks);
                            if (!Equals(ks, nk)) keyUpdates.Add((k!, nk));
                        }

                        if (v is string vs)
                        {
                            var nv = FixText(vs);
                            if (!Equals(vs, nv)) valUpdates.Add((k!, nv));
                        }
                        else
                        {
                            PushIfNeeded(v, depth + 1);
                        }
                    }

                    // apply pending updates (avoid duplicate keys)
                    foreach (var (oldKey, newKey) in keyUpdates)
                    {
                        if (!Equals(oldKey, newKey) && !dict.Contains(newKey))
                        {
                            var val = dict[oldKey];
                            dict.Remove(oldKey);
                            dict[newKey] = val!;
                        }
                    }
                    foreach (var (key, newVal) in valUpdates)
                        dict[key] = newVal;

                    continue;
                }

                // IEnumerable (but not string)
                if (node is IEnumerable en)
                {
                    foreach (var item in en)
                        PushIfNeeded(item, depth + 1);
                    continue;
                }

                // POCOs: public instance props (no indexers) + fields
                foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!p.CanRead || p.GetIndexParameters().Length != 0) continue;

                    object? val;
                    try { val = p.GetValue(node); }
                    catch { continue; }

                    if (val is null) continue;

                    if (p.PropertyType == typeof(string) && p.CanWrite)
                    {
                        var nv = FixText((string)val);
                        if (!Equals(val, nv))
                            TrySetProperty(p, node, nv);
                    }
                    else
                    {
                        PushIfNeeded(val, depth + 1);
                    }
                }

                foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    object? val;
                    try { val = f.GetValue(node); }
                    catch { continue; }

                    if (val is null) continue;

                    if (f.FieldType == typeof(string))
                    {
                        var nv = FixText((string)val);
                        if (!Equals(val, nv))
                            TrySetField(f, node, nv);
                    }
                    else
                    {
                        PushIfNeeded(val, depth + 1);
                    }
                }
            }
        }

        private static bool ShouldSkipType(Type t)
        {
            // Skip delegates, reflection, tasks, streams, UI types, etc.
            if (typeof(Delegate).IsAssignableFrom(t)) return true;
            if (typeof(MemberInfo).IsAssignableFrom(t)) return true;
            if (typeof(Type).IsAssignableFrom(t)) return true;
            if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(t)) return true;
            if (typeof(System.IO.Stream).IsAssignableFrom(t)) return true;

            // Avoid diving into framework types (we still allow collections)
            var ns = t.Namespace ?? "";
            if (ns.StartsWith("System.", StringComparison.Ordinal) || ns == "System")
            {
                if (typeof(IEnumerable).IsAssignableFrom(t) || typeof(IDictionary).IsAssignableFrom(t))
                    return false;
                return true;
            }

            return false;
        }

        private static void TrySetProperty(PropertyInfo p, object target, string value)
        {
            try { p.SetValue(target, value); } catch { /* ignore */ }
        }

        private static void TrySetField(FieldInfo f, object target, string value)
        {
            try { f.SetValue(target, value); } catch { /* ignore */ }
        }

        /// <summary>
        /// Fix mojibake, normalize Unicode, standardize Romanian comma-below diacritics, collapse whitespace.
        /// </summary>
        public static string FixText(string? s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;

            // Decode JSON-style escapes (e.g., Rom\u00E2nia)
            string unescaped = s;
            for (int i = 0; i < 2; i++)
            {
                string tmp;
                try { tmp = Regex.Unescape(unescaped); }
                catch { break; }
                if (tmp == unescaped) break;
                unescaped = tmp;
            }

            string repaired = TryRepairUtf8Mojibake(unescaped);

            repaired = repaired.Normalize(NormalizationForm.FormC);

            // cedilla -> comma below
            repaired = repaired
                .Replace("ş", "ș").Replace("Ş", "Ș")
                .Replace("ţ", "ț").Replace("Ţ", "Ț");

            return CollapseWs(repaired);
        }

        /// <summary>
        /// Convenience: FixText + normalize to official country label (when needed).
        /// Example: "Română" -> "România".
        /// </summary>
        public static string Norma(string? s)
            => RegesName.NormalizeNationality(FixText(s));

        private static string TryRepairUtf8Mojibake(string s)
        {
            if (!(s.Contains('Ã') || s.Contains('Å') || s.Contains('Â') || s.Contains('�'))) return s;
            try
            {
                var bytes = Encoding.GetEncoding(1252).GetBytes(s);
                var repaired = Encoding.UTF8.GetString(bytes);
                return repaired.Contains('�') ? s : repaired;
            }
            catch { return s; }
        }

        private static string CollapseWs(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool ws = false;
            foreach (var c in s.Trim())
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!ws) { sb.Append(' '); ws = true; }
                }
                else
                {
                    sb.Append(c);
                    ws = false;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Minimal, practical mapping from adjectives/aliases to the exact country labels
        /// expected by the REGES API. Extend as you meet rejections.
        /// </summary>
        private static class RegesName
        {
            public static string NormalizeNationality(string? raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;

                var v = FixText(raw).Trim();

                // Mapări -> nume de țară, nu adjectiv:
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // România
                    ["romana"] = "România",
                    ["română"] = "România",
                    ["romanian"] = "România",
                    ["romania"] = "România",

                    // Cazuri frecvente respinse de API
                    ["republica moldova"] = "Moldova (Republica)",
                    ["moldova"] = "Moldova (Republica)",
                    ["uk"] = "Marea Britanie",
                    ["regatul unit"] = "Marea Britanie",
                    ["cote d'ivoire"] = "Coasta de Fildeş",
                    ["côte d'ivoire"] = "Coasta de Fildeş",
                    ["costa de fildes"] = "Coasta de Fildeş",
                    ["statele unite"] = "Statele Unite ale Americii",
                    ["sua"] = "Statele Unite ale Americii",
                    ["coreea de sud"] = "Coreea de Sud",
                    ["coreea de nord"] = "Coreea de Nord",
                };

                var key = RemoveDiacritics(v).ToLowerInvariant();
                if (map.TryGetValue(key, out var fixedName))
                    return fixedName;

                // If already a correct country label, return as-is.
                return v;
            }

            private static string RemoveDiacritics(string text)
            {
                var norm = text.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(norm.Length);
                foreach (var ch in norm)
                {
                    var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (cat != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(ch);
                }
                return sb.ToString().Normalize(NormalizationForm.FormC);
            }
        }

        /// <summary>
        /// Reference equality comparer for visited set in graph traversal.
        /// </summary>
        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
