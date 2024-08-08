using System.Collections.Concurrent;

namespace VertiClientTask_MapReducer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var data = new List<string>
        {
            "hello world",
            "hello from the other side",
            "world of C#",
            "hello from C#"
        };

            var timeout = TimeSpan.FromSeconds(2); // Define timeout
            var result = await MapReduceAsync(data, timeout);
            foreach (var kvp in result)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }


        static async Task<Dictionary<string, int>> MapReduceAsync(IEnumerable<string> data, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            var tasks = new List<Task<Dictionary<string, int>>>();

            // Map phase: create a task for each piece of data
            Console.WriteLine("Executing parallel map for each piece of collection.");
            foreach (var item in data)
            {
                tasks.Add(Task.Run(() => Map(item), cts.Token));
            }

            Console.WriteLine("Waiting for all tasks to complete or timeout.");
            // Wait for all tasks to complete or timeout
            var completedTasks = await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(timeout));

            if (completedTasks != Task.WhenAll(tasks))
            {
                // Timeout logic: cancel remaining tasks
                cts.Cancel();
                Console.WriteLine("Operation timed out.");
            }

            // Gather results from completed tasks
            var results = new Dictionary<string, int>();
            Console.WriteLine("Gathering results from completed tasks after map and summarize those.");
            foreach (var task in tasks)
            {
                try
                {
                    var mapResult = await task;
                    foreach (var kvp in mapResult)
                    {
                        if (results.ContainsKey(kvp.Key))
                        {
                            results[kvp.Key] += kvp.Value;
                        }
                        else
                        {
                            results[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("A task was cancelled.");
                }
            }

            return results;
        }

        static Dictionary<string, int> Map(string input)
        {
            Console.WriteLine("Mapping:Processes a single string to count occurrences of each word.");
            // Simulate processing time
            Thread.Sleep(500);

            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var wordCount = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (wordCount.ContainsKey(word))
                {
                    wordCount[word]++;
                }
                else
                {
                    wordCount[word] = 1;
                }
            }
            Console.WriteLine("Mapping complete.");
            return wordCount;
        }
    }

}

