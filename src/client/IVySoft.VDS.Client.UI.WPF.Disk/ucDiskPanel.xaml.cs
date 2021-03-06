﻿using IVySoft.VDS.Client.UI.Logic.Files;
using IVySoft.VDS.Client.UI.WPF.Common;
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

namespace IVySoft.VDS.Client.UI.WPF.Disk
{
    /// <summary>
    /// Interaction logic for ucDiskPanel.xaml
    /// </summary>
    public partial class ucDiskPanel : UserControl
    {
        public ucDiskPanel()
        {
            InitializeComponent();
        }

        public new FileListState DataContext
        {
            get
            {
                return (FileListState)base.DataContext;
            }
            set
            {
                base.DataContext = value;
            }
        }

        private void CurrentSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Refresh();
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(1 == FileListView.SelectedItems.Count)
            {
                var item = (IFileListItem)FileListView.SelectedItems[0];
                if (item.IsFolder)
                {
                    this.DataContext.Path = item.FullName;
                    this.Refresh();
                }
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var context = this.DataContext;
            ProgressWindow.Run("Update " + context.Path, Window.GetWindow(this), token => context.Refresh(token, x => Dispatcher.Invoke(x)));
        }
    }
}
