using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MrMarkov
{
    static class ControlExtensions
    {
        /// <summary>
        /// Extension method that allows for automatic anonymous method invocation.
        /// </summary>
        public static void Invoke(this Control c, MethodInvoker mi)
        {
            c.Invoke(mi);
        }
    }
}
