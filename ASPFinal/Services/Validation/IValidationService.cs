namespace ASPFinal.Services.Validation
{
    public interface IValidationService
    {
        bool Validate(string source, params ValidationTerms[] terms);
    }
}
