namespace DietHelper.Common.DTO
{
    public class OpenFoodFactsDto
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbs { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
