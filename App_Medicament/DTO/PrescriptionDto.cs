namespace App_Medicament.DTO;

public class PrescriptionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public DoctorDto Doctor { get; set; }
    
    public PatientDto Patient { get; set; }
    public List<MedicamentDto> Medicaments { get; set; }
}