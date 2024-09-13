internal class Treap<T>
{
	private class Node
	{
		public T Value;
		public Node? Left;
		public Node? Right;
		public readonly int Priority;

		public Node(Treap<T> treap, T value)
		{
			Value = value;
			Priority = treap.random.Next();
		}
	}
	private Node? root;
	private readonly Func<T, T, int> comparer;
	private readonly Random random = new();

	public Treap(Func<T, T, int> comparer)
	{
		this.comparer = comparer;
	}

	public void Insert(T value)
	{
		root = Insert(root, value);
	}

	private Node Insert(Node? node, T value)
	{
		if (node == null)
			return new Node(this, value);

		if (comparer(value, node.Value) < 0)
		{
			node.Left = Insert(node.Left, value);
			if (node.Left.Priority > node.Priority)
				node = Rotate(node, node.Left);
		}
		else
		{
			node.Right = Insert(node.Right, value);
			if (node.Right.Priority > node.Priority)
				node = Rotate(node, node.Right);
		}

		return node;
	}

	public void Delete(T value)
	{
		root = Delete(root, value);
	}

	private Node? Delete(Node? node, T value)
	{
		if (node == null)
			return null;

		var result = comparer(value, node.Value);
		if (result < 0)
		{
			node.Left = Delete(node.Left, value);
		}
		else if (result > 0)
		{
			node.Right = Delete(node.Right, value);
		}
		else
		{
			if (node.Left == null)
				return node.Right;

			if (node.Right == null)
				return node.Left;

			if (node.Left.Priority > node.Right.Priority)
			{
				node = Rotate(node, node.Left);
				node.Right = Delete(node.Right, value);
			}
			else
			{
				node = Rotate(node, node.Right);
				node.Left = Delete(node.Left, value);
			}
		}

		return node;
	}

	public bool Contains(T value)
	{
		Node? node = root;

		while (node != null)
		{
			var result = comparer(value, node.Value);
			if (result < 0)
				node = node.Left;
			else if (result > 0)
				node = node.Right;
			else
				return true;
		}

		return false;
	}

	public T? Previous(T value)
	{
		Node? previous = null;
		Node? node = root;

		while (node != null)
		{
			if (comparer(value, node.Value) > 0)
			{
				previous = node;
				node = node.Right;
			}
			else
			{
				node = node.Left;
			}
		}

		if (previous == null)
			return default;

		return previous.Value;
	}

	public T? Next(T value)
	{
		Node? next = null;
		Node? node = root;

		while (node != null)
		{
			if (comparer(value, node.Value) < 0)
			{
				next = node;
				node = node.Left;
			}
			else
			{
				node = node.Right;
			}
		}

		if (next == null)
			return default;

		return next.Value;
	}

	private static Node Rotate(Node node, Node child)
	{
		if (node.Left == child)
		{
			node.Left = child.Right;
			child.Right = node;
		}
		else
		{
			node.Right = child.Left;
			child.Left = node;
		}
		return child;
	}
}