﻿
/*
 * 
 * Copyright (C) 2009-2015 JFo.nz
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

namespace TypedQuery.Logic {

	public interface ITable {
		Sql.DatabaseType DatabaseType { get; }
		string TableName { get; }
		string Schema { get; }
		bool IsView { get; }
	}

	public interface ITableDetails {

		string TableName { get; }
		string Schema { get; }
		bool IsView { get; }
		string Description { get; }

		IPrimaryKey PrimaryKey { get; }
		IList<IForeignKey> ForeignKeys { get; }
		IList<IColumn> Columns { get; }
	}

	public interface IColumn {

		string ColumnName { get; }
		System.Data.DbType DbType { get; }
		bool IsNullable { get; }
		bool IsPrimaryKey { get; }
		bool IsAutoGenerated { get; }
		int? MaxLength { get; }
		string Description { get; }
	}

	public interface IPrimaryKey {

		string ContraintName { get; }
		IList<IColumn> Columms { get; }
	}

	public interface IForeignKey {

		string ConstraintName { get; }
		IList<IForeignKeyColumns> KeyColumns { get; }
	}
	public interface IForeignKeyColumns {

		IColumn ForeignKeyColumn { get; }
		string PrimaryKeyTableName { get; }
		string PrimaryKeyColumnName { get; }
	}

	public interface IStoredProcedureDetail {

		string Schema { get; }
		string Name { get; }
		List<ISpParameter> Parameters { get; }
	}

	public interface ISpParameter {

		int ParamId { get; }
		string Name { get; }
		System.Data.DbType ParamType { get; }
		System.Data.ParameterDirection Direction { get; }
	}

	public class Table : ITable {

		public Sql.DatabaseType DatabaseType { get; private set; }
		public string TableName { get; private set; }
		public string Schema { get; private set; }
		public bool IsView { get; private set; }

		public Table(Sql.DatabaseType pDatabaseType, string pTableName, string pSchema, bool pIsView) {
			DatabaseType = pDatabaseType;
			TableName = pTableName;
			Schema = pSchema;
			IsView = pIsView;
		}
	}

	public class TableDetails : ITableDetails {

		public string TableName { get; private set; }
		public string Schema { get; private set; }
		public bool IsView { get; private set; }
		public string Description { get; set; }

		public IPrimaryKey PrimaryKey { get; set; }
		public IList<IColumn> Columns { get; private set; }
		public IList<IForeignKey> ForeignKeys { get; private set; }

		public TableDetails(string pTableName, string pSchema, bool pIsView) {
			TableName = pTableName;
			Schema = pSchema;
			IsView = pIsView;
			Columns = new List<IColumn>();
			ForeignKeys = new List<IForeignKey>();
			Description = string.Empty;
		}
	}

	public class Column : IColumn {

		public string ColumnName { get; private set; }
		public System.Data.DbType DbType { get; private set; }
		public bool IsNullable { get; private set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsAutoGenerated { get; private set; }
		public string Description { get; set; }
		public int? MaxLength { get; private set; }

		public Column(string pColumnName, System.Data.DbType pDbType, bool pIsNullable, bool pIsAutoGenerated, int? pMaxLength) {
			ColumnName = pColumnName;
			DbType = pDbType;
			IsNullable = pIsNullable;
			IsAutoGenerated = pIsAutoGenerated;
			MaxLength = pMaxLength;
		}
	}

	public class PrimaryKey : IPrimaryKey {

		public string ContraintName { get; private set; }
		public IList<IColumn> Columms { get; private set; }

		public PrimaryKey(string pContraintName) {
			ContraintName = pContraintName;
			Columms = new List<IColumn>();
		}
	}

	public class ForeignKey : IForeignKey {

		public string ConstraintName { get; private set; }
		public IList<IForeignKeyColumns> KeyColumns { get; private set; }

		public ForeignKey(string pConstraintName) {
			ConstraintName = pConstraintName;
			KeyColumns = new List<IForeignKeyColumns>();
		}
	}

	public class KeyColumn : IForeignKeyColumns {

		public IColumn ForeignKeyColumn { get; private set; }
		public string PrimaryKeyTableName { get; private set; }
		public string PrimaryKeyColumnName { get; private set; }

		public KeyColumn(IColumn pForeignKeyColumn, string pPrimaryKeyTableName, string pPrimaryKeyColumnName) {

			ForeignKeyColumn = pForeignKeyColumn;
			PrimaryKeyTableName = pPrimaryKeyTableName;
			PrimaryKeyColumnName = pPrimaryKeyColumnName;
		}
	}

	public class StoredProcedureDetail : IStoredProcedureDetail {

		public string Schema { get; private set; }
		public string Name { get; private set; }

		public List<ISpParameter> Parameters { get; private set; }

		public StoredProcedureDetail(string pSchema, string pName) {
			Schema = pSchema;
			Name = pName;
			Parameters = new List<ISpParameter>();
		}

		public void AddParameter(ISpParameter pParameter) {
			Parameters.Add(pParameter);
		}
	}

	public class SpParameter : ISpParameter {

		public int ParamId { get; private set; }
		public string Name { get; private set; }
		public System.Data.DbType ParamType { get; private set; }
		public System.Data.ParameterDirection Direction { get; private set; }

		public SpParameter(int pParamId, string pName, System.Data.DbType pParamType, System.Data.ParameterDirection pDirection) {
			ParamId = pParamId;
			Name = pName;
			ParamType = pParamType;
			Direction = pDirection;
		}
	}
}