using Pipeline.Models;

namespace Pipeline;

public sealed class PipelineScheduler : IPipelineScheduler
{
    public HashSet<string> GetJobNames(PipelineModel pipeline)
    {
        ValidatePipeline(pipeline);
        return pipeline.Jobs.Select(job => job.Name).ToHashSet(StringComparer.Ordinal);
    }

    public IEnumerable<JobModel> GetRemainingJobs(
        PipelineModel pipeline,
        HashSet<string> completedJobs)
    {
        ValidatePipeline(pipeline);
        ArgumentNullException.ThrowIfNull(completedJobs);

        return pipeline.Jobs.Where(job => !completedJobs.Contains(job.Name));
    }

    public IDictionary<string, int> CountRemainingDependencies(
        IDictionary<string, int> remainingJobDependencies,
        HashSet<string> completedJobs)
    {
        ArgumentNullException.ThrowIfNull(remainingJobDependencies);
        ArgumentNullException.ThrowIfNull(completedJobs);

        return remainingJobDependencies
            .Where(item => !completedJobs.Contains(item.Key))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    }

    public List<List<string>> GetSchedule(PipelineModel pipeline)
    {
        ValidatePipeline(pipeline);

        var jobsByName = pipeline.Jobs.ToDictionary(job => job.Name, StringComparer.Ordinal);
        var completedJobs = new HashSet<string>(StringComparer.Ordinal);
        var schedule = new List<List<string>>();

        while (completedJobs.Count < jobsByName.Count)
        {
            var readyJobs = jobsByName.Values
                .Where(job =>
                    !completedJobs.Contains(job.Name) &&
                    job.Dependencies.All(completedJobs.Contains))
                .Select(job => job.Name)
                .ToList();

            if (readyJobs.Count == 0)
            {
                throw new InvalidOperationException(
                    "Les dependances du pipeline sont circulaires ou introuvables.");
            }

            schedule.Add(readyJobs);
            completedJobs.UnionWith(readyJobs);
        }

        return schedule;
    }

    private static void ValidatePipeline(PipelineModel pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(pipeline.Jobs);

        if (pipeline.Jobs.Count == 0)
        {
            throw new ArgumentException("Le pipeline doit contenir au moins une tache.", nameof(pipeline));
        }

        if (pipeline.Jobs.Any(job => job is null ||
                                     string.IsNullOrWhiteSpace(job.Name) ||
                                     job.Dependencies is null))
        {
            throw new ArgumentException("Chaque tache doit avoir un nom et une liste de dependances.", nameof(pipeline));
        }

        if (pipeline.Jobs.Select(job => job.Name).Distinct(StringComparer.Ordinal).Count() != pipeline.Jobs.Count)
        {
            throw new ArgumentException("Les noms des taches doivent etre uniques.", nameof(pipeline));
        }

        var names = pipeline.Jobs.Select(job => job.Name).ToHashSet(StringComparer.Ordinal);
        if (pipeline.Jobs.SelectMany(job => job.Dependencies).Any(dependency => !names.Contains(dependency)))
        {
            throw new InvalidOperationException("Une dependance reference une tache inexistante.");
        }
    }
}
