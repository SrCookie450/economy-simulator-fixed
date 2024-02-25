using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using Roblox.Models;

namespace Roblox.Libraries.Cursor
{

    public static class Cursor
    {
#if DEBUG==true
        private static string cursorForwards { get; set; } = "DEBUG";
        private static string cursorBackwards { get; set; } = "DEBUG";
#else
        private static string cursorForwards { get; set; } = Guid.NewGuid().ToString();
        private static string cursorBackwards { get; set; } = Guid.NewGuid().ToString();
#endif

        private static readonly HashAlgorithm CursorHashAlgo = MD5.Create();

        private static string GetKey(Sort direction)
        {
            return direction == Sort.Asc
                ? cursorBackwards
                : cursorForwards;
        }
        
        private static string CreateCursorFromString(string cursorStr, Sort direction)
        {
            var sigBytes = Encoding.UTF8.GetBytes(cursorStr + GetKey(direction));
            var md5Signature = CursorHashAlgo.ComputeHash(sigBytes);
            return cursorStr + "_" + Convert.ToHexString(md5Signature).ToLower();
        }
        
        public static string EncodeCursor(CursorEntry cursor)
        {
            var rawCursor = $"{cursor.startId}_{(int)cursor.direction}";
            return CreateCursorFromString(rawCursor, cursor.sort);
        }
        
        public static string EncodeCursor(long startId, Direction direction, Sort sort)
        {
            return EncodeCursor(new CursorEntry()
            {
                startId = startId,
                direction = direction,
                sort = sort,
            });
        }
        
        public static CursorEntry DecodeCursor(Sort sortMode, string? cursor)
        {
            if (string.IsNullOrEmpty(cursor))
                return new CursorEntry()
                {
                    startId = 0,
                    direction = Direction.Forwards,
                    sort = sortMode,
                };
            var split = cursor.Split("_");
            if (split.Length != 3) throw new BadCursorException();
            // re-create the signature
            var reCreationResult = CreateCursorFromString(split[0] + "_" + split[1], sortMode);
            if (reCreationResult != cursor)
                throw new BadCursorException();

            return new CursorEntry()
            {
                startId = long.Parse(split[0]),
                direction = Enum.Parse<Direction>(split[1]),
                sort = sortMode,
            };
        }
    }
}