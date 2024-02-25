using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class RobloxInfluxDb
{
    public static InfluxDBClient? client { get; set; }
    const string Bucket = "roblox-website-v2";
    const string Org = "Roblox";
    
    public static void Configure(string baseUrl, string websiteToken)
    {
        client = InfluxDBClientFactory.Create(baseUrl, websiteToken);
    }

    public static List<PointData> points { get; set; } = new();
    private static readonly Mutex PointsMutex = new();
    public static bool pointUploaderRunning { get; set; }

    public static void StartWriterTask()
    {
        lock (PointsMutex)
        {
            if (pointUploaderRunning || points.Count == 0)
                return;
            pointUploaderRunning = true;
        }

        Task.Run(async () =>
        {
            try
            {
                var len = points.Count;

                while (len != 0)
                {
                    for (var i = 0; i < len; i++)
                    {
                        await WritePointAsync(points[i]);
                    }

                    // cut out the points we uploaded
                    lock (PointsMutex)
                    {
                        points = points.Skip(len).ToList();
                        len = points.Count;
                    }
                }
            }
            catch (Exception e)
            {
                Writer.Info(LogGroup.Metrics, "could not upload points. error = {0}", e.Message);
            }
            finally
            {
                lock (PointsMutex)
                {
                    pointUploaderRunning = false;
                }
            }
        });
    }

    public static void WritePointInBackground(PointData point)
    {
        if (client == null)
            return;
        
        lock (PointsMutex)
        {
            points.Add(point);
        }

        StartWriterTask();
    }
    
    public static async Task WritePointAsync(PointData point)
    {
        if (client == null)
            return;
        var writeApi = client.GetWriteApiAsync();
        try
        {
            await writeApi.WritePointAsync(point, Bucket, Org);
        }
        catch (ArgumentException e)
        {
            // Strange bug. Inserts fine but gives this error that seems to be meaningless.
            if (e.Message != "An item with the same key has already been added. Key: Alt-Svc")
                throw;
        }
    }
    
    public static async Task WriteMeasurement(Measurement data)
    {
        if (client == null)
            return;
        var writeApi = client.GetWriteApiAsync();
        await writeApi.WriteMeasurementAsync(data, WritePrecision.Ms, Bucket, Org);
    }
}