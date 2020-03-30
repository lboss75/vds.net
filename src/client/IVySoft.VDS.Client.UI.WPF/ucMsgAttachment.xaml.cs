using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IVySoft.VDS.Client.UI.WPF
{
    /// <summary>
    /// Interaction logic for ucMsgAttachment.xaml
    /// </summary>
    public partial class ucMsgAttachment : UserControl
    {
        public ucMsgAttachment()
        {
            InitializeComponent();
        }

        public new System.IO.FileInfo DataContext
        {
            get
            {
                return (System.IO.FileInfo)base.DataContext;
            }
            set
            {
                base.DataContext = value;
            }
        }
    }
}
