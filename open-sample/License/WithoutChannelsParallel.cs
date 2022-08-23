using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace open_sample.License
{
    public class WithoutChannelsParallel
    {
        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
        public async Task RunWithoutChannelsAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using var reader = new StreamReader(File.OpenRead(LicenseStrings.InputFilePath));
            await using var writer = new StreamWriter(File.OpenWrite("../../../withoutchannelsParallel-output.txt"));
            string? url;

            List<Task> tasks = new();
            
            while ((url = await reader.ReadLineAsync()) != null)
            {
                url = url.Trim();
                                
                tasks.Add(Dadada(url, writer));                
            }

            await Task.WhenAll(tasks);


            stopwatch.Stop();
            Console.WriteLine("Without Channels Parallel ==> Completed in {0}", stopwatch.Elapsed);
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private async Task<int> Dadada(string url, StreamWriter writer)
        {
            var contents = await _httpClient.GetStringOrNullAsync(url);

            var licenseIdentifier = LicenseStrings.TryIdentify(contents ?? string.Empty);                        
            {
                await _semaphore.WaitAsync();
                await writer.WriteLineAsync($"{url};{licenseIdentifier}");
                _semaphore.Release();
            }

            return 0;
        }
    }
}