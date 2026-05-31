namespace RMS.Application.Common.Options;

public class DefaultStepIdsOptions
{
    public const string SectionName = "DefaultStepIds";

    public Guid TeacherStepId { get; set; }
    public Guid DvqlttStepId { get; set; }
    public Guid TttvStepId { get; set; }
    public Guid DvqlttReviewStepId { get; set; }
    public Guid KhcnHtqtStepId { get; set; }
    public Guid ReturnedStepId { get; set; }
}
