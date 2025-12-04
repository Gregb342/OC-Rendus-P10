using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Pages
{
    public partial class NoteCreate
    {
        [Parameter] public int PatientId { get; set; }
        [Inject] private INoteService NoteService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private NoteCreateDto Note = new();
        private bool IsSubmitting;

        protected override void OnInitialized()
        {
            Note.PatientId = PatientId;
        }

        private async Task HandleValidSubmit()
        {
            IsSubmitting = true;

            try
            {
                await NoteService.CreateNoteAsync(Note);
                NavigationManager.NavigateTo($"/patients/details/{PatientId}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private void Annuler()
        {
            NavigationManager.NavigateTo($"/patients/details/{PatientId}");
        }
    }
}