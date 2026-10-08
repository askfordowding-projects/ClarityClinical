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
    public string GivenName { get; private set; }
    public string FamilyName { get; private set; }
    public string DisplayName => $"{GivenName} {FamilyName}";
    public DateOnly DateOfBirth { get; private set; }
    public IReadOnlyCollection<PatientCondition> Conditions => _conditions.AsReadOnly();
    public IReadOnlyCollection<PatientAllergy> Allergies => _allergies.AsReadOnly();
    public IReadOnlyCollection<PatientMedication> Medications => _medications.AsReadOnly();

    public void UpdateDemographics(string givenName, string familyName, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(givenName)) throw new ArgumentException("Given name is required.", nameof(givenName));
        if (string.IsNullOrWhiteSpace(familyName)) throw new ArgumentException("Family name is required.", nameof(familyName));
        GivenName = givenName.Trim();
        FamilyName = familyName.Trim();
        DateOfBirth = dateOfBirth;
    }

    public void ReplaceConditions(IEnumerable<PatientCondition> values)
    {
        _conditions.Clear();
        _conditions.AddRange(values);
    }

    public void ReplaceAllergies(IEnumerable<PatientAllergy> values)
    {
        _allergies.Clear();
        _allergies.AddRange(values);
    }

    public void ReplaceMedications(IEnumerable<PatientMedication> values)
    {
        _medications.Clear();
        _medications.AddRange(values);
    }

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
