using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace www.linuxwochen.common.ui
{
    public partial class DayView : ContentView
    {
        public DayView()
        {
            InitializeComponent();
        }

        private void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Debug.Assert(true);
        }
    }
}
