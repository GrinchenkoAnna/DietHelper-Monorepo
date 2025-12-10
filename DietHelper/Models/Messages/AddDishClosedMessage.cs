using DietHelper.ViewModels.Dishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Models.Messages
{
    public class AddDishClosedMessage(DishViewModel selectedDish)
    {
        public DishViewModel SelectedDish { get; } = selectedDish;
    }
}
