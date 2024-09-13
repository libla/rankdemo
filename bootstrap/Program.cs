using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Linq;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddAuthorization();


		var app = builder.Build();

		// Configure the HTTP request pipeline.

		app.UseAuthorization();

		var rank = Rank.Create();

		var options = new JsonSerializerOptions
		{
			WriteIndented = true,
			Converters = { new CustomerConverter() },
		};
		var cancel = new CancellationTokenSource();
		new Thread(() => {
			app.WaitForShutdown();
			cancel.Cancel();
		}).Start();
		var jobs = new BlockingCollection<Action>();
		var thread = new Thread(() =>
		{
			while (true)
			{
				Action job;
				try
				{
					job = jobs.Take(cancel.Token);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				job();
			}
		});
		thread.Start();

		app.MapPost("/customer/{id:long:required}/score/{score:decimal:required}", (long id, decimal score) =>
		{
			var builder = AsyncTaskMethodBuilder<decimal>.Create();
			jobs.Add(() => builder.SetResult(rank.Update(id, score)));
			return builder.Task;
		});
		app.MapGet("/leaderboard/{id:long:required}", (HttpContext context, long id) =>
		{
			if (!int.TryParse(context.Request.Query["high"], out var high))
				high = 0;
			if (!int.TryParse(context.Request.Query["low"], out var low))
				low = 0;
			var builder = AsyncTaskMethodBuilder.Create();
			jobs.Add(() =>
			{
				var result = rank.Find(id, high, low);
				var task = context.Response.WriteAsync(JsonSerializer.Serialize(result, options));
				task.GetAwaiter().UnsafeOnCompleted(() =>
				{
					if (task.Exception != null)
						builder.SetException(task.Exception);
					else
						builder.SetResult();

				});
			});
			return builder.Task;
		});
		app.MapGet("/leaderboard", (HttpContext context) =>
		{
			if (!int.TryParse(context.Request.Query["start"], out var start))
				start = 0;
			if (!int.TryParse(context.Request.Query["end"], out var end))
				end = int.MaxValue;
			var builder = AsyncTaskMethodBuilder.Create();
			jobs.Add(() =>
			{
				var result = rank.Range(start, end);
				var task = context.Response.WriteAsync(JsonSerializer.Serialize(result, options));
				task.GetAwaiter().UnsafeOnCompleted(() =>
				{
					if (task.Exception != null)
						builder.SetException(task.Exception);
					else
						builder.SetResult();

				});
			});
			return builder.Task;
		});

		app.Run();
		thread.Join();
	}

	private class CustomerConverter : JsonConverter<ICustomer>
	{
		public override ICustomer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, ICustomer value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("Customer ID", value.Id);
			writer.WriteNumber("Score", value.Score);
			writer.WriteNumber("Rank", value.Rank + 1);
			writer.WriteEndObject();
		}
	}
}