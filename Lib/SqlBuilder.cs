using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace The.DotNet.Lib
{
    public class JoinSpec
    {
        public string Alias { get; set; }
        public string Table { get; set; }
        public string LocalKey { get; set; }
        public string ForeignKey { get; set; }
        public List<string> Cols { get; set; }
        public string Prefix { get; set; }
    }

    public class SqlBuilder
    {
        public static List<string> BuildSelectList(string alias, List<string> cols = null, string prefix = null)
        {
            if (cols == null || cols.Count == 0)
            {
                return new List<string> { $"{alias}.*" };
            }
            return cols.Select(col =>
            {
                var @as = !string.IsNullOrEmpty(prefix) ? $"{prefix}__{col}" : col;
                return $"{alias}.`{col}` AS {@as}";
            }).ToList();
        }

        public static (string Sql, List<object> Placeholders, List<string> SelectParts) BuildJoinQuery(
            string baseTable,
            string baseAlias,
            List<string> baseCols,
            List<JoinSpec> joins,
            Dictionary<string, object> where = null)
        {
            var selectParts = BuildSelectList(baseAlias, baseCols);
            var joinSql = new StringBuilder();

            foreach (var join in joins)
            {
                var relSelect = BuildSelectList(
                    join.Alias,
                    join.Cols ?? new List<string>(),
                    join.Prefix
                );
                selectParts.AddRange(relSelect);
                joinSql.Append($" LEFT JOIN {join.Table} {join.Alias} ON {baseAlias}.`{join.LocalKey}` = {join.Alias}.`{join.ForeignKey}`");
            }

            var whereClauses = new List<string>();
            var placeholders = new List<object>();

            if (where != null)
            {
                foreach (var kvp in where)
                {
                    if (kvp.Value is System.Collections.IEnumerable list && !(kvp.Value is string))
                    {
                        var vals = new List<object>();
                        foreach (var v in list) vals.Add(v);
                        
                        if (vals.Count > 0)
                        {
                            var qs = string.Join(",", vals.Select(_ => "?"));
                            whereClauses.Add($"{baseAlias}.`{kvp.Key}` IN ({qs})");
                            placeholders.AddRange(vals);
                        }
                    }
                    else
                    {
                        whereClauses.Add($"{baseAlias}.`{kvp.Key}` = ?");
                        placeholders.Add(kvp.Value);
                    }
                }
            }

            var whereSql = whereClauses.Count > 0 ? " WHERE " + string.Join(" AND ", whereClauses) : "";

            var sql = $"SELECT {string.Join(", ", selectParts)} FROM {baseTable} {baseAlias}{joinSql}{whereSql}";

            return (sql, placeholders, selectParts);
        }
    }
}
