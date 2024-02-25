using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Roblox.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Roblox.Libraries.UnitTest;

public class AsyncLimitUnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public AsyncLimitUnitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Writer.OnLog((msg) =>
        {
            _testOutputHelper.WriteLine(msg);
            return 0;
        });
    }
    
    [Fact]
    public async Task CreateGroupWith10Entries()
    {
        var count = 0;
        var countMux = new Mutex();
        var asyncLimit = new AsyncLimit("UnitTest1", 10);
        for (var i = 0; i < 10; i++)
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await using var j = await asyncLimit.CreateAsync(TimeSpan.FromSeconds(5));
                lock (countMux)
                {
                    count++;
                }
            });
        }

        await Task.Delay(TimeSpan.FromMilliseconds(1500));
        Assert.Equal(10, count);
    }
    
    [Fact]
    public async Task CreateGroupWith100EntriesAndLimitOf10()
    {
        for (var x = 0; x < 1; x++)
        {
            var count = 0;
            var countMux = new Mutex();
            var asyncLimit = new AsyncLimit("UnitTest1", 10);
            var running = 0;
            var runningMux = new Mutex();
            // 1 second
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using var j = await asyncLimit.CreateAsync(TimeSpan.FromSeconds(5));
                    lock (runningMux)
                    {
                        running++;
                        Assert.True(running <= 10);
                    }

                    lock (countMux)
                    {
                        count++;
                    }
                    
                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                    lock (runningMux)
                    {
                        running--;
                        Assert.True(running <= 10);
                        Assert.True(running >= 0);
                    }
                }));
            }
            Assert.True(count <= 10);
            var attempts = 0;
            while (count < 100)
            {
                _testOutputHelper.WriteLine("Count is {0}", count);
                attempts++;
                if (attempts >= 20)
                    throw new Exception("Timeout - test should not take this long");
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                foreach (var t in tasks)
                {
                    Assert.Null(t.Exception);
                }
            }
            foreach (var t in tasks)
            {
                Assert.Null(t.Exception);
            }
            Assert.Equal(100, count);
        }
    }
}