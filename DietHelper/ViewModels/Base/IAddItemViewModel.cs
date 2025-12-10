namespace DietHelper.ViewModels.Base
{
    public interface IAddItemViewModel
    {
        string? ManualName { get; set; }
        double ManualCalories { get; set; }
        double ManualProtein { get; set; }
        double ManualFat { get; set; }
        double ManualCarbs { get; set; }
    }
}
