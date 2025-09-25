using Patients.Model.Entities;
using Patients.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patients.Repository
{
    internal class PatientRepository : IPatientRepository
    {
        public Task AddAsync(Patient patient)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Patient>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Patient?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Patient patient)
        {
            throw new NotImplementedException();
        }
    }
}
