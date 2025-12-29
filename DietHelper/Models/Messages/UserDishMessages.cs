using CommunityToolkit.Mvvm.Messaging.Messages;
using DietHelper.ViewModels.Dishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Models.Messages
{
    public class AddUserDishMessage : AsyncRequestMessage<UserDishViewModel?> { }

    public class AddUserDishClosedMessage(UserDishViewModel selectedDish)
    {
        public UserDishViewModel SelectedDish { get; } = selectedDish;
    }

    public class DeleteUserDishMessage : ValueChangedMessage<int>
    {
        public DeleteUserDishMessage(int value) : base(value) { }
    }
}
