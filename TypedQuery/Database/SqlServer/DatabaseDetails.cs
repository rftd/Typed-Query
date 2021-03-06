﻿
/*
 * 
 * Copyright (C) 2009-2016 JFo.nz
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, version 3 of the License.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see http://www.gnu.org/licenses/.
 **/

using System;
using System.Collections.Generic;
using System.Text;

namespace Sql.Database.SqlServer {

	public static class DatabaseDetails {

		public static DbTable GetTable(ADatabase pDatabase, Sql.ATable pTable) {
			IList<DbTable> tables = GetTables(pTable.TableName, pDatabase.GetConnection(false));
			return tables.Count > 0 ? tables[0] : null;
		}
		
		public static IList<DbTable> GetTables(System.Data.Common.DbConnection pConnection) {
			return GetTables(null, pConnection);
		}
		
		private static IList<DbTable> GetTables(string pTableName, System.Data.Common.DbConnection pConnection) {

			Dictionary<string, DbTable> tables = new Dictionary<string, DbTable>();
			IList<DbTable> tableList = new List<DbTable>();	
			
			using(System.Data.Common.DbCommand command = Transaction.CreateCommand(pConnection, null)) {

				string whereClause = !string.IsNullOrEmpty(pTableName) ? "WHERE tables.TABLE_NAME = '" + pTableName + "' " : string.Empty;
				
				command.CommandText = "SELECT tables.TABLE_NAME, tables.TABLE_SCHEMA, columns.COLUMN_NAME, IS_NULLABLE,DATA_TYPE, COLUMN_DEFAULT, tc.CONSTRAINT_TYPE , " +
					"columnproperty(object_id(quotename(columns.TABLE_SCHEMA) + '.' + quotename(columns.TABLE_NAME)), columns.COLUMN_NAME, 'IsIdentity') " +
					"FROM INFORMATION_SCHEMA.TABLES AS tables " +
					"JOIN INFORMATION_SCHEMA.COLUMNS AS columns ON tables.TABLE_CATALOG = columns.TABLE_CATALOG AND " +
					"tables.TABLE_SCHEMA = columns.TABLE_SCHEMA AND tables.TABLE_NAME = columns.TABLE_NAME " +
					"LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu ON cu.Table_Name = columns.Table_Name AND cu.Column_Name = columns.COLUMN_NAME " +
					"LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME AND CONSTRAINT_TYPE = 'PRIMARY KEY' " +
					whereClause +
					"ORDER BY tables.TABLE_SCHEMA, tables.TABLE_NAME, columns.ORDINAL_POSITION";
				
				command.Connection = pConnection;
				
				Dictionary<string, DbColumn> columnLookup = new Dictionary<string, DbColumn>();
				
				using(System.Data.Common.DbDataReader reader = command.ExecuteReader()) {
					
					while (reader.Read()) {
						
						string tableName = reader.GetString(0);
						string schemaName = reader.GetString(1);
						string columnName = reader.GetString(2);
						string isNullable = reader.GetString(3);
						string dataType = reader.GetString(4);
						string columnDefault = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;
						bool isPrimaryKey =  !reader.IsDBNull(6) && !string.IsNullOrEmpty(reader.GetString(6));
						bool isIdentity = reader.GetInt32(7) == 1;

						if (!tables.ContainsKey(tableName)) {
							DbTable table = new DbTable(tableName, schemaName, new List<DbColumn>());
							tables.Add(tableName, table);
							tableList.Add(table);
						}

						IList<DbColumn> columns = tables[tableName].Columns;
						string columnKey = tableName + schemaName + columnName;
						
						if(!columnLookup.ContainsKey(columnKey)) {
							DbColumn column = new DbColumn(columnName, GetDataType(dataType), isNullable == "YES", isPrimaryKey, isIdentity, false);
							columns.Add(column);
							columnLookup.Add(columnKey, column);
						}
						else {
							
							DbColumn column = columnLookup[columnKey];
							
							if(isPrimaryKey)
								column.IsPrimaryKey = isPrimaryKey;
							
							if(isIdentity)
								column.IsAutoGenerated = isIdentity;
						}
					}
				}
			}
			
			LoadForeignKeys(pTableName, ref tableList, pConnection);
			
			using(System.Data.Common.DbCommand command = Transaction.CreateCommand(pConnection, null)) {
				
				command.CommandText = "SELECT VIEWS.TABLE_NAME, VIEWS.TABLE_SCHEMA, VIEW_COLUMN_USAGE.COLUMN_NAME, IS_NULLABLE,DATA_TYPE " +
					"FROM INFORMATION_SCHEMA.VIEWS AS VIEWS " +
					"JOIN INFORMATION_SCHEMA.VIEW_COLUMN_USAGE AS VIEW_COLUMN_USAGE ON VIEWS.TABLE_CATALOG = VIEW_COLUMN_USAGE.VIEW_CATALOG AND VIEWS.TABLE_SCHEMA = VIEW_COLUMN_USAGE.VIEW_SCHEMA AND VIEWS.TABLE_NAME = VIEW_COLUMN_USAGE.VIEW_NAME " +
					"JOIN INFORMATION_SCHEMA.COLUMNS AS COLUMNS ON VIEW_COLUMN_USAGE.TABLE_CATALOG = COLUMNS.TABLE_CATALOG AND VIEW_COLUMN_USAGE.TABLE_SCHEMA = COLUMNS.TABLE_SCHEMA AND VIEW_COLUMN_USAGE.TABLE_NAME = COLUMNS.TABLE_NAME AND VIEW_COLUMN_USAGE.COLUMN_NAME = COLUMNS.COLUMN_NAME " +
					(!string.IsNullOrEmpty(pTableName) ? "WHERE VIEWS.TABLE_NAME = '" + pTableName + "' " : string.Empty);
				
				command.Connection = pConnection;

				using(System.Data.Common.DbDataReader reader = command.ExecuteReader()) {
					
					tables.Clear();
					
					while (reader.Read()) {
						
						string tableName = reader.GetString(0);
						string schemaName = reader.GetString(1);
						string columnName = reader.GetString(2);
						string isNullable = reader.GetString(3);
						string dataType = reader.GetString(4);
						
						if (!tables.ContainsKey(tableName)) {
							DbTable table = new DbTable(tableName, schemaName, new List<DbColumn>());
							tables.Add(tableName, table);
							tableList.Add(table);
						}
						IList<DbColumn> columns = tables[tableName].Columns;
						columns.Add(new DbColumn(columnName, GetDataType(dataType), isNullable == "YES", false, false, true));
					}
				}
			}
			pConnection.Close();
			return tableList;
		}
		
