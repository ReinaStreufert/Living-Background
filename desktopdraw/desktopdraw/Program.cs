using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace desktopdraw
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Run(new BackgroundForm(new FireCursorBackground()));
        }
    }
}
