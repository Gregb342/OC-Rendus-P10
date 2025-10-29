namespace Patients_Frontend.Components.Pages
{
    public partial class Home
    {
        public class Patient
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Prenom { get; set; } = string.Empty;
            public DateTime DateNaissance { get; set; }
        }

        private List<Patient> Patients = new()
    {
        new Patient { Id = 1, Nom = "Dupont", Prenom = "Jean", DateNaissance = new DateTime(1980, 5, 15) },
        new Patient { Id = 2, Nom = "Martin", Prenom = "Marie", DateNaissance = new DateTime(1975, 8, 22) },
        new Patient { Id = 3, Nom = "Bernard", Prenom = "Pierre", DateNaissance = new DateTime(1990, 3, 10) },
        new Patient { Id = 4, Nom = "Dubois", Prenom = "Sophie", DateNaissance = new DateTime(1985, 12, 5) },
        new Patient { Id = 5, Nom = "Moreau", Prenom = "Antoine", DateNaissance = new DateTime(1970, 7, 18) }
    };

        private void EditerPatient(int patientId)
        {
            // Logique pour éditer le patient
            Console.WriteLine($"Édition du patient ID: {patientId}");
        }

        private void VoirPatient(int patientId)
        {
            // Logique pour voir les détails du patient
            Console.WriteLine($"Affichage du patient ID: {patientId}");
        }
    }
}