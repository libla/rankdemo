namespace Test
{
	[TestClass]
	public class UnitTest
	{
		private readonly Rank rank = Rank.Create();
		private readonly Random random = new();
		private readonly List<long> keys = new();
		[TestInitialize]
		public void Initialize()
		{
			for (int i = 0; i < 1000; i++)
			{
				long id = (uint)random.Next();
				keys.Add(id << 32 | (uint)random.Next());
			}
		}

		[TestCleanup]
		public void Cleanup()
		{
		}

		[TestMethod("Correct")]
		public void MethodCorrect()
		{
			var rank = Rank.Create();
			var dict = new Dictionary<long, decimal>();
			for (var n = 0; n < 5; n++)
			{
				for (var i = 0; i < 1000; i++)
				{
					var id = keys[random.Next(keys.Count)];
					var value = (decimal)(random.Next(200000) * 0.01) - 1000;
					if (!dict.TryGetValue(id, out var old))
						old = 0;
					dict[id] = old + value;
					rank.Update(id, value);
				}
				var list = dict.ToList();
				list.Sort((left, right) =>
				{
					var result = right.Value.CompareTo(left.Value);
					if (result != 0)
						return result;
					return left.Key.CompareTo(right.Key);
				});
				foreach (var kv in dict)
				{
					Assert.AreEqual(kv.Value, rank.Update(kv.Key, 0));
				}
				for (var i = 0; i < list.Count; i++)
				{
					var kv = list[i];
					var result = rank.Range(i, i);
					Assert.IsTrue(result.Length == 1);
					Assert.AreEqual(kv.Key, result[0].Id);
				}
				for (var i = 0; i < list.Count; i++)
				{
					var kv = list[i];
					var result = rank.Find(kv.Key, 0, 0);
					Assert.IsTrue(result.Length == 1);
					Assert.AreEqual(i, result[0].Rank);
				}
			}
		}

		[TestMethod("PerformanceUpdate")]
		public void MethodPerformanceUpdate()
		{
			var rank = Rank.Create();
			for (var i = 0; i < 1000000; i++)
			{
				var id = keys[random.Next(keys.Count)];
				var value = (decimal)(random.Next(200000) * 0.01) - 1000;
				rank.Update(id, value);
			}
		}

		[TestMethod("PerformanceFind")]
		public void MethodPerformanceFind()
		{
			var rank = Rank.Create();
			for (var i = 0; i < 1000; i++)
			{
				var id = keys[random.Next(keys.Count)];
				var value = (decimal)(random.Next(200000) * 0.01) - 1000;
				rank.Update(id, value);
			}
			for (var i = 0; i < 1000000; i++)
			{
				var id = keys[random.Next(keys.Count)];
				rank.Find(id, 0, 0);
			}
		}

		[TestMethod("PerformanceRange")]
		public void MethodPerformanceRange()
		{
			var rank = Rank.Create();
			for (var i = 0; i < 1000; i++)
			{
				var id = keys[random.Next(keys.Count)];
				var value = (decimal)(random.Next(200000) * 0.01) - 1000;
				rank.Update(id, value);
			}
			for (var i = 0; i < 1000000; i++)
			{
				var index = random.Next(keys.Count);
				rank.Range(index, index);
			}
		}
	}
}