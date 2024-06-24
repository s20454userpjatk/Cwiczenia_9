using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App_Medicament.Models;
using App_Medicament.Data;
using App_Medicament.DTO;
using System.Linq;
using System.Threading.Tasks;

namespace App_Medicament.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Prescription>> CreatePrescription(CreatePrescriptionRequest request)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId);
            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = request.Patient.FirstName,
                    LastName = request.Patient.LastName,
                    Birthdate = request.Patient.Birthdate
                };
                _context.Patients.Add(patient);
            }

            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
            {
                return BadRequest("Invalid doctor.");
            }

            if (request.Medicaments.Count > 10)
            {
                return BadRequest("Prescription cannot contain more than 10 medicaments.");
            }

            var prescription = new Prescription
            {
                Date = request.Date,
                DueDate = request.DueDate,
                Patient = patient,
                Doctor = doctor,
                PrescriptionMedicaments = new List<PrescriptionMedicament>()
            };

            foreach (var medicamentRequest in request.Medicaments)
            {
                var medicament = await _context.Medicaments.FindAsync(medicamentRequest.MedicamentId);
                if (medicament == null)
                {
                    return BadRequest($"Medicament with ID {medicamentRequest.MedicamentId} does not exist.");
                }
                prescription.PrescriptionMedicaments.Add(new PrescriptionMedicament
                {
                    Prescription = prescription,
                    Medicament = medicament,
                    MedicamentId = medicament.Id,
                    Dose = medicamentRequest.Dose
                });
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrescriptionById), new { id = prescription.Id }, prescription);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PrescriptionDto>> GetPrescriptionById(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionMedicaments)
                .ThenInclude(pm => pm.Medicament)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            var prescriptionDto = new PrescriptionDto
            {
                Id = prescription.Id,
                Date = prescription.Date,
                DueDate = prescription.DueDate,
                Doctor = new DoctorDto
                {
                    Id = prescription.Doctor.Id,
                    FirstName = prescription.Doctor.FirstName,
                    LastName = prescription.Doctor.LastName,
                    Email = prescription.Doctor.Email
                },
                Patient = new PatientDto
                {
                    Id = prescription.Patient.Id,
                    FirstName = prescription.Patient.FirstName,
                    LastName = prescription.Patient.LastName,
                    Birthdate = prescription.Patient.Birthdate
                },
                Medicaments = prescription.PrescriptionMedicaments.Select(pm => new MedicamentDto
                {
                    Id = pm.Medicament.Id,
                    Name = pm.Medicament.Name,
                    Type = pm.Medicament.Type,
                    Description = pm.Medicament.Description
                }).ToList()
            };

            return prescriptionDto;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<PatientPrescriptionsDto>> GetPatientPrescriptions(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.Doctor)
                .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.PrescriptionMedicaments)
                .ThenInclude(pm => pm.Medicament)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            var patientDto = new PatientPrescriptionsDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Birthdate = patient.Birthdate,
                Prescriptions = patient.Prescriptions.OrderBy(p => p.DueDate).Select(pr => new PrescriptionDto
                {
                    Id = pr.Id,
                    Date = pr.Date,
                    DueDate = pr.DueDate,
                    Doctor = new DoctorDto
                    {
                        Id = pr.Doctor.Id,
                        FirstName = pr.Doctor.FirstName,
                        LastName = pr.Doctor.LastName,
                        Email = pr.Doctor.Email
                    },
                    Patient = new PatientDto
                    {
                        Id = pr.Patient.Id,
                        FirstName = pr.Patient.FirstName,
                        LastName = pr.Patient.LastName,
                        Birthdate = pr.Patient.Birthdate
                    },
                    Medicaments = pr.PrescriptionMedicaments.Select(pm => new MedicamentDto
                    {
                        Id = pm.Medicament.Id,
                        Name = pm.Medicament.Name,
                        Type = pm.Medicament.Type,
                        Description = pm.Medicament.Description
                    }).ToList()
                }).ToList()
            };

            return patientDto;
        }
    }
}
