using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic
{
    public class UIUtils
    {
        public static string GetErrorMessage(Exception ex, bool required = true)
        {
            if(ex is AggregateException)
            {
                var e = (AggregateException)ex;
                foreach(var x in e.InnerExceptions)
                {
                    if(!(x is AggregateException))
                    {
                        var msg = GetErrorMessage(x, false);
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
                        var msg = GetErrorMessage(x, false);
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
