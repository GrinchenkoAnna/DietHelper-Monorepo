using CommunityToolkit.Mvvm.Messaging.Messages;
using DietHelper.ViewModels.Products;

namespace DietHelper.Models.Messages
{
    public class AddUserProductMessage : AsyncRequestMessage<UserProductViewModel?> { }

    public class AddUserProductClosedMessage(UserProductViewModel selectedProduct)
    {
        public UserProductViewModel SelectedProduct { get; } = selectedProduct;
    }

    public class DeleteUserProductMessage : ValueChangedMessage<int>
    {
        public DeleteUserProductMessage(int value) : base(value) { }
    }
}
