using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace The.DotNet.Lib
{
    // Define dummy interfaces for compilation context
    public interface IDB
    {
        void RawSql(string sql);
        void SetPlaceholders(List<object> placeholders);
        List<Dictionary<string, object>> Many();
        Dictionary<string, object> First();
        // ... other DB methods
    }

    public abstract class Model
    {
        protected string Table;
        protected string Name;
        protected IDB Db;
        protected Dictionary<string, dynamic> Relations = new Dictionary<string, dynamic>();
        protected List<dynamic> Items = new List<dynamic>();

        public Model(IDB db)
        {
            this.Db = db;
        }

        public Model Join(Dictionary<string, dynamic> joins, Dictionary<string, object> where = null)
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
                        Table = r["table"],
                        LocalKey = r["name"],
                        ForeignKey = r["key"],
                        Cols = cols,
                        Prefix = relationName
                    });
                }
            }

            var query = SqlBuilder.BuildJoinQuery(this.Table, this.Table, new List<string>(), joinSpecs, where);
            
            this.Db.RawSql(query.Sql);
            this.Db.SetPlaceholders(query.Placeholders);
            var results = this.Db.Many();
            // Convert results to Items...
            
            return this;
        }
    }
}
