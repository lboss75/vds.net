using System;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client.UI.Logic
{
    public class VdsService
    {
        public static string Server = "localhost:8050";
        public static VdsService Instance = new VdsService();

        private VdsApi api_;

        public event LoginRequiredDelegate OnLoginRequired;

        public VdsApi Api => this.api_;
        public Action<Exception> ErrorHandler { get; set; }

        public void OpenConnection()
        {
            this.api_ = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + Server + "/api/ws"
            });

            this.api_.ErrorHandler = this.Client_ErrorHandler;

            if(this.OnLoginRequired != null)
            {
                this.OnLoginRequired(this, new LoginRequiredEventArg());
            }
        }

        private void Client_ErrorHandler(Exception ex)
        {
            this.api_ = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + Server + "/api/ws"
            });

            this.api_.ErrorHandler = this.Client_ErrorHandler;
            this.ErrorHandler(ex);

        }

        public void Stop()
        {
            if (null != this.api_)
            {
                this.api_.Dispose();
                this.api_ = null;
            }
        }


    }
}
