using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace ADO2SQL
{
    class SQLiteExport
    {
        #region Static Methods
        public static string GetCreateSQLDb(DataSet ds)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("CREATE DATABASE ");
            //sb.Append(ds.DataSetName);
            //sb.AppendLine(" ENCODING = 'UTF8' TABLESPACE = pg_default LC_COLLATE = 'en_US.UTF-8' LC_CTYPE = 'en_US.UTF-8' CONNECTION LIMIT = -1;");
            //sb.AppendLine("");

            foreach (DataTable t in ds.Tables)
            {
                GetCreateSQL(t, sb);
            }

            foreach (DataTable t in ds.Tables)
            {
                GetInsertSQL(t, sb);
            }
            
            return sb.ToString();
        }

        private static void GetInsertSQL(DataTable t, StringBuilder stringBuilder)
        {
            foreach (DataRow r in t.Rows)
            {
                stringBuilder.Append("INSERT INTO ");
                stringBuilder.Append(t.TableName);
                stringBuilder.Append(" (");
                foreach (DataColumn c in t.Columns)
                {
                    stringBuilder.Append(c.ColumnName);
                    stringBuilder.Append(", ");
                }

                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.Append(") VALUES (");

                foreach (DataColumn c in t.Columns)
                {
                    stringBuilder.Append("\"");
                    switch (c.DataType.ToString())
                    {
                        case "System.Boolean":
                            if ((bool)r[c.ColumnName] == true)
                            {
                                stringBuilder.Append("1");
                            }
                            else
                            {
                                stringBuilder.Append("0");
                            }
                            break;
                        case "System.DateTime":
                            if (r[c.ColumnName] != DBNull.Value)
                                stringBuilder.Append(((DateTime)r[c.ColumnName]).ToString("yyyy-MM-dd hh:mm:ss"));
                            break;
                        case "System.Double":
                            if (r[c.ColumnName] != DBNull.Value)
                                stringBuilder.Append(((double)r[c.ColumnName]).ToString("G", CultureInfo.InvariantCulture));
                            break;
                        default:
                            stringBuilder.Append(r[c.ColumnName].ToString());
                            break;
                    }
                    stringBuilder.Append("\"");
                    stringBuilder.Append(", ");
                }
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.AppendLine(");");
                stringBuilder.AppendLine();
            }
        }

        public static void GetCreateSQL(DataTable schema, StringBuilder stringBuilder)
        {
            stringBuilder.Append("CREATE TABLE ");
            stringBuilder.Append(schema.TableName);
            stringBuilder.AppendLine(" (");

            // columns

            DataColumn pk = null;
            if (schema.PrimaryKey.Length == 1)
            {
                pk = schema.PrimaryKey[0];
            }

            foreach (DataColumn column in schema.Columns)
            {
                stringBuilder.Append(column.ColumnName);
                stringBuilder.Append(" ");
                stringBuilder.Append(SQLGetType(column));
                if (pk != null && column == pk)
                {
                    stringBuilder.Append(" PRIMARY KEY ");
                    stringBuilder.Append(SQLGetAutoIncrement(column));
                    stringBuilder.Append(" ");
                }
                stringBuilder.Append(SQLGetAllowNull(column));
                stringBuilder.AppendLine(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 4, 4);

            // primary keys
            if (pk == null)
            {
                stringBuilder.Append(", ");
                stringBuilder.AppendLine();
                stringBuilder.Append("CONSTRAINT PK_");
                stringBuilder.Append(schema.TableName);
                stringBuilder.Append(" PRIMARY KEY (");
                bool hasKeys = (schema.PrimaryKey != null && schema.PrimaryKey.Length > 0);
                if (hasKeys)
                {
                    // user defined keys
                    foreach (DataColumn key in schema.PrimaryKey)
                    {
                        stringBuilder.Append(key.ColumnName);
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.Append(")");
            }
            stringBuilder.AppendLine(");");
            stringBuilder.AppendLine();
        }

        private static string SQLGetAllowNull(DataColumn column)
        {
            if (column.DataType.ToString() != "System.Boolean")
            {
                if (column.AllowDBNull == false)
                    return " NOT NULL";
                else return "";
            }
            else return "";
        }

        private static string SQLGetAutoIncrement(DataColumn column)
        {
            if (column.AutoIncrement == true)
                return " AUTOINCREMENT";
            else return "";
        }

        public static string GetCreateFromDataTableSQL(string tableName, DataTable table)
        {
            string sql = "CREATE TABLE [" + tableName + "] (\n";
            
            // columns
            foreach (DataColumn column in table.Columns)
            {
                sql += "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";

            // primary keys
            if (table.PrimaryKey.Length > 0)
            {
                sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql += "[" + column.ColumnName + "],";
                }
                sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
            }
            return sql;
        }



        public static string[] GetPrimaryKeys(DataTable schema)
        {

            List<string> keys = new List<string>();
            foreach (DataRow column in schema.Rows)
            {
                if (schema.Columns.Contains("IsKey") && (bool)column["IsKey"])
                    keys.Add(column["ColumnName"].ToString());
            }

            return keys.ToArray();
        }



        // Return T-SQL data type definition, based on schema definition for a column
        // Based off of http://msdn.microsoft.com/en-us/library/ms131092.aspx
        public static string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.Byte[]":
                    return "VARBINARY(MAX)";

//                case "System.Boolean":
//                    return "BIT";
                case "System.Boolean":
                    return "INTEGER(1)";

                case "System.DateTime":
                    return "DATETIME";

                case "System.DateTimeOffset":
                    return "DATETIMEOFFSET";

                case "System.Decimal":
                    if (numericPrecision != -1 && numericScale != -1)
                        return "DECIMAL(" + numericPrecision + "," + numericScale + ")";
                    else
                        return "DECIMAL";

                case "System.Double":
                    return "FLOAT";

                case "System.Single":
                    return "REAL";

                case "System.Int64":
                    return "BIGINT";

                case "System.Int32":
                    return "INTEGER";

                case "System.Int16":
                    return "SMALLINT";

//                case "System.String":
//                    return "NVARCHAR(" + ((columnSize == -1 || columnSize > 8000) ? "MAX" : columnSize.ToString()) + ")";
                case "System.String":
                    return "TEXT";

                case "System.Byte":
                    return "TINYINT";

                case "System.Guid":
                    return "UNIQUEIDENTIFIER";

                default:
                    throw new Exception(type.ToString() + " not implemented.");
            }
        }

        // Overload based on row from schema table
        public static string SQLGetType(DataRow schemaRow)
        {
            int numericPrecision;
            int numericScale;

            if (!int.TryParse(schemaRow["NumericPrecision"].ToString(), out numericPrecision))
            {
                numericPrecision = -1;
            }
            if (!int.TryParse(schemaRow["NumericScale"].ToString(), out numericScale))
            {
                numericScale = -1;
            }

            return SQLGetType(schemaRow["DataType"],
                                int.Parse(schemaRow["ColumnSize"].ToString()),
                                numericPrecision,
                                numericScale);
        }
        // Overload based on DataColumn from DataTable type
        public static string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, -1, -1);
        }
    }
        #endregion
}

