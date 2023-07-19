using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCP.Common.Helpers
{
    public static class ByteArrayHelper
    {
        public static string ConvertToString(this byte[] data)
        {
            var res = new StringBuilder();
            foreach (var b in data)
            {
                res.Append(b.ToString("X2"));
            }
            res.Append(" (");
            foreach (var b in data)
            {
                if ((b >= 32) && (b < 127))
                    res.Append(Encoding.ASCII.GetString(new byte[] { b }));
                else res.Append(" ");
            }
            res.Append(")");
            return res.ToString();
        }
    }
}
