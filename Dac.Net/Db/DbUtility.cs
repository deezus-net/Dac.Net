using System.Collections.Generic;
using System.Linq;

namespace Dac.Net.Db
{
    public static class DbUtility
    {
        public static DbDiff Diff(this Dictionary<string, DbTable> org, Dictionary<string, DbTable> target)
        {
            var result = new DbDiff
            {

                CurrentTables = org,
                NewTables = target
            };

            // tables

            foreach (var tableName in org.Keys.Concat(target.Keys).Distinct())
            {
                if (!target.ContainsKey(tableName))
                {
                    result.DeletedTableNames.Add(tableName);

                }
                else if (!org.ContainsKey(tableName))
                {
                    result.AddedTables[tableName] = target[tableName];

                }
                else
                {
                    // columns

                    foreach (var columnName in org[tableName].Columns.Keys.Concat(target[tableName].Columns.Keys)
                        .Distinct())
                    {
                        if (!target[tableName].Columns.ContainsKey(columnName))
                        {
                            InitModifiedTable(result, tableName);
                            result.ModifiedTables[tableName].DeletedColumnName.Add(columnName);

                        }
                        else if (!org[tableName].Columns.ContainsKey(columnName))
                        {
                            InitModifiedTable(result, tableName);
                            result.ModifiedTables[tableName].AddedColumns.Add(columnName, target[tableName].Columns[columnName]);

                        }
                        else if (!org[tableName].Columns[columnName].Equal(target[tableName].Columns[columnName]))
                        {
                            InitModifiedTable(result, tableName);
                            result.ModifiedTables[tableName].ModifiedColumns.Add(columnName, new[]
                            {
                                org[tableName].Columns[columnName],
                                target[tableName].Columns[columnName]
                            });
                        }
                    }

                    // indexes
                    foreach (var indexName in org[tableName].Indices.Keys.Concat(target[tableName].Indices.Keys).Distinct()) {
                        
                        
                        
                        if (!target[tableName].Indices.ContainsKey(indexName)) {
                            InitModifiedTable(result, tableName);
                            result.ModifiedTables[tableName].DeletedIndexNames.Add(indexName);

                        } else if (!org[tableName].Indices.ContainsKey(indexName)) {
                            InitModifiedTable(result, tableName);
                            result.ModifiedTables[tableName].AddedIndices.Add(indexName, target[tableName].Indices[indexName]);

                        } else if (!target[tableName].Indices[indexName].Equal(org[tableName].Indices[indexName])) {
                            InitModifiedTable(result, tableName);
                            result.ModifiedTables[tableName].ModifiedIndices.Add(indexName, new[]
                                {
                                    org[tableName].Indices[indexName],
                                    target[tableName].Indices[indexName]
                                }
                            );

                        }

                    }
                }

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="org"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Equal(this DbColumn org, DbColumn target)
        {
            // foreign key check
            var fkDiff = false;
            foreach (var fkName in org.Fk.Keys.Concat(target.Fk.Keys).Distinct())
            {
                if (!org.Fk.Keys.Contains(fkName) || !target.Fk.Keys.Contains(fkName))
                {
                    fkDiff = true;
                    break;
                }

                if ((org.Fk[fkName].Update == target.Fk[fkName].Update) &&
                    (org.Fk[fkName].Delete == target.Fk[fkName].Delete) &&
                    (org.Fk[fkName].Table == target.Fk[fkName].Table) &&
                    (org.Fk[fkName].Column == target.Fk[fkName].Column))
                {
                    continue;
                }
                fkDiff = true;
                break;
            }

            return org.Type == target.Type &&
                   org.Length == target.Length &&
                   org.NotNull == target.NotNull &&
                   org.Id == target.Id &&
                   org.Default == target.Default &&
                   !fkDiff;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="org"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Equal(this DbIndex org, DbIndex target)
        {
            var col1 = string.Join("¥t", org.Columns.Select(x => $"{x.Key},{x.Value}"));
            var col2 = string.Join("¥t", target.Columns.Select(x => $"{x.Key},{x.Value}"));
            return org.Unique == target.Unique && org.Type == target.Type && col1 == col2;
        }


        private static void InitModifiedTable(DbDiff result, string tableName)
        {
            if (!result.ModifiedTables.ContainsKey(tableName))
            {
                result.ModifiedTables.Add(tableName, new ModifiedTable());
            }
        }
    }
}