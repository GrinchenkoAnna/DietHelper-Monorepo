using DietHelper.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Models.Messages
{
    public class AddProductClosedMessage(ProductViewModel selectedProduct)
    {
        public ProductViewModel SelectedProduct { get; } = selectedProduct;
    }
}
