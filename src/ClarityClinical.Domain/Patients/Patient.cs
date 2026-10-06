namespace ClarityClinical.Domain.Patients;

public sealed class Patient
{
    private readonly List<PatientCondition> _conditions = [];
    private readonly List<PatientAllergy> _allergies = [];
    private readonly List<PatientMedication> _medications = [];

    public Patient(Guid id, string givenName, string familyName, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(givenName))
        {
            throw new ArgumentException("Given name is required.", nameof(givenName));
        }

        if (string.IsNullOrWhiteSpace(familyName))
        {
            throw new ArgumentException("Family name is required.", nameof(familyName));
        }

        Id = id;
        GivenName = givenName.Trim();
        FamilyName = familyName.Trim();
        DateOfBirth = dateOfBirth;
    }

    public Guid Id { get; }
    public string GivenName { get; }
    public string FamilyName { get; }
    public string DisplayName => $"{GivenName} {FamilyName}";
    public DateOnly DateOfBirth { get; }
    public IReadOnlyCollection<PatientCondition> Conditions => _conditions.AsReadOnly();
    public IReadOnlyCollection<PatientAllergy> Allergies => _allergies.AsReadOnly();
    public IReadOnlyCollection<PatientMedication> Medications => _medications.AsReadOnly();

    public void AddCondition(PatientCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        _conditions.Add(condition);
    }

    public void AddAllergy(PatientAllergy allergy)
    {
        ArgumentNullException.ThrowIfNull(allergy);
        _allergies.Add(allergy);
    }

    public void AddMedication(PatientMedication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        _medications.Add(medication);
    }
}
