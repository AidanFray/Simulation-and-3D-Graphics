using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs.ACW
{
    public class Unit
    {
        public static float ConvertToCm(float value)
        {
            return value * 0.06f;
        }

        public static float ConvertToKgM(float value)
        {
            return value;
        }
    }
}
