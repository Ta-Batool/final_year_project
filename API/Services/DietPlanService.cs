public DietPlan GeneratePlan(string userId, double maintenance, double weightKg, string goal)
{
    double targetCalories = goal switch
    {
        "Lose" => maintenance - 500,
        "Gain" => maintenance + 300,
        _ => maintenance
    };

    double protein = weightKg * 1.6;
    double fat = weightKg * 0.8;
    double proteinCalories = protein * 4;
    double fatCalories = fat * 9;
    double carbCalories = targetCalories - (proteinCalories + fatCalories);
    double carbs = carbCalories / 4;

    return new DietPlan
    {
        UserId = userId,
        TargetCalories = Math.Round(targetCalories),
        ProteinGrams = Math.Round(protein),
        FatGrams = Math.Round(fat),
        CarbGrams = Math.Round(carbs),
        CreatedAt = DateTime.UtcNow
    };
}
