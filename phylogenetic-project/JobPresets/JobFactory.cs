
namespace phylogenetic_project.JobPresents;

public class JobFactory
{
    private readonly Dictionary<string, Type> jobTypes = new();

    public JobFactory()
    {
        RegisterJobs();
    }

    private void RegisterJobs()
    {
        var jobInterface = typeof(IJobPreset);
        var jobClasses = jobInterface.Assembly.GetTypes()
            .Where(t => jobInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in jobClasses)
        {
            var jobIdProp = type.GetProperty("jobId", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var jobId = jobIdProp?.GetValue(null)?.ToString();

            if (jobId != null)
                jobTypes[jobId] = type;
        }
    }

    public IJobPreset Create(string jobId)
    {
        if (jobTypes.TryGetValue(jobId, out var type))
            return (IJobPreset)Activator.CreateInstance(type)!;

        throw new ArgumentException($"No job with id {jobId}");
    }
}
