using System;
using Roblox.Libraries.Cursor;
using Roblox.Models;
using Xunit;
using Xunit.Abstractions;

namespace Roblox.Libraries.UnitTest.Cursor;

public class UnitTestCursor
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTestCursor(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CreateAndDecodeCursor()
    {
        var result = Roblox.Libraries.Cursor.Cursor.EncodeCursor(new CursorEntry()
        {
            direction = Direction.Forwards,
            previousStartId = 1,
            startId = 123,
            sort = Sort.Asc,
            sortColumn = "id",
        });
        _testOutputHelper.WriteLine("Generated cursor is {0}", result);

        var decodedResult = Roblox.Libraries.Cursor.Cursor.DecodeCursor(Sort.Asc, result);
        Assert.Equal(Sort.Asc, decodedResult.sort);
        Assert.Equal(123, decodedResult.startId);
        Assert.Equal(Direction.Forwards, decodedResult.direction);
    }
    
    [Fact]
    public void CreateCursorAndError()
    {
        var result = Roblox.Libraries.Cursor.Cursor.EncodeCursor(new CursorEntry()
        {
            direction = Direction.Forwards,
            previousStartId = 1,
            startId = 123,
            sort = Sort.Asc,
            sortColumn = "id",
        });
        _testOutputHelper.WriteLine("Generated cursor is {0}", result);

        Assert.Throws<BadCursorException>(() =>
        {
            // Bad sort
            Roblox.Libraries.Cursor.Cursor.DecodeCursor(Sort.Desc, result);
        });
        
        Assert.Throws<BadCursorException>(() =>
        {
            // Bad key
            var badKey = result;
            badKey = badKey.Substring(0, badKey.Length - 1);
            
            Roblox.Libraries.Cursor.Cursor.DecodeCursor(Sort.Asc, badKey);
        });
    }
}