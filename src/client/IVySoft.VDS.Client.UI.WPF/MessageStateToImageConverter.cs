using IVySoft.VDS.Client.UI.Logic.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace IVySoft.VDS.Client.UI.WPF
{
    public sealed class MessageStateToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var imagePath = "Images/";
                var state = (MessageState)value;
                switch (state)
                {
                    case MessageState.Draft:
                        imagePath += "msgStateDraft.png";
                        break;
                    case MessageState.Uploaded:
                        imagePath += "msgStateUploaded.png";
                        break;
                }

                Uri imageUri = new Uri(imagePath, UriKind.Relative);
                BitmapImage imageBitmap = new BitmapImage(imageUri);
                return imageBitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }
    }
}
