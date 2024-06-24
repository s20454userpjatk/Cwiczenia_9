using System;
using System.Collections.Generic;

namespace App_Medicament.DTO;

public class CreatePrescriptionRequest
{
    public int PatientId { get; set; }
    public PatientDto Patient { get; set; }
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public List<MedicamentRequest> Medicaments { get; set; }
}

public class MedicamentRequest
{
    public int MedicamentId { get; set; }
    public int Dose { get; set; }
}