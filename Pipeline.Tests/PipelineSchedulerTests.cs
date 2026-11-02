using Pipeline;
using Pipeline.Models;

namespace Pipeline.Tests;

public sealed class PipelineSchedulerTests
{
    private readonly PipelineScheduler _scheduler = new();

    [Fact]
    public void GetSchedule_ReturnsExpectedGroups()
    {
        var pipeline = new PipelineModel
        {
            Jobs =
            [
                new() { Name = "Analyser" },
                new() { Name = "Compiler" },
                new() { Name = "Tester", Dependencies = ["Compiler"] },
                new() { Name = "Creer le paquet", Dependencies = ["Analyser", "Tester"] },
                new() { Name = "Publier", Dependencies = ["Creer le paquet"] }
            ]
        };

        var schedule = _scheduler.GetSchedule(pipeline);

        Assert.Equal(4, schedule.Count);
        Assert.Equal(["Analyser", "Compiler"], schedule[0]);
        Assert.Equal(["Tester"], schedule[1]);
        Assert.Equal(["Creer le paquet"], schedule[2]);
        Assert.Equal(["Publier"], schedule[3]);
    }

    [Fact]
    public void GetSchedule_WithOneJob_ReturnsOneGroup()
    {
        var pipeline = new PipelineModel
        {
            Jobs = [new JobModel { Name = "Compiler" }]
        };

        var schedule = _scheduler.GetSchedule(pipeline);

        Assert.Single(schedule);
        Assert.Equal(["Compiler"], schedule[0]);
    }

    [Fact]
    public void GetSchedule_WithChain_ReturnsOneJobPerGroup()
    {
        var pipeline = new PipelineModel
        {
            Jobs =
            [
                new() { Name = "A" },
                new() { Name = "B", Dependencies = ["A"] },
                new() { Name = "C", Dependencies = ["B"] }
            ]
        };

        var schedule = _scheduler.GetSchedule(pipeline);

        Assert.Equal([["A"], ["B"], ["C"]], schedule);
    }

    [Fact]
    public void GetSchedule_WithCircularDependencies_Throws()
    {
        var pipeline = new PipelineModel
        {
            Jobs =
            [
                new() { Name = "A", Dependencies = ["B"] },
                new() { Name = "B", Dependencies = ["A"] }
            ]
        };

        Assert.Throws<InvalidOperationException>(() => _scheduler.GetSchedule(pipeline));
    }

    [Fact]
    public void GetSchedule_WithEmptyPipeline_Throws()
    {
        Assert.Throws<ArgumentException>(() => _scheduler.GetSchedule(new PipelineModel()));
    }
}
