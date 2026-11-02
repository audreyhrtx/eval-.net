using Pipeline.Models;

namespace Pipeline;

public interface IPipelineScheduler
{
    HashSet<string> GetJobNames(PipelineModel pipeline);

    IEnumerable<JobModel> GetRemainingJobs(PipelineModel pipeline, HashSet<string> completedJobs);

    IDictionary<string, int> CountRemainingDependencies(
        IDictionary<string, int> remainingJobDependencies,
        HashSet<string> completedJobs);

    List<List<string>> GetSchedule(PipelineModel pipeline);
}