		public static List<StoredProcedure> GetStoredProcedures(System.Data.Common.DbConnection pConnection) {
			
			List<StoredProcedure> spList = new List<StoredProcedure>();
			
			using(System.Data.Common.DbCommand command = Transaction.CreateCommand(pConnection, null)) {
				
				command.CommandText = "SELECT SCHEMA_NAME(schema_id) AS schema_name, o.name AS object_name, p.parameter_id, p.name AS parameter_name" +
										",TYPE_NAME(p.user_type_id) AS parameter_type, p.is_output as is_output " +
										"FROM sys.objects AS o " +
										"INNER JOIN sys.parameters AS p ON o.object_id = p.object_id " +
										"WHERE type_desc = 'SQL_STORED_PROCEDURE' " +
										"ORDER BY schema_name, object_name, p.parameter_id;";
				
				command.Connection = pConnection;

				using(System.Data.Common.DbDataReader reader = command.ExecuteReader()) {

					Dictionary<string, StoredProcedure> lookup = new Dictionary<string, StoredProcedure>();
					
					while (reader.Read()) {
						
						string schema = reader.GetString(0);
						string name = reader.GetString(1);
						
						string key = schema + "*" + name;
						
						StoredProcedure sp;
						
						if(!lookup.ContainsKey(key)) {
							sp = new StoredProcedure(schema, name);
							lookup.Add(key, sp);
							spList.Add(sp);
						}
						else
							sp = lookup[key];
						
						int paramId = reader.GetInt32(2);
						string paramName = reader.GetString(3);
						string paramType = reader.GetString(4);
						bool isOutput = reader.GetBoolean(5);
						
						System.Data.ParameterDirection direction = System.Data.ParameterDirection.Input;
						
						if(isOutput)
							direction = System.Data.ParameterDirection.InputOutput;
						
						sp.AddParameter(new SpParameter(paramId, paramName, GetDataType(paramType), direction));
					}
				}
			}
			return spList;
		}
		
