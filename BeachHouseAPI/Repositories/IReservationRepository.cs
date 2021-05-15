using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using BeachHouseAPI.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public interface IReservationRepository
    {
        IEnumerable<AvailableDatesSerializer> GetAvailableDates(AvailableDatesDTO value);
        Task<int> Reserve(string user_id, string requestor, ReservationDTO value);
        Task<int> Cancel(string user_id, string res_id);
        int EmailReminder(string key);
        int ValidateReport(string user_id, string startDate, string endDate, string mode);
        int ValidateUserReservation(string user_id, string requestor, string mode);
        IEnumerable<ReservationsReportDTO> ReservationsReport(string user_id, string startDate, string endDate, string mode);
        IEnumerable<ReservationsReportDTO> UserReservations(string user_id, string requestor, string mode);
        int DaysLeft(string requestor);

    }
}
