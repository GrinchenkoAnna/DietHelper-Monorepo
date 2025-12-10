using DietHelper.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Interfaces.Core
{
    public interface INutritional
    {
        NutritionInfo NutritionFacts { get; }
    }
}
