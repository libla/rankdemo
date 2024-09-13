public interface ICustomer
{
	long Id { get; }
	decimal Score { get; }
	int Rank { get; }
}

public abstract class Rank
{
	public abstract decimal Update(long customer, decimal score);
	public abstract ICustomer[] Range(int start, int end);
	public abstract ICustomer[] Find(long customer, int left, int right);

	public static Rank Create()
	{
		return new Simple();
	}
}