using CommunityToolkit.Mvvm.Messaging.Messages;
using DietHelper.ViewModels.Dishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Models.Messages
{
    public class AddDishMessage : AsyncRequestMessage<DishViewModel?>
    {
    }
}
