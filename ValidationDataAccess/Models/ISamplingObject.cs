namespace ValidationDataAccess.Models
{
    public interface ISamplingObject
    {
        int Id { get; set; }
        string LabSampleIdentifier { get; set; }
        string PWSIdentifier { get; set; }
        string LabAccredidationIdentifier { get; set; }
        string Errors { get; set; }
    }
}
