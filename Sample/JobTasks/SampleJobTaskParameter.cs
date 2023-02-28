namespace Sample.JobTasks;

// The Parameter for the job, usually an model known to your API.
public class SampleJobTaskParameter
{
    public int InitialProgress { get; set; }
    public int CurrentProgress { get; set; }
}
