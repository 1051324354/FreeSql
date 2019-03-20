using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;

namespace FreeSql.Tests {
	public class UnitTest1 {

		public class Order {
			[Column(IsPrimary = true)]
			public int Id { get; set; }
			public string OrderTitle { get; set; }
			public string CustomerName { get; set; }
			public DateTime TransactionDate { get; set; }
			public virtual List<OrderDetail> OrderDetails { get; set; }
		}
		public class OrderDetail {
			[Column(IsPrimary = true)]
			public int Id { get; set; }

			public int OrderId { get; set; }
			public virtual Order Order { get; set; }
		}

		ISelect<TestInfo> select => g.mysql.Select<TestInfo>();


		class OrderContext : DbContext {

			public DbSet<Order> Orders { get; set; }
			public DbSet<OrderDetail> OrderDetails { get; set; }
		}

		[Fact]
		public void Test1() {

			using (var ctx = new OrderContext()) {
				ctx.Orders.Insert(new Order { }).ExecuteAffrows();
				ctx.Orders.Delete.Where(a => a.Id > 0).ExecuteAffrows();

				ctx.OrderDetails.Select.Where(dt => dt.Order.Id == 10).ToList();

				ctx.SaveChanges();
			}

				var parentSelect1 = select.Where(a => a.Type.Parent.Parent.Parent.Parent.Name == "").Where(b => b.Type.Name == "").ToSql();


			var collSelect1 = g.mysql.Select<Order>().Where(a =>
				a.OrderDetails.AsSelect().Any(b => b.Id > 100)
			);

			var collectionSelect = select.Where(a => 
				//a.Type.Guid == a.TypeGuid &&
				//a.Type.Parent.Id == a.Type.ParentId &&
				a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(
					//b => b.ParentId == a.Type.Parent.Id
				)
			);

			var collectionSelect2 = select.Where(a =>
				a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(
					b => b.Parent.Name == "xxx" && b.Parent.Parent.Name == "ccc"
					&& b.Parent.Parent.Parent.Types.AsSelect().Any(cb => cb.Name == "yyy")
				)
			);

			var collectionSelect3 = select.Where(a =>
				a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(
					bbb => bbb.Parent.Types.AsSelect().Where(lv2 => lv2.Name == bbb.Name + "111").Any(
					)
				)
			);



			var order = g.mysql.Select<Order>().Where(a => a.Id == 1).ToOne(); //��ѯ������
			if (order == null) {
				var orderId = g.mysql.Insert(new Order { }).ExecuteIdentity();
				order = g.mysql.Select<Order>(orderId).ToOne();
			}
			

			var orderDetail1 = order.OrderDetails; //��һ�η��ʣ���ѯ���ݿ�
			var orderDetail2 = order.OrderDetails; //�ڶ��η��ʣ�����
			var order1 = orderDetail1.FirstOrDefault(); //���ʵ������ԣ���ʱ�������ݿ⣬��Ϊ OrderDetails ��ѯ������ʱ��������˸�����


			var queryable = g.mysql.Queryable<TestInfo>().Where(a => a.Id == 1).ToList();
			
			var sql2222 = select.Where(a => 
				select.Where(b => b.Id == a.Id && 
					select.Where(c => c.Id == b.Id).Where(d => d.Id == a.Id).Where(e => e.Id == b.Id)
					.Offset(a.Id)
					.Any()
				).Any()
			).ToList();


			var groupby = g.mysql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.Where(a => a.Id == 1)
			)
			.GroupBy((a, b, c) => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
			.Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
			.Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
			.OrderBy(a => a.Key.tt2)
			.OrderByDescending(a => a.Count())
			.ToList(a => new { a.Key.tt2, cou1 = a.Count(), arg1 = a.Avg(a.Key.mod4),
				ccc2 = a.Key.tt2 ?? "now()",
				//ccc = Convert.ToDateTime("now()"), partby = Convert.ToDecimal("sum(num) over(PARTITION BY server_id,os,rid,chn order by id desc)")
			});
			
			var arrg = g.mysql.Select<TestInfo>().ToAggregate(a => new { sum = a.Sum(a.Key.Id + 11.11), avg = a.Avg(a.Key.Id), count = a.Count(), max = a.Max(a.Key.Id), min = a.Min(a.Key.Id) });

			var arrg222 = g.mysql.Select<NullAggreTestTable>().ToAggregate(a => new { sum = a.Sum(a.Key.Id + 11.11), avg = a.Avg(a.Key.Id), count = a.Count(), max = a.Max(a.Key.Id), min = a.Min(a.Key.Id) });

			var t1 = g.mysql.Select<TestInfo>().Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToList();
			var t2 = g.mysql.Select<TestInfo>().As("b").Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToList();


			var sql1 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).ToList();
			var sql2 = select.LeftJoin<TestTypeInfo>((a, b) => a.TypeGuid == b.Guid && b.Name == "111").ToList();
			var sql3 = select.LeftJoin("TestTypeInfo b on b.Guid = a.TypeGuid").ToList();

