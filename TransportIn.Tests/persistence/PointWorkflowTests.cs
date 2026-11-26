using TransportIn.Services;

namespace TransportIn.Tests;

public class PointWorkflowTests
{
    [Theory]
    [InlineData(true, true, "Завантажується", "Завантажено")]
    [InlineData(true, false, "Завантажується", "Завантажено")]
    [InlineData(false, true, "Розвантажується", "Розвантажено")]
    [InlineData(false, false, "Розвантажується", "Розвантажено")]
    public void GetStages_ReturnsStepsInRouteOrder(
        bool isLoading, bool hasNextPoint, string processingStatus, string processedStatus)
    {
        var stages = PointWorkflow.GetStages(isLoading, hasNextPoint);

        string[] expectedStatuses = hasNextPoint
            ? ["Прибув", processingStatus, processedStatus, "Виїхав"]
            : ["Прибув", processingStatus, processedStatus];

        Assert.Equal(expectedStatuses, stages.Select(stage => stage.Status));
        Assert.Equal(hasNextPoint, stages[^1].IsDeparture);
        Assert.All(stages.Take(3), stage => Assert.False(stage.IsDeparture));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Нова")]
    [InlineData("Невідомий статус")]
    public void GetNextStageIndex_StartsAtArrivalForNewOrUnknownStatus(string? status)
    {
        var stages = PointWorkflow.GetStages(isLoading: true, hasNextPoint: true);

        Assert.Equal(0, PointWorkflow.GetNextStageIndex(stages, status));
    }

    [Theory]
    [InlineData(true, true, "Прибув", 1)]
    [InlineData(true, true, "Завантажується", 2)]
    [InlineData(true, true, "Завантажено", 3)]
    [InlineData(true, true, "Виїхав", 4)]
    [InlineData(false, false, "Прибув", 1)]
    [InlineData(false, false, "Розвантажується", 2)]
    [InlineData(false, false, "Розвантажено", 3)]
    public void GetNextStageIndex_AdvancesAfterConfirmedStep(
        bool isLoading, bool hasNextPoint, string status, int expectedIndex)
    {
        var stages = PointWorkflow.GetStages(isLoading, hasNextPoint);

        Assert.Equal(expectedIndex, PointWorkflow.GetNextStageIndex(stages, status));
    }
}
