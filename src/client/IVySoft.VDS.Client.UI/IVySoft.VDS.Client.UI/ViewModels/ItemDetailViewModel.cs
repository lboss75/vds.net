using System;

using IVySoft.VDS.Client.UI.Models;

namespace IVySoft.VDS.Client.UI.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}
