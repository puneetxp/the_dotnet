using System;
using System.Collections.Generic;
using System.Data.Common;

namespace The.DotNet.Lib
{
    public class DB : IDB
    {
        private readonly DbConnection _connection;
        private string _query = "";
        private List<object> _placeholders = new List<object>();

        public DB(DbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void RawSql(string sql)
        {
            _query = sql;
        }

        public void SetPlaceholders(List<object> placeholders)
        {
            _placeholders = placeholders ?? new List<object>();
        }

        public List<Dictionary<string, object>> Many()
        {
            var results = new List<Dictionary<string, object>>();
            
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = _query;

                // Bind parameters: replace '?' with named parameters '@p0', '@p1', etc.
                int paramIndex = 0;
                while (command.CommandText.Contains("?"))
                {
                    int index = command.CommandText.IndexOf("?");
                    command.CommandText = command.CommandText.Remove(index, 1).Insert(index, $"@p{paramIndex}");
                    
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@p{paramIndex}";
                    parameter.Value = (paramIndex < _placeholders.Count ? _placeholders[paramIndex] : null) ?? DBNull.Value;
                    command.Parameters.Add(parameter);

                    paramIndex++;
                }

                // If no '?' are in command text but we have placeholders, bind them directly
                if (paramIndex == 0 && _placeholders.Count > 0)
                {
                    for (int i = 0; i < _placeholders.Count; i++)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = $"@p{i}";
                        parameter.Value = _placeholders[i] ?? DBNull.Value;
                        command.Parameters.Add(parameter);
                    }
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                }
            }

            return results;
        }

        public Dictionary<string, object> First()
        {
            var results = Many();
            return results.Count > 0 ? results[0] : new Dictionary<string, object>();
        }
    }
}