		private static void LoadForeignKeys(string pTableName, ref IList<DbTable> pDbTables, System.Data.Common.DbConnection pConnection){
		
			using(System.Data.Common.DbCommand command = Transaction.CreateCommand(pConnection, null)) {
				
				command.CommandText =
					"SELECT DISTINCT column_constraint.TABLE_SCHEMA, column_constraint.TABLE_NAME, column_constraint.COLUMN_NAME, primary_key_table_constraint.TABLE_NAME " +
					"FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as column_constraint " +
					"JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS as ref_constraint ON column_constraint.TABLE_SCHEMA = ref_constraint.CONSTRAINT_SCHEMA AND column_constraint.CONSTRAINT_NAME = ref_constraint.CONSTRAINT_NAME " +
					"JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as primary_key_table_constraint ON ref_constraint.CONSTRAINT_SCHEMA = primary_key_table_constraint.CONSTRAINT_SCHEMA AND ref_constraint.UNIQUE_CONSTRAINT_NAME = primary_key_table_constraint.CONSTRAINT_NAME " +
					(!string.IsNullOrWhiteSpace(pTableName) ? "WHERE column_constraint.TABLE_NAME = '" + pTableName + "'" : string.Empty);
				
				
				Dictionary<string, string> pkTableLookup = new Dictionary<string, string>();
				
				using(System.Data.Common.DbDataReader reader = command.ExecuteReader()) {
					
					while (reader.Read()) {
						
						string schema = reader.GetString(0);
						string tableName = reader.GetString(1);
						string columnName = reader.GetString(2);
						string primaryKeyTableName = reader.GetString(3);
						
						string key = (schema + "." + tableName + "." + columnName).ToLower();
						pkTableLookup.Add(key, primaryKeyTableName);
					}
				}
				
				foreach(DbTable table in pDbTables){
				
					foreach(DbColumn column in table.Columns){
					
						string key = (table.SchemaName + "." + table.Name  + "." + column.Name).ToLower();
						
						if(pkTableLookup.ContainsKey(key)){
							column.ForeignKeyToTable = pkTableLookup[key];
						}
					}
				}
			}
		}
		
		private static Dictionary<string, System.Data.DbType> sTypeLookup;

		private static System.Data.DbType GetDataType(string pDataType) {

			if (sTypeLookup == null) {
				sTypeLookup = new Dictionary<string, System.Data.DbType>();

				sTypeLookup.Add("bigint", System.Data.DbType.Int64);
				sTypeLookup.Add("bit", System.Data.DbType.Boolean);
				sTypeLookup.Add("char", System.Data.DbType.String);
				sTypeLookup.Add("date", System.Data.DbType.Date);
				sTypeLookup.Add("time", System.Data.DbType.Time);
				sTypeLookup.Add("datetime", System.Data.DbType.DateTime);
				sTypeLookup.Add("datetime2", System.Data.DbType.DateTime2);
				sTypeLookup.Add("datetimeoffset", System.Data.DbType.DateTimeOffset);				
				sTypeLookup.Add("float", System.Data.DbType.Double);
				sTypeLookup.Add("real", System.Data.DbType.Single);
				sTypeLookup.Add("image", System.Data.DbType.Byte);
				sTypeLookup.Add("int", System.Data.DbType.Int32);
				sTypeLookup.Add("ntext", System.Data.DbType.String);
				sTypeLookup.Add("numeric", System.Data.DbType.Decimal);
				sTypeLookup.Add("decimal", System.Data.DbType.Decimal);
				sTypeLookup.Add("nvarchar", System.Data.DbType.String);
				sTypeLookup.Add("smallint", System.Data.DbType.Int16);
				sTypeLookup.Add("tinyint", System.Data.DbType.Byte);
				sTypeLookup.Add("uniqueidentifier", System.Data.DbType.Guid);
				sTypeLookup.Add("varbinary", System.Data.DbType.Binary);
				sTypeLookup.Add("varchar", System.Data.DbType.String);
			}
			string key = pDataType.ToLower();
			return sTypeLookup.ContainsKey(key) ? sTypeLookup[key] : System.Data.DbType.Object;
		}
	}
}