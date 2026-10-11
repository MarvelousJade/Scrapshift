using System;
using System.IO;

namespace Scrapshift
{
    public static class CaptureVerification
    {
        // A requested capture is not evidence until a fresh, nonempty file exists.
        public static bool Fresh(string folder, DateTime startedUtc, params string[] names)
        {
            foreach (string name in names)
            {
                var file = new FileInfo(Path.Combine(folder, name + ".png"));
                if (!file.Exists || file.Length < 8 || file.LastWriteTimeUtc < startedUtc) return false;
                using (var stream = file.OpenRead())
                {
                    byte[] png = {137,80,78,71,13,10,26,10};
                    foreach (byte value in png) if (stream.ReadByte() != value) return false;
                }
            }
            return true;
        }
    }
}
