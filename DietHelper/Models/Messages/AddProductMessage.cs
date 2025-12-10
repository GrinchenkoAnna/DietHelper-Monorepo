using CommunityToolkit.Mvvm.Messaging.Messages;
using DietHelper.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Models.Messages
{
    public class AddProductMessage : AsyncRequestMessage<ProductViewModel?>
    {
    }
}
