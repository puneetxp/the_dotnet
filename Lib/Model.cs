using System;
using System.Collections.Generic;
using System.Linq;

namespace The.DotNet.Lib
{
    // Define database access interface
    public interface IDB
    {
        void RawSql(string sql);
        void SetPlaceholders(List<object> placeholders);
        List<Dictionary<string, object>> Many();
        Dictionary<string, object> First();
    }

    public abstract class Model
    {
        protected string Table = "";
        protected string Name = "";
        protected IDB Db;
        protected Dictionary<string, dynamic> Relations = new Dictionary<string, dynamic>();
        protected List<string> One = new List<string>();
        protected dynamic? _with;
        protected bool Singular = false;

        public Dictionary<string, object>? Row { get; protected set; }
        public dynamic Items { get; protected set; } = new List<Dictionary<string, object>>();
        public Dictionary<string, object> Page = new Dictionary<string, object>();

        // Indexer and Dictionary-like helpers for backward compatibility (e.g. Auth.cs)
        public object? this[string key]
        {
            get => Row != null && Row.ContainsKey(key) ? Row[key] : null;
            set { if (Row != null) Row[key] = value!; }
        }

        public bool ContainsKey(string key) => Row != null && Row.ContainsKey(key);

        public Model(IDB db)
        {
            this.Db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Model All()
        {
            var sql = $"SELECT * FROM {this.Table}";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(new List<object>());
            this.Items = this.Db.Many();
            this.Singular = false;
            return this;
        }

        public Model? Find(object value, string key = "id")
        {
            var sql = $"SELECT * FROM {this.Table} WHERE `{key}` = ? LIMIT 1";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(new List<object> { value });
            var result = this.Db.First();
            if (result != null && result.Count > 0)
            {
                this.Row = result;
                this.Items = new List<Dictionary<string, object>> { result };
                this.Singular = true;
                return this;
            }
            
            this.Row = null;
            this.Items = new List<Dictionary<string, object>>();
            this.Singular = false;
            return null;
        }

        public Model Where(Dictionary<string, object> conditions)
        {
            var clauses = new List<string>();
            var placeholders = new List<object>();
            foreach (var kvp in conditions)
            {
                if (kvp.Value is System.Collections.IEnumerable list && !(kvp.Value is string))
                {
                    var vals = new List<object>();
                    foreach (var v in list) vals.Add(v);
                    if (vals.Count > 0)
                    {
                        var qs = string.Join(",", vals.Select(_ => "?"));
                        clauses.Add($"`{kvp.Key}` IN ({qs})");
                        placeholders.AddRange(vals);
                    }
                }
                else
                {
                    clauses.Add($"`{kvp.Key}` = ?");
                    placeholders.Add(kvp.Value);
                }
            }
            var whereSql = clauses.Count > 0 ? " WHERE " + string.Join(" AND ", clauses) : "";
            var sql = $"SELECT * FROM {this.Table}{whereSql}";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(placeholders);
            this.Items = this.Db.Many();
            this.Singular = false;
            return this;
        }

        public Model Wherec(string customSql, List<object> placeholders)
        {
            var sql = $"SELECT * FROM {this.Table} WHERE {customSql}";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(placeholders);
            this.Items = this.Db.Many();
            this.Singular = false;
            return this;
        }

        public int Count()
        {
            var sql = $"SELECT COUNT(*) as count FROM {this.Table}";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(new List<object>());
            var result = this.Db.First();
            if (result != null && result.ContainsKey("count"))
            {
                return Convert.ToInt32(result["count"]);
            }
            return 0;
        }

        public Model? Paginate(int pageNumber = 1, int pageItems = 25)
        {
            var total = Count();
            Page["result"] = total;
            
            if (total > 0)
            {
                Page["pageNumber"] = pageNumber;
                Page["pageItems"] = pageItems;
                Page["totalpages"] = (int)Math.Ceiling((double)total / pageItems);
                
                int offset = (pageNumber - 1) * pageItems;
                if (offset > total)
                {
                    offset = 0;
                }
                
                var sql = $"SELECT * FROM {this.Table} LIMIT {pageItems} OFFSET {offset}";
                this.Db.RawSql(sql);
                this.Db.SetPlaceholders(new List<object>());
                this.Items = this.Db.Many();
                this.Singular = false;
                return this;
            }
            
            return null;
        }

        public Dictionary<string, object> Create(Dictionary<string, object> data)
        {
            var columns = new List<string>();
            var values = new List<string>();
            var placeholders = new List<object>();
            foreach (var kvp in data)
            {
                columns.Add($"`{kvp.Key}`");
                values.Add("?");
                placeholders.Add(kvp.Value);
            }
            var sql = $"INSERT INTO {this.Table} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(placeholders);
            this.Db.Many(); // Execute Insert
            return data;
        }

        public int Update(Dictionary<string, object> data, Dictionary<string, object> where)
        {
            var setClauses = new List<string>();
            var placeholders = new List<object>();
            foreach (var kvp in data)
            {
                setClauses.Add($"`{kvp.Key}` = ?");
                placeholders.Add(kvp.Value);
            }
            var whereClauses = new List<string>();
            foreach (var kvp in where)
            {
                whereClauses.Add($"`{kvp.Key}` = ?");
                placeholders.Add(kvp.Value);
            }
            var whereSql = whereClauses.Count > 0 ? " WHERE " + string.Join(" AND ", whereClauses) : "";
            var sql = $"UPDATE {this.Table} SET {string.Join(", ", setClauses)}{whereSql}";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(placeholders);
            var result = this.Db.Many();
            return result.Count;
        }

        public int Delete(Dictionary<string, object> where)
        {
            var clauses = new List<string>();
            var placeholders = new List<object>();
            foreach (var kvp in where)
            {
                clauses.Add($"`{kvp.Key}` = ?");
                placeholders.Add(kvp.Value);
            }
            var whereSql = clauses.Count > 0 ? " WHERE " + string.Join(" AND ", clauses) : "";
            var sql = $"DELETE FROM {this.Table}{whereSql}";
            this.Db.RawSql(sql);
            this.Db.SetPlaceholders(placeholders);
            var result = this.Db.Many();
            return result.Count;
        }

        public Model Join(Dictionary<string, dynamic> joins, Dictionary<string, object> where = null!)
        {
            var joinSpecs = new List<JoinSpec>();

            foreach (var kvp in joins)
            {
                var key = kvp.Key;
                var val = kvp.Value;
                
                string relationName = (int.TryParse(key, out _)) ? (string)val : key;
                List<string> cols = (val is Dictionary<string, object> dict && dict.ContainsKey("cols")) ? (List<string>)dict["cols"] : new List<string>();
                string alias = (val is Dictionary<string, object> dictAlias && dictAlias.ContainsKey("alias")) ? (string)dictAlias["alias"] : relationName;

                if (Relations.ContainsKey(relationName))
                {
                    var r = Relations[relationName];
                    joinSpecs.Add(new JoinSpec
                    {
                        Alias = alias,
                        Table = GetRelationValue(r, "table"),
                        LocalKey = GetRelationValue(r, "name"),
                        ForeignKey = GetRelationValue(r, "key"),
                        Cols = cols,
                        Prefix = relationName
                    });
                }
            }

            var query = SqlBuilder.BuildJoinQuery(this.Table, this.Table, new List<string>(), joinSpecs, where);
            
            this.Db.RawSql(query.Sql);
            this.Db.SetPlaceholders(query.Placeholders);
            this.Items = this.Db.Many();
            this.Singular = false;
            
            return this;
        }

        // --- Eager Relationship Loading (With & Sort) ---

        public Model With(object data, bool first = true)
        {
            var baseItems = Singular && Row != null 
                ? new List<Dictionary<string, object>> { Row } 
                : (Items is List<Dictionary<string, object>> l ? l : new List<Dictionary<string, object>>());

            if (baseItems.Count > 0)
            {
                var x = new Dictionary<string, object>();
                if (first)
                {
                    _with = data;
                }

                if (data is string relationName)
                {
                    var relModel = GetRelationModel(relationName);
                    x[relationName] = relModel?.Items ?? new List<Dictionary<string, object>>();
                }
                else if (data is System.Collections.IEnumerable list && !(data is string))
                {
                    foreach (var item in list)
                    {
                        if (item is string relName)
                        {
                            var relModel = GetRelationModel(relName);
                            x[relName] = relModel?.Items ?? new List<Dictionary<string, object>>();
                        }
                        else if (item is IDictionary<string, object> dict)
                        {
                            foreach (var kvp in dict)
                            {
                                var relModel = GetRelationModel(kvp.Key);
                                if (relModel != null)
                                {
                                    relModel.With(kvp.Value);
                                    x[kvp.Key] = relModel.Items;
                                }
                            }
                        }
                    }
                }
                else if (data is IDictionary<string, object> dict)
                {
                    foreach (var kvp in dict)
                    {
                        var relModel = GetRelationModel(kvp.Key);
                        if (relModel != null)
                        {
                            relModel.With(kvp.Value);
                            x[kvp.Key] = relModel.Items;
                        }
                    }
                }

                x[this.Name] = baseItems;
                this.Items = x;
            }

            return this;
        }

        public Model Sort()
        {
            if (Items is Dictionary<string, object> dict)
            {
                if (_with != null)
                {
                    var baseItems = dict.ContainsKey(this.Name) ? (dict[this.Name] as List<Dictionary<string, object>>) : null;
                    if (baseItems != null)
                    {
                        var sorted = SortOut(_with, baseItems, dict);
                        this.Items = sorted;
                        if (Singular && sorted.Count > 0)
                        {
                            this.Row = sorted[0];
                            this.Items = sorted[0];
                        }
                    }
                }
            }
            return this;
        }

        private List<Dictionary<string, object>> SortOut(object relations, List<Dictionary<string, object>> data, Dictionary<string, object> baseDict)
        {
            if (relations is string relationName)
            {
                return FilterRelation(relationName, data, baseDict);
            }
            else if (relations is System.Collections.IEnumerable list && !(relations is string))
            {
                var currentData = data;
                foreach (var item in list)
                {
                    if (item is string relName)
                    {
                        currentData = FilterRelation(relName, currentData, baseDict);
                    }
                    else if (item is IDictionary<string, object> dict)
                    {
                        foreach (var kvp in dict)
                        {
                            currentData = FilterRelation(kvp.Key, currentData, baseDict);
                        }
                    }
                }
                return currentData;
            }
            return data;
        }

        private List<Dictionary<string, object>> FilterRelation(string relation, List<Dictionary<string, object>> data, Dictionary<string, object> baseDict)
        {
            var relSpec = Relations.ContainsKey(relation) ? Relations[relation] : null;
            if (relSpec == null) return data;

            var localKey = GetRelationValue(relSpec, "name");
            var foreignKey = GetRelationValue(relSpec, "key");

            var childRecords = baseDict.ContainsKey(relation) ? (baseDict[relation] as List<Dictionary<string, object>>) : null;
            if (childRecords == null) childRecords = new List<Dictionary<string, object>>();

            foreach (var item in data)
            {
                var localVal = item.ContainsKey(localKey) ? item[localKey] : null;
                if (localVal == null)
                {
                    item[relation] = One.Contains(relation) ? (object)"" : new List<Dictionary<string, object>>();
                    continue;
                }

                var matchingChildren = childRecords.Where(child => 
                    child.ContainsKey(foreignKey) && child[foreignKey]?.ToString() == localVal.ToString()
                ).ToList();

                if (matchingChildren.Count > 0 && matchingChildren[0].ContainsKey("sort"))
                {
                    matchingChildren = matchingChildren.OrderBy(child => {
                        int.TryParse(child["sort"]?.ToString(), out int sortVal);
                        return sortVal;
                    }).ToList();
                }

                if (One.Contains(relation))
                {
                    item[relation] = matchingChildren.Count > 0 ? (object)matchingChildren[0] : "";
                }
                else
                {
                    item[relation] = matchingChildren;
                }
            }

            return data;
        }

        public Model? GetRelationModel(string relationName)
        {
            if (!Relations.ContainsKey(relationName)) return null;
            var r = Relations[relationName];
            var localKeyName = GetRelationValue(r, "name");
            var foreignKeyName = GetRelationValue(r, "key");
            var callback = GetRelationCallback(r);
            if (callback == null) return null;

            var localValues = new List<object>();
            if (Singular && Row != null)
            {
                if (Row.ContainsKey(localKeyName) && Row[localKeyName] != null)
                {
                    localValues.Add(Row[localKeyName]);
                }
            }
            else if (Items is List<Dictionary<string, object>> list)
            {
                foreach (var item in list)
                {
                    if (item.ContainsKey(localKeyName) && item[localKeyName] != null)
                    {
                        localValues.Add(item[localKeyName]);
                    }
                }
            }

            if (localValues.Count == 0) return null;

            var relModel = callback();
            relModel.Where(new Dictionary<string, object> { { foreignKeyName, localValues } });
            return relModel;
        }

        private string GetRelationValue(dynamic r, string key)
        {
            if (r is IDictionary<string, object> dict)
            {
                return dict[key]?.ToString() ?? "";
            }
            if (r is Dictionary<string, dynamic> dictDyn)
            {
                return dictDyn[key]?.ToString() ?? "";
            }
            var prop = r.GetType().GetProperty(key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null)
            {
                return prop.GetValue(r)?.ToString() ?? "";
            }
            return "";
        }

        private Func<Model>? GetRelationCallback(dynamic r)
        {
            if (r is IDictionary<string, object> dict && dict.ContainsKey("callback"))
            {
                var cb = dict["callback"];
                if (cb is Func<Model> f) return f;
            }
            if (r is Dictionary<string, dynamic> dictDyn && dictDyn.ContainsKey("callback"))
            {
                var cb = dictDyn["callback"];
                if (cb is Func<Model> f) return f;
            }
            var prop = r.GetType().GetProperty("callback", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null)
            {
                var cb = prop.GetValue(r);
                if (cb is Func<Model> f) return f;
            }
            return null;
        }

        public dynamic Array()
        {
            return this.Items;
        }
    }
}
