using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic
{
    public class UIUtils
    {
        public static string GetErrorMessage(Exception ex, bool required = true)
        {
            var text = GetErrorMessageText(ex, required);
            if(null != text)
            {
                switch (text)
                {
                    case "Root block is not allowed":
                        return UIResources.RootBlockIsNotAllowed;
                }

            }
            return text;
        }
        public static string GetErrorMessageText(Exception ex, bool required = true)
        {
            if(ex is AggregateException)
            {
                var e = (AggregateException)ex;
                foreach(var x in e.InnerExceptions)
                {
                    if(!(x is AggregateException))
                    {
                        var msg = GetErrorMessageText(x, false);
                        if(null != msg)
                        {
                            return msg;
                        }
                    }
                }
                foreach (var x in e.InnerExceptions)
                {
                    if (x is AggregateException)
                    {
                        var msg = GetErrorMessageText(x, false);
                        if (null != msg)
                        {
                            return msg;
                        }
                    }
                }

                if (!required)
                {
                    return null;
                }
            }

            return ex.Message;
        }
    }
}
