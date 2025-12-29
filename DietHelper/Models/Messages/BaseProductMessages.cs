using CommunityToolkit.Mvvm.Messaging.Messages;
using DietHelper.ViewModels.Products;

namespace DietHelper.Models.Messages
{
    public class AddBaseProductMessage : AsyncRequestMessage<BaseProductViewModel?> { }

    public class AddBaseProductClosedMessage(BaseProductViewModel selectedProduct)
    {
        public BaseProductViewModel SelectedProduct { get; } = selectedProduct;
    }
}
