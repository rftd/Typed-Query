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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sql.Tests {

	[TestClass]
	public class DecimalTest {

		[TestInitialize()]
		public void Init() {

			using(Transaction transaction = new Transaction(DB.TestDB)) {

				Tables.DecimalTable.Table table = Tables.DecimalTable.Table.INSTANCE;

				Query.Delete(table).NoWhereCondition.Execute(transaction);

				transaction.Commit();
			}
		}

		private decimal mDecimal1 = 25.250m;

		[TestMethod]
		public void Test_01() {

			using(Transaction transaction = new Transaction(DB.TestDB)) {

				Tables.DecimalTable.Row row = new Sql.Tables.DecimalTable.Row();

				row.DecimalValue += mDecimal1;  //+= tests the default values on a new row
				row.Update(transaction);

				transaction.Commit();
			}

			Tables.DecimalTable.Table table = Tables.DecimalTable.Table.INSTANCE;

			IResult result = Query.Select(table.DecimalValue).From(table).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue == mDecimal1).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue != mDecimal1).Execute(DB.TestDB);
			Assert.AreEqual(0, result.Count);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue > 0).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue < 0).Execute(DB.TestDB);
			Assert.AreEqual(0, result.Count);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue <= mDecimal1).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue >= mDecimal1).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue <= (mDecimal1 - 1m)).Execute(DB.TestDB);
			Assert.AreEqual(0, result.Count);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue >= (mDecimal1 + 1m)).Execute(DB.TestDB);
			Assert.AreEqual(0, result.Count);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue.In(mDecimal1, 5m)).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue.NotIn(mDecimal1, 5m)).Execute(DB.TestDB);
			Assert.AreEqual(0, result.Count);

			System.Collections.Generic.List<decimal> list = new System.Collections.Generic.List<decimal>();
			list.Add(mDecimal1);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue.In(list)).Execute(DB.TestDB);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table.DecimalValue).From(table).Where(table.DecimalValue.NotIn(list)).Execute(DB.TestDB);
			Assert.AreEqual(0, result.Count);

			Tables.DecimalTable.Table table2 = new Sql.Tables.DecimalTable.Table();

			result = Query.Select(table, table2)
				.From(table)
				.Join(table2, table.DecimalValue == table2.DecimalValue)
				.Execute(DB.TestDB);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);
			Assert.AreEqual(mDecimal1, table2[0, result].DecimalValue);

			result = Query.Select(table, table2)
				.From(table)
				.Join(table2, table.DecimalValue != table2.DecimalValue)
				.Execute(DB.TestDB);

			Assert.AreEqual(0, result.Count);

			result = Query.Select(table, table2)
				.From(table)
				.Join(table2, table.DecimalValue >= table2.DecimalValue)
				.Execute(DB.TestDB);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);
			Assert.AreEqual(mDecimal1, table2[0, result].DecimalValue);

			result = Query.Select(table, table2)
				.From(table)
				.Join(table2, table.DecimalValue <= table2.DecimalValue)
				.Execute(DB.TestDB);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);
			Assert.AreEqual(mDecimal1, table2[0, result].DecimalValue);

			result = Query.Select(table, table2)
				.From(table)
				.Join(table2, table.DecimalValue < table2.DecimalValue)
				.Execute(DB.TestDB);

			Assert.AreEqual(0, result.Count);

			result = Query.Select(table, table2)
				.From(table)
				.Join(table2, table.DecimalValue > table2.DecimalValue)
				.Execute(DB.TestDB);

			Assert.AreEqual(0, result.Count);

			result = Query.Select(table)
				.From(table)
				.Where(table.DecimalValue.In(Query.Select(table2.DecimalValue).Distinct.From(table2)))
				.Execute(DB.TestDB);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(mDecimal1, table[0, result].DecimalValue);

			result = Query.Select(table)
				.From(table)
				.Where(table.DecimalValue.NotIn(Query.Select(table2.DecimalValue).Distinct.From(table2)))
				.Execute(DB.TestDB);

			Assert.AreEqual(0, result.Count);

			result = Query.Select(table)
				.From(table)
				.Where(table.DecimalValue + 5m > 2m)
				.Execute(DB.TestDB);

			return;
		}

		[TestMethod]
		public void ParametersTurnedOff() {

			Settings.UseParameters = false;

			try {
				Init();
				Test_01();
			}
			finally {
				Settings.UseParameters = true;
			}
		}

		[TestMethod]
		public void ParametersTurnedOn() {

			Settings.UseParameters = true;

			try {
				Init();
				Test_01();
			}
			finally {
				Settings.UseParameters = true;
			}
		}
	}
}
