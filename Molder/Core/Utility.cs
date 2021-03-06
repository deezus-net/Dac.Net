using System;
using System.Collections.Generic;
using System.IO;

 using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Molder.Db;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Molder.Core
{
    public class Utility
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        private static readonly ISerializer Serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Dictionary<string, Server> LoadServers(string file)
        {
            try
            {
                var yml = File.ReadAllText(file);
                return Deserializer.Deserialize<Dictionary<string, Server>>(yml);
            }
            catch (Exception e)
            {

            }

            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DataBase LoadDataBase(string file)
        {
          //  try
          //  {
                var yml = File.ReadAllText(file);
                return Deserializer.Deserialize<DataBase>(yml);
           // }
           // catch (Exception e)
           // {

           // }

            return null;
        }

        public static void TrimDataBaseProperties(DataBase db)
        {
            foreach (var (tableName, table) in db.Tables)
            {

                foreach (var (columnName, column) in table.Columns)
                {
                    if (column.Id ?? false)
                    {
                        column.Type = null;
                        column.NotNull = null;
                        column.Pk = null;
                        column.Length = null;
                    }
                    else
                    {
                        column.Id = null;
                    }

                    if (!(column.Pk ?? false))
                    {
                        column.Pk = null;
                    }
                    else
                    {
                        column.NotNull = true;
                    }

                    if (!(column.NotNull ?? false))
                    {
                        column.NotNull = null;
                    }

                    if (string.IsNullOrWhiteSpace(column.Default))
                    {
                        column.Default = null;
                    }

                    if (column.LengthInt == 0 || string.IsNullOrWhiteSpace(column.Length))
                    {
                        column.Length = null;
                    }


                    if (column.ForeignKeys != null)
                    {
                        foreach (var (fkName, fk) in column.ForeignKeys)
                        {
                            if (string.IsNullOrEmpty(fk.Update) || fk.Update == "NO_ACTION")
                            {
                                fk.Update = null;
                            }

                            if (string.IsNullOrWhiteSpace(fk.Delete) || fk.Delete == "NO_ACTION")
                            {
                                fk.Delete = null;
                            }
                        }

                        if (!column.ForeignKeys.Any())
                        {
                            column.ForeignKeys = null;
                        }
                    }

                }

                if (table.Indexes != null)
                {
                    foreach (var (indexName, index) in table.Indexes)
                    {
                        if (!(index.Unique ?? false))
                        {
                            index.Unique = null;
                        }

                        index.Type = index.Type?.ToLower();

                        var indexColumns = new Dictionary<string, string>();
                        foreach (var (indexColumnName, direction) in index.Columns)
                        {
                            indexColumns.Add(indexColumnName, (direction ?? "").ToLower());
                        }

                        index.Columns = indexColumns;

                    }

                    if (!table.Indexes.Any())
                    {
                        table.Indexes = null;
                    }
                }
            }

            if (!(db.Synonyms?.Any() ?? false))
            {
                db.Synonyms = null;
            }
            
            if (!(db.Views?.Any() ?? false))
            {
                db.Views = null;
            }
        }

        public static string DataBaseToYaml(DataBase db)
        {
            TrimDataBaseProperties(db);
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                Serializer.Serialize(tw, db);
            }

            return sb.ToString();

        }

        public static string TrimQuery(string src)
        {
            var result = Regex.Replace(src, "[\\s]+", " ");
            
            
            
            return result;
        }

        public static string CreateQueryHeader(Server server)
        {
            return $"/* {server.Database} */";
        }
    }
}