using System;
using System.Collections.Generic;

internal class Simple : Rank
{
	private class Customer : ICustomer
	{
		public long Id { get; set; }
		public decimal Score { get; set; }
		public int Rank { get; set; }

		public static readonly Func<Customer, Customer, int> Comparer = (left, right) =>
		{
			var result = right.Score.CompareTo(left.Score);
			if (result != 0)
				return result;
			return left.Id.CompareTo(right.Id);
		};
	}
	private readonly Treap<Customer> treap = new(Customer.Comparer);
	private readonly List<Customer> list = new();
	private readonly Dictionary<long, Customer> customers = new();
	public override ICustomer[] Find(long id, int left, int right)
	{
		if (!customers.TryGetValue(id, out var customer))
			return Array.Empty<ICustomer>();
		return Range(customer.Rank - left, customer.Rank + right);
	}

	public override ICustomer[] Range(int start, int end)
	{
		start = Math.Max(start, 0);
		end = Math.Min(end, list.Count - 1);
		if (start > end)
			return Array.Empty<ICustomer>();
		var result = new ICustomer[end - start + 1];
		for (int i = start; i <= end; i++)
		{
			result[i - start] = list[i];
		}
		return result;
	}

	public override decimal Update(long id, decimal score)
	{
		int? end = null;
		if (customers.TryGetValue(id, out var customer))
		{
			end = customer.Rank;
			treap.Delete(customer);
		}
		else
		{
			customer = new Customer();
			customers.Add(id, customer);
			customer.Id = id;
			customer.Score = 0;
		}
		customer.Score += score;
		treap.Insert(customer);
		var previous = treap.Previous(customer);
		int start = previous == null ? -1 : previous.Rank;
		customer.Rank = start + 1;
		if (!end.HasValue)
		{
			list.Add(customer);
			end = list.Count - 1;
		}
		for (int i = end.Value; i > customer.Rank; i--)
		{
			list[i] = list[i - 1];
			list[i].Rank = i;
		}
		list[customer.Rank] = customer;
		return customer.Score;
	}
}
