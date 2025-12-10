using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Models.Messages
{
    public class DishDeleteMessage : ValueChangedMessage<int>
    {
        public DishDeleteMessage(int value) : base(value)
        {
        }
    }
}