			//g.mysql.Select<TestInfo, TestTypeInfo, TestTypeParentInfo>().Join((a, b, c) => new Model.JoinResult3(
			//   Model.JoinType.LeftJoin, a.TypeGuid == b.Guid,
			//   Model.JoinType.InnerJoin, c.Id == b.ParentId && c.Name == "xxx")
			//);

			//var sql4 = select.From<TestTypeInfo, TestTypeParentInfo>((a, b, c) => new SelectFrom()
			//	.InnerJoin(a.TypeGuid == b.Guid)
			//	.LeftJoin(c.Id == b.ParentId)
			//	.Where(b.Name == "xxx"))
			//.Where(a => a.Id == 1).ToSql();

			var sql4 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.InnerJoin(a => a.TypeGuid == b.Guid)
				.LeftJoin(a => c.Id == b.ParentId)
				.Where(a => b.Name == "xxx")).ToList();
			//.Where(a => a.Id == 1).ToSql();

			
			var list111 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.InnerJoin(a => a.TypeGuid == b.Guid)
				.LeftJoin(a => c.Id == b.ParentId)
				.Where(a => b.Name != "xxx")).ToList((a, b, c) => new {
					a.Id,
					a.Title,
					a.Type,
					ccc = new { a.Id, a.Title },
					tp = a.Type,
					tp2 = new {
						a.Id,
						tp2 = a.Type.Name
					},
					tp3 = new {
						a.Id,
						tp33 = new {
							a.Id
						}
					}
				});

			var ttt122 = g.mysql.Select<TestTypeParentInfo>().Where(a => a.Id > 0).ToList();




			var sql5 = g.mysql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s).Where((a, b, c) => a.Id == b.ParentId).ToList();





			//((JoinType.LeftJoin, a.TypeGuid == b.Guid), (JoinType.InnerJoin, b.ParentId == c.Id)

			var t11112 = g.mysql.Select<TestInfo>().ToList(a => new {
				a.Id, a.Title, a.Type, 
				ccc = new { a.Id, a.Title },
				tp = a.Type,
				tp2 = new {
					a.Id, tp2 = a.Type.Name
				},
				tp3 = new {
					a.Id,
					tp33 = new {
						a.Id
					}
				}

			});

			var t100 = g.mysql.Select<TestInfo>().Where("").Where(a => a.Id > 0).Skip(100).Limit(200).Caching(50).ToList();
			var t101 = g.mysql.Select<TestInfo>().As("b").Where("").Where(a => a.Id > 0).Skip(100).Limit(200).Caching(50).ToList();


			var t1111 = g.mysql.Select<TestInfo>().ToList(a => new { a.Id, a.Title, a.Type });

			var t2222 = g.mysql.Select<TestInfo>().ToList(a => new { a.Id, a.Title, a.Type.Name });

			var t3 =  g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).IgnoreColumns(a => a.Title).ToSql();
			var t4 =  g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
			var t5 =  g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).IgnoreColumns(a => new { a.Title, a.TypeGuid, a.CreateTime }).ToSql();
			var t6 = g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).InsertColumns(a => new { a.Title }).ToSql();

			var t7 =  g.mysql.Update<TestInfo>().ToSql();
			var t8 =  g.mysql.Update<TestInfo>().Where(new TestInfo { }).ToSql();
			var t9 = g.mysql.Update<TestInfo>().Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).ToSql();
			var t10 =  g.mysql.Update<TestInfo>().Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).Where(a => a.Title == "111").ToSql();
			var t11 =  g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).ToSql();
			var t12 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Where(a => a.Title == "111").ToSql();

			var t13 =  g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").ToSql();
			var t14 =  g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").Where(new TestInfo { }).ToSql();
			var t15 =  g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).ToSql();
			var t16 =  g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).Where(a => a.Title == "111").ToSql();
			var t17 =  g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.Title, "222111").ToSql();
			var t18 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.Title, "222111").Where(a => a.Title == "111").ToSql();

			var t19 =  g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).ToSql();
			var t20 =  g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).Where(new TestInfo { }).ToSql();
			var t21 =  g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).ToSql();
			var t22 =  g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).Where(a => a.Title == "111").ToSql();
			var t23 =  g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.TypeGuid + 222111).ToSql();
			var t24 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.TypeGuid + 222111).Where(a => a.Title == "111").ToSql();

		}
	}
	class NullAggreTestTable {
		[Column(IsIdentity = true)]
		public int Id { get; set; }
	}


	[Table(Name = "xxx", SelectFilter = " a.id > 0")]
	class TestInfo {
		[Column(IsIdentity = true, IsPrimary = true)]
		public int Id { get; set; }
		public int TypeGuid { get; set; }
		public TestTypeInfo Type { get; set; }
		public string Title { get; set; }
		public DateTime CreateTime { get; set; }
	}

	class TestTypeInfo {
		[Column(IsIdentity = true)]
		public int Guid { get; set; }
		public int ParentId { get; set; }
		public TestTypeParentInfo Parent { get; set; }
		public string Name { get; set; }
	}

	class TestTypeParentInfo {
		public int Id { get; set; }
		public string Name { get; set; }

		public int ParentId { get; set; }
		public TestTypeParentInfo Parent { get; set; }

		public List<TestTypeInfo> Types { get; set; }
	}
}
